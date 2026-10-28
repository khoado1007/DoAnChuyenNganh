---
name: architecture-guardian
description: Use when planning features, changing domain boundaries, choosing data models, or reviewing ITAM/ITSM architecture.
---

# Architecture guardian

## Product contract
One deployed web application with role routes `/admin`, `/employee`, `/technician`; React + TypeScript + Vite frontend; ASP.NET Core Web API backend; PostgreSQL relational metadata; private file storage on Docker volume; Docker Compose. `/technician` is a limited future-ready ticket workflow, not a second administration system. Do not introduce MongoDB, Razor MVC views, domain controllers, VM requirements, osquery or remote shell without explicit approval.

Two Python agent instances share one protocol: **real** agent gathers host facts; **simulated** agent supplies clearly labeled synthetic host facts. Both may run on one physical PC; they do not prove two independent physical devices.

## Domain invariants
- Asset inventory, allocation history, agent enrollment, agent connectivity, employee identity and storage ownership are separate concerns.
- `online/offline` is computed from last heartbeat; `active/disabled` is an admin policy; `available/assigned/retired` is asset lifecycle. Never conflate them.
- Revocation must block new authorized operations server-side; do not promise instant offline agent shutdown.
- Account data belongs to the employee, not to an agent or asset. Agent identity must never authorize access to personal files by itself.
- Admin bootstrap is controlled; no public admin registration.

## Working method
Inspect repo and current tests before changes. State assumptions and acceptance criteria. Prefer one vertical slice with DB migration, service, API, UI, tests. Preserve existing conventions unless harmful. Produce concise change log, verification evidence and remaining risks. Never claim Windows/Linux OS login is implemented.
