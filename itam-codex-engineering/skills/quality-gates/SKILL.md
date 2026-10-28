---
name: quality-gates
description: Use when writing tests, reviewing changes, preparing demos or evaluating whether a feature is complete.
---

# Quality gates

Use risk-based tests: xUnit for domain services, ASP.NET integration tests with PostgreSQL test instance, pytest for Python agents, Vitest/React Testing Library for frontend, Playwright for end-to-end. Prefer deterministic fixtures and isolated test DB.

## Essential E2E
1. Bootstrap admin, create employee, asset and enrollment token.
2. Enroll real and simulated agents; dashboard shows two identities and correct modes.
3. Stop one agent: it becomes offline after threshold but is not automatically retired.
4. Admin disables one agent: subsequent authenticated operations denied; historical telemetry retained.
5. Assign and return asset; audit entries persist and employee permissions change appropriately.
6. Employee A uploads a file; access via own account succeeds regardless of simulated agent context; B cannot read it.
7. Concurrent uploads cannot exceed quota; delete updates accounting.
8. Employee ticket -> technician update -> employee sees resolution.

Always run available build/lint/tests and report commands + results. If not runnable, explain precisely what was not verified. Never fabricate test success.
