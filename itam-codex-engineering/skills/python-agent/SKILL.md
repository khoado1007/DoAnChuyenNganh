---
name: python-agent
description: Use when building, debugging, packaging or testing the real and simulated Python endpoint agents.
---

# Python Agent engineering

## Shared protocol
One Python codebase with config `mode=real|simulated`; separate instance IDs and credential stores. `real` collects limited host inventory using psutil/platform; `simulated` reads a checked-in nonsecret fixture. Explicitly label simulated values and never pass them as genuine telemetry. Each instance enrolls separately and uses unique server-issued credential.

## Runtime behavior
- HTTPS for non-local deployments; certificate verification on; never disable TLS verification to fix a connection error.
- Heartbeat with bounded interval, exponential backoff + jitter, request timeouts, no tight loops.
- Retry only safe idempotent requests; handle 401/403/410 as disabled/revoked; stop authenticated actions and surface diagnostic.
- Report hostname, OS, CPU, RAM, app version and last-seen only with clear consent; avoid collecting documents, browser history or secrets.
- Do not create OS accounts, run remote shell commands, elevate privileges, or auto-install services by default.
- Handle graceful shutdown and use structured redacted logs.

## Test matrix
Unit-test config parsing, fixture loading, payload schema, offline/reconnect, invalid credentials, timeout, server disable, duplicated enrollment, version compatibility. Run real and simulated agents on one host with different IDs and confirm the dashboard labels them correctly.
