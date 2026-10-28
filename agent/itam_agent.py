"""Opt-in inventory Agent; no remote commands or OS account operations."""
from __future__ import annotations

import argparse
from dataclasses import dataclass
import getpass
import ipaddress
import json
import os
from pathlib import Path
import platform
import random
import signal
import subprocess
import tempfile
import threading
import urllib.error
import urllib.parse
import urllib.request
import uuid


class ProtocolError(Exception):
    def __init__(self, status: int):
        self.status = status
        super().__init__(f"Server returned HTTP {status}")


@dataclass(frozen=True)
class Config:
    server: str
    mode: str
    state: Path
    consent: bool
    interval: int = 30

    def __post_init__(self):
        parsed = urllib.parse.urlsplit(self.server)
        try:
            local = ipaddress.ip_address(parsed.hostname or '').is_loopback
        except ValueError:
            local = parsed.hostname == 'localhost'
        if (not parsed.hostname or parsed.username or parsed.password or parsed.query or parsed.fragment
                or parsed.path not in ('', '/') or parsed.scheme not in ('http', 'https')
                or (parsed.scheme != 'https' and not local)):
            raise ValueError('Use an HTTPS origin; HTTP is allowed only on loopback.')
        if self.mode not in ('real', 'simulated') or not self.consent or not 10 <= self.interval <= 60:
            raise ValueError('Choose real/simulated, consent to inventory, and interval 10..60 seconds.')
        object.__setattr__(self, 'server', self.server.rstrip('/'))
        object.__setattr__(self, 'state', Path(self.state).resolve())


def load_inventory(config: Config) -> dict:
    if config.mode == 'simulated':
        values = json.loads((Path(__file__).parent / 'fixtures/simulated.json').read_text(encoding='utf-8-sig'))
    else:
        import psutil
        values = dict(hostname=platform.node() or 'unknown', os=platform.platform(),
                      cpu=platform.processor() or platform.machine() or 'unknown',
                      ramBytes=psutil.virtual_memory().total)
    if (set(values) != {'hostname', 'os', 'cpu', 'ramBytes'}
            or any(not isinstance(values[k], str) or not values[k].strip() or len(values[k]) > 200
                   for k in ('hostname', 'os', 'cpu'))
            or type(values['ramBytes']) is not int or values['ramBytes'] <= 0):
        raise ValueError('Inventory fixture does not match protocol version 1.')
    return dict(values, protocolVersion=1, agentVersion='1.0.0')


def retry_delay(failures: int, interval: int) -> float:
    return min(300, interval * 2 ** min(max(failures, 0), 5) * random.uniform(1, 1.25))


def save_private(path: Path, value: dict) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    descriptor, temporary = tempfile.mkstemp(prefix=path.name + '.', dir=path.parent)
    try:
        # Restrict access before writing credentials. Windows chmod alone cannot set ACLs.
        if os.name == 'nt':
            subprocess.run(['icacls', temporary, '/inheritance:r', '/grant:r', f'{getpass.getuser()}:(F)'],
                           check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                           creationflags=subprocess.CREATE_NO_WINDOW)
        else:
            os.chmod(temporary, 0o600)
        with os.fdopen(descriptor, 'w', encoding='utf-8') as stream:
            descriptor = -1
            json.dump(value, stream)
            stream.flush()
            os.fsync(stream.fileno())
        os.replace(temporary, path)
    finally:
        if descriptor != -1:
            os.close(descriptor)
        if os.path.exists(temporary):
            os.unlink(temporary)


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None  # Never forward enrollment tokens/credentials to a redirected origin.


