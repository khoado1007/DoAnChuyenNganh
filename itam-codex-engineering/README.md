# ITAM Codex Engineering

A skills-only Codex/ChatGPT plugin for the ITAM/ITSM Agent–Server student project. No external MCP service, no runtime credentials, no automatic code execution.

## Skills
- architecture-guardian: scope, entities, boundaries
- aspnet-api: API, PostgreSQL, RBAC, files and quota
- python-agent: real + simulated endpoint agents
- react-ui: role-specific UX
- quality-gates: automated tests and acceptance criteria
- delivery-devops: Compose, CI, backups and reproducible demos

## Usage
Install the plugin in Codex if supported. Copy `templates/AGENTS.md` into your repository root as `AGENTS.md` to make project rules available in repository-based Codex workflows. Skills do not substitute for project source files or tests.

Suggested prompt: `Inspect the repo and implement Agent enrollment plus heartbeat end-to-end, using architecture-guardian, aspnet-api, python-agent, quality-gates.`
