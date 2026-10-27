# MVC refactor — approved 2026-10-28

## Scope
ASP.NET Core Controllers -> Application service contracts -> Infrastructure services/repositories -> PostgreSQL. React remains the View. Keep current frameworks and UI style.

## Delivery
- Phase A (in progress): DTO contracts, persistence foundation, migrations/seed, real login/me API, frontend API client/login/layout.
- Phase B (pending): users, assets, tickets list/create API and forms.
- Phase C (pending): dashboard/quota queries, remove demo data, integration verification.

## Decisions / rulings
- Workspace is not a Git repository: work in place; no worktree/commit helpers apply.
- Approved design/plan is the five-part plan in this chat. This file preserves execution progress.
- Phase A preserves clearly labelled preview module content until Phase B/C replaces it; authentication itself uses the API.
- Independent frontend requests may run concurrently; queries sharing an EF DbContext stay sequential.
- No new business modules beyond approved scope; no deployment or production security work.

## Verification
- Baseline backend build passed (0 warnings/errors); frontend build passed (existing large-bundle warning).
- Docker CLI exists, but Docker Desktop Linux daemon is not running at first inspection.
- Started Docker Desktop and project PostgreSQL; no existing database deleted.
- RED: both FoundationTests failed (swallowed transaction failure; no cancellation). GREEN after async fixes.
- RED: auth endpoints returned 404 and seed quota was null. GREEN after controller/service/seed implementation.
- RED: concurrent sequence test returned 1 distinct number for 8 requests. GREEN with PostgreSQL sequence.
- Backend suite: 10/10 passed against isolated real PostgreSQL databases.
- Frontend API client: 3/3 tests passed. Frontend production build passed after extraction.
- Independent review: no Critical/Important defects. Minor deferred: automate React restore/retry/logout transitions; manual browser check in progress.
- Development browser exposed pre-existing missing caniuse-lite package/lock entry; restoring dependency through npm install (npm ci first rejected inconsistent lockfile).

## Additional rulings
- Ticket sequence is global and monotonic; year remains a display prefix. Gaps after rollback are valid. Cost: no annual sequence reset, which was not a requirement.
- Keep legacy MediatR/AutoMapper until Phase C cleanup, to keep this phase focused on a working auth slice.
- Remove fake quota/notification controls from the extracted layout until real endpoints exist; preview tables/dashboard stay explicitly labelled.

## Interface handoffs
- AuthController -> IAuthService: LoginAsync(LoginRequest, CancellationToken), GetCurrentUserAsync(CancellationToken).
- Frontend auth client consumes camelCase ApiResponse<AuthResponse/UserResponse>; role/status enum names are strings.
- Persistence uses one scoped AppDbContext per request and explicit async transactions for multi-step writes.