class Agent:
    def __init__(self, config: Config, transport=None):
        self.config = config
        self.transport = transport or self._request
        if config.state.exists():
            self.state = json.loads(config.state.read_text(encoding='utf-8'))
            if self.state['server'] != config.server or self.state['mode'] != config.mode:
                raise ValueError('Credential store belongs to another server or mode; use a separate state file.')
        else:
            self.state = dict(server=config.server, mode=config.mode, instanceId=str(uuid.uuid4()),
                              sequence=0, credential=None, pending=None, revoked=False)
            self._save()

    def _save(self):
        save_private(self.config.state, self.state)

    def _request(self, path, body, credential):
        headers = {'Content-Type': 'application/json', 'Accept': 'application/json'}
        if credential:
            headers['Authorization'] = 'Agent ' + credential
        request = urllib.request.Request(self.config.server + path, json.dumps(body).encode(), headers, method='POST')
        try:
            with urllib.request.build_opener(NoRedirect()).open(request, timeout=15) as response:
                data = json.load(response)
        except urllib.error.HTTPError as error:
            raise ProtocolError(error.code) from None
        if not data.get('success') or not isinstance(data.get('data'), dict):
            raise ValueError('Unexpected server response.')
        return data['data']

    def enroll(self, token: str):
        if self.state['credential']:
            raise ValueError('Already enrolled; do not overwrite existing credentials.')
        # Never retry this non-idempotent operation automatically.
        result = self.transport('/api/agents/enroll', dict(token=token, instanceId=self.state['instanceId'],
                                mode=self.config.mode, protocolVersion=1), None)
        self.state.update(credential=result['credential'], agentId=result['agentId'])
        self._save()

    def heartbeat(self):
        if self.state['revoked'] or not self.state['credential']:
            raise ValueError('Agent is revoked or has not enrolled.')
        if self.state['pending'] is None:
            self.state['sequence'] += 1
            self.state['pending'] = dict(load_inventory(self.config), sequence=self.state['sequence'])
            self._save()  # Persist before sending: a restart repeats exactly the same request.
        try:
            response = self.transport('/api/agents/heartbeat', self.state['pending'], self.state['credential'])
        except ProtocolError as error:
            if error.status in (401, 403, 410):
                self.state['revoked'] = True
                self._save()
            raise
        self.state['pending'] = None
        self._save()
        return response

    def run(self, stop: threading.Event):
        failures = 0
        while not stop.is_set():
            try:
                self.heartbeat()
                failures = 0
                delay = self.config.interval
                print(json.dumps(dict(event='heartbeat_accepted', mode=self.config.mode)), flush=True)
            except ProtocolError as error:
                if error.status not in (408, 429) and error.status < 500:
                    print(json.dumps(dict(event='agent_stopped', status=error.status)), flush=True)
                    return 1
                delay = retry_delay(failures, self.config.interval)
                failures += 1
            except (OSError, TimeoutError):
                delay = retry_delay(failures, self.config.interval)
                failures += 1
            if failures:
                print(json.dumps(dict(event='heartbeat_retry', retrySeconds=round(delay))), flush=True)
            stop.wait(delay)
        return 0


def acquire_instance_lock(path: Path):
    stream = open(path, 'a+b')
    try:
        if os.fstat(stream.fileno()).st_size == 0:
            stream.write(b'\0')
            stream.flush()
        stream.seek(0)
        if os.name == 'nt':
            import msvcrt
            msvcrt.locking(stream.fileno(), msvcrt.LK_NBLCK, 1)
        else:
            import fcntl
            fcntl.flock(stream.fileno(), fcntl.LOCK_EX | fcntl.LOCK_NB)
        return stream  # Closing or process death releases the OS lock; retain the inode.
    except BaseException:
        stream.close()
        raise

def main():
    parser = argparse.ArgumentParser(description='Opt-in ITAM inventory: hostname, OS, CPU, RAM and Agent version only.')
    parser.add_argument('action', choices=['enroll', 'run', 'once'])
    parser.add_argument('--server', required=True)
    parser.add_argument('--mode', choices=['real', 'simulated'], required=True)
    parser.add_argument('--state', type=Path, required=True, help='Private state file; separate file for every instance')
    parser.add_argument('--consent-inventory', action='store_true', help='Consent to sending limited host inventory')
    parser.add_argument('--interval', type=int, default=30)
    args = parser.parse_args()
    lock = None
    try:
        config = Config(args.server, args.mode, args.state, args.consent_inventory, args.interval)
        config.state.parent.mkdir(parents=True, exist_ok=True)
        lock_path = config.state.with_suffix(config.state.suffix + '.lock')
        lock = acquire_instance_lock(lock_path)
        agent = Agent(config)
        if args.action == 'enroll':
            agent.enroll(getpass.getpass('One-use enrollment token: '))
            print(json.dumps(dict(event='enrolled', mode=config.mode, agentId=agent.state['agentId'])))
            return 0
        if args.action == 'once':
            agent.heartbeat()
            print(json.dumps(dict(event='heartbeat_accepted', mode=config.mode)))
            return 0
        stop = threading.Event()
        signal.signal(signal.SIGINT, lambda *_: stop.set())
        signal.signal(signal.SIGTERM, lambda *_: stop.set())
        return agent.run(stop)
    except (ValueError, KeyError, OSError, ProtocolError):
        # No exception bodies, requests, tokens or credentials in diagnostics.
        print(json.dumps(dict(event='agent_error', hint='Check configuration, server status and private state; enrollment is not retried.')))
        return 1
    finally:
        if lock is not None:
            lock.close()


if __name__ == '__main__':
    raise SystemExit(main())

