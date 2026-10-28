# Codex project instructions — ITAM/ITSM Agent–Server

Read the repository README and design docs before coding. Respect the current project scope: one React frontend with `/admin`, `/employee`, `/technician`, one ASP.NET Core Web API, PostgreSQL, private file storage, Docker Compose, Python real + simulated agents.

Do not replace the stack, introduce MongoDB, OS account management, VMs, osquery or remote execution without approval. Never equate simulated agents with independent physical computers.

For each task: inspect files -> summarize plan -> implement smallest safe change -> add focused tests -> run checks -> report modified files, commands and remaining risks.

Security invariants: server-side RBAC; authenticated agent enrollment; device status distinct from connectivity; employee files isolated by account; atomic quota enforcement; no plaintext credentials; no public file paths; immutable audit history.

Prefer Vietnamese for user-facing copy and explanations; code identifiers in clear English. UI typography: Georgia, solid colors, no gradients.
