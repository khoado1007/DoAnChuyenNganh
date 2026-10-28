# Agent enrollment and heartbeat — 2026-10-29

## Design and acceptance
Implement in the existing ASP.NET Core/Application/Domain/Infrastructure stack and PostgreSQL, with one Python client (real/simulated) and an Admin Agent screen. Work directly in the existing workspace as requested; preserve existing uncommitted work. Commands run from itam-codex-engineering. This folder contains instruction skills, not executable application code.

Chosen approach: opaque 256-bit credentials, SHA-256 hashes at rest, separate agent identity from user JWT and asset lifecycle. Alternatives: JWT agent credentials require additional revocation checks anyway; shared enrollment passwords cannot enforce one-use per asset. Admin creates a token bound to an existing asset and mode, valid 15 minutes. Enrollment consumes it atomically, with one agent per asset and one unique instance ID. Re-enrollment/replacement is outside this slice. Default heartbeat 30 seconds, offline after 90 seconds. Disabled agents cannot heartbeat; historical last telemetry remains. Protocol version 1, server UTC last-seen, monotonic sequence makes heartbeat retries idempotent. TLS required except loopback. No OS login, remote execution, or employee file access.

## Steps
1. Inspect/cleanup: preserve source, docs and plugin; no proven redundant files to delete. Add narrow ignore entries for Python caches/credentials. Write API integration tests and observe missing endpoint failures.
2. Backend: agent/token entities, EF configuration and migration; service with atomic enrollment/heartbeat/disable; thin controllers, Admin authorization, rate limits, input validation and OpenAPI. Minimal asset create/list supports enrollment without expanding assignment workflows.
3. Python: consent/config validation, separate credential files, real inventory via platform/psutil, labeled fixture, explicit enrollment without automatic retries, heartbeat retry/backoff/timeout/revocation/shutdown. Pytest regression tests.
4. UI: Admin Agent screen using live API, create asset/token, show mode/connectivity/policy/asset status separately, disable action, loading/errors. Existing demo screens remain labeled.
5. Verification: dotnet test ../backend/ITManagement.slnx with isolated PostgreSQL databases; python -m pytest ../agent/tests; npm --prefix ../frontend/admin test and build; migration pending-model check; real/simulated protocol E2E and review.

## Review focus
Concurrent reuse of a token; expired/wrong token; credential/user-auth separation; disable racing heartbeat; duplicate/reordered heartbeat; missing/deleted asset; no plaintext secrets in persistence or logs; consent and HTTPS; credential store not reused across server/mode; reconnect bounded; UI labels simulated inventory explicitly.

## Execution record
- Inspection: no enrollment/heartbeat implementation; frontend is a demo; .NET 10 available. Docker daemon initially stopped; Python command is a Store alias. Use installed bundled Python if suitable.
- No existing files deleted. No commit/push or changes to business database planned. Use dedicated test databases.
- Step 1 complete: plan + test-first API checks (missing endpoints returned 404). Preserved all pre-existing source/docs/plugin changes; added narrow Python artifact ignores.
- Step 2 complete: migration, service/controller, Admin RBAC, hashed one-use enrollment, credential-scoped heartbeat, sequence replay protection, server disable, rate limits and OpenAPI. Initial record validation metadata error reproduced and corrected on constructor parameters. Swagger anonymous security represented explicitly because empty security lists are omitted by the serializer.
- Step 3 complete: real/simulated Python client, consent/TLS validation, redacted logs, private atomic state, bounded retries, revocation and restart recovery. Independent review found persistent lock-file recovery bug; fixed with OS-held locks. Regression kills the actual Python process (Windows venv launcher otherwise leaves child alive).
- Step 4 complete: live Admin Agent page; minimal asset creation/token issuance, mode/connectivity/policy/lifecycle shown separately, disable, polling and errors. Existing unrelated demo screens retained.
- Verification: dotnet test ../backend/ITManagement.slnx --no-restore => 20/20 passed on isolated PostgreSQL port 55432. Includes concurrent enrollment, expired/wrong token, RBAC, replay/offline, deleted asset, disable race, rate limit, Swagger and existing regressions.
- Verification: ../agent/.venv/Scripts/python.exe -m pytest ../agent/tests -q => 13/13 passed.
- Verification: npm --prefix ../frontend/admin test => 3/3 passed; npm --prefix ../frontend/admin run build => passed (existing large JS bundle warning remains).
- Verification: EF has-pending-model-changes => none; migration only adds agents/enrollment-token tables and indexes; git diff --check => clean.
- Live smoke: Python real + simulated on one physical host, distinct instance IDs/credentials, heartbeat persisted, disabling simulated denied its heartbeat while real continued => passed.
- Browser smoke: actual API, create asset/token, mode labels, online state, disable, mobile width and API error => passed; repeatable script frontend/admin/tests/agents/smoke.cjs.
- Final review: independent reviewer found no other critical/important defects. No deferred minor findings from review. Existing EF query-filter relationship warnings and frontend bundle-size warning are outside this slice.
- Ruling: build/test must finish before starting API on Windows because running API locks output DLLs. Verification was rerun after stopping it.
- Ruling: keep existing workspace and edits rather than creating a second checkout; user asked execution from this folder. No commits or pushes.
- Limitations: latest inventory snapshot only, no per-heartbeat history; credential rotation/re-enrollment recovery UI not implemented; default per-IP rate limits need sizing for many agents behind NAT. No production deployment or business database migration performed.
