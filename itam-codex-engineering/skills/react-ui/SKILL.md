---
name: react-ui
description: Use when building React TypeScript frontend screens, role routing, dashboard, file portal or ticket UI.
---

# React interface

Single React + TypeScript + Vite frontend with route groups `/admin`, `/employee`, `/technician`. Use route guards for UX only; backend owns authorization. Use consistent design tokens, solid colors (no gradients), accessible contrast and Georgia as primary display/text font if suitable; fall back gracefully.

Admin: asset inventory, enrollment token creation, assign/return, agent status and last seen, users, quota, audit, ticket overview. Employee: own profile, assigned assets, personal files, quota, create/view own tickets. Technician: assigned ticket queue and status updates; avoid privileged account creation.

Never present simulated device as real hardware. Device cards should show `Real` or `Simulated`, `Online/Offline`, and independent `Assigned/Available/Retired` state. No hardcoded Machine 1/Machine 2 selector; list comes from API. Handle loading, empty, error, forbidden, and revoked states. Use generated typed API clients if present; never store secrets in source or localStorage by default.

Verify responsive layout, keyboard navigation, Vietnamese text rendering, and UI tests for role boundaries and device changes.
