---
name: delivery-devops
description: Use when preparing Docker Compose, deployment, CI, backups, local developer setup or release checklists.
---

# Delivery and DevOps

Use one domain and one frontend deployment; route API under `/api`. Docker Compose for API, PostgreSQL and optional frontend reverse proxy; file storage via dedicated named volume. Python agents run on host as separate processes for demo (do not accidentally containerize the real agent if host telemetry is needed).

Keep DB credentials, admin bootstrap secrets, agent tokens outside Git. Provide `.env.example` with placeholders. Apply least-privilege database credentials, health checks, startup ordering with readiness, bounded logs and resource limits appropriate to a modest laptop. Avoid making Docker socket accessible to the app.

Back up PostgreSQL and file volume consistently; demonstrate restore on sample data. CI: formatting, type-check, lint, unit tests, integration tests, dependency checks; avoid expensive jobs by default. README must include reproducible start, agent real/simulated commands, shutdown, test, backup and rollback.
