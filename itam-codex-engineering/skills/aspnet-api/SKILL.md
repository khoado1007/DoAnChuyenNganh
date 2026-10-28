---
name: aspnet-api
description: Use when implementing ASP.NET Core REST APIs, EF Core PostgreSQL models, RBAC, enrollment, files, or tickets.
---

# ASP.NET Core API implementation

Use Controllers + application services + EF Core repositories/DbContext where needed; controllers stay thin. Prefer DTOs over exposing EF entities. Validate inputs, enforce server-side authorization policies for Admin, Employee, Technician, and distinguish user JWT/cookie credentials from agent credentials.

## Core endpoints (illustrative; adapt to repository)
- `POST /api/agents/enroll` consumes a short-lived single-use enrollment token, binds an agent credential to one asset, returns scoped credentials; hash token at rest.
- `POST /api/agents/heartbeat` authenticates agent and updates last-seen and inventory; use rate limits and idempotent updates.
- `GET /api/assets`, `POST /api/assets`, `POST /api/assets/{id}/assign`, `POST /api/assets/{id}/return`.
- `GET /api/files`, `POST /api/files`, `GET /api/files/{id}/download`, `DELETE /api/files/{id}` scoped to authenticated owner.
- `POST /api/tickets`, `GET /api/tickets`, `PATCH /api/tickets/{id}` with state transition rules.

## Safety and consistency
Never trust a client-supplied userId, deviceId or role as proof of authorization. File names are display metadata, not storage paths. Stream uploads to nonpublic volume using opaque IDs, reject traversal and unsafe file sizes, and authorize downloads. Enforce quota atomically under concurrency (reservation or transaction + locking), account for failed uploads and deletion, avoid trusting Content-Length alone. No plaintext passwords or agent secrets in logs. Database migrations are reviewed and reversible where practical. Keep allocation and audit history immutable.

## Done criteria
OpenAPI endpoint contracts; integration tests for forbidden roles, invalid enrollment, disabled device, wrong owner, quota races, lifecycle transitions and database persistence.
