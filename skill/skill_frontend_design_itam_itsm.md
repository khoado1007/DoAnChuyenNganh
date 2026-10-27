---
description: Design and implement a consistent, enterprise-grade
  React/TypeScript frontend for the IT Asset Management and IT Service
  Management student project. Use this skill whenever creating, editing,
  reviewing, or extending frontend pages, components, layouts, styling,
  forms, tables, dashboards, ticket workflows, asset workflows, or
  knowledge search.
name: frontend-design-itam-itsm
---

# Frontend Design Skill --- ITAM / ITSM

## 1. Role and mission

You are the senior product designer and frontend engineer for a
small-business internal IT Asset Management (ITAM) and IT Service
Management (ITSM) web application.

Design an interface that feels like a practical, trustworthy enterprise
IT portal: calm, precise, information-dense where needed, and easy for
employees, IT technicians, and administrators to use. The product is a
student project MVP, not a marketing website and not a clone of a
commercial platform.

**Primary design constraints** - The primary typeface is **Georgia**. -
**Never use gradients**: no gradient backgrounds, gradient buttons,
gradient borders, or gradient text. - Prefer solid colors, restrained
borders, consistent spacing, and clear hierarchy. - Build reusable
design patterns instead of styling every page independently. - Implement
only the product scope supported by the project specification. Do not
invent extra business capabilities.

## 2. Product context

The application combines: - IT asset inventory and asset details. -
Asset assignment and return history. - Internal support tickets,
assignment, priority, status, comments, and status history. -
Knowledge-base articles and keyword search. - A research prototype
comparing keyword search with semantic search. - Basic dashboard
statistics. - Authentication and role-based access for Employee, IT
Technician, and Administrator.

The planned stack is React + TypeScript + Vite, with Ant Design as the
UI component library, and an ASP.NET Core REST API. Keep frontend
components compatible with this stack unless the repository has already
established a different implementation.

### User roles and interface priorities

**Employee** - Quickly see their assigned assets. - Create a support
request and track its progress. - Read and search troubleshooting
articles. - Understand ticket status without needing IT terminology
explained elsewhere.

**IT Technician** - Find tickets needing attention. - See priority,
status, requester, assignee, timestamps, and linked asset. - Update
ticket status, add comments, and use relevant knowledge articles. -
Avoid unnecessary clicks in repeated operational workflows.

**Administrator** - Manage asset records, categories, locations,
departments, accounts, and role access within the implemented scope. -
Inspect important records and histories. - Use dashboard summaries to
understand the state of assets and tickets.

Do not assume that a user can access a page or record simply because a
menu item is hidden. Backend authorization remains authoritative;
frontend role checks are for navigation and user experience only.

## 3. Visual direction

### Design keywords

**Editorial enterprise · restrained · structured · practical · readable
· trustworthy**

Use a subtle editorial character from Georgia typography, balanced by
conventional enterprise application patterns: compact navigation, clear
page titles, practical tables, aligned forms, and quiet status
indicators.

Avoid: - SaaS landing-page aesthetics. - Giant hero sections inside the
application. - Decorative illustrations that do not help complete a
task. - Excessive rounded cards, pills, shadows, or oversized
typography. - Glassmorphism, neon colors, animated backgrounds, and
gradients. - Random icon styles or emoji used as interface icons. - Fake
analytics, fabricated operational data, or decorative controls that do
nothing.

## 4. Design tokens

Create a single source of truth for tokens using CSS variables and/or
the existing theme configuration. Do not scatter arbitrary hex values
through components.

### Color palette

Use this palette as the default. If the existing repository has
established tokens, map them to these roles rather than creating a
competing palette.

  --------------------------------------------------------------------------
  Token                      Value                   Usage
  -------------------------- ----------------------- -----------------------
  `--color-app-bg`           `#F4F6F8`               Main application
                                                     background

  `--color-surface`          `#FFFFFF`               Tables, forms,
                                                     dropdowns, dialogs

  `--color-surface-subtle`   `#EEF2F5`               Secondary panels,
                                                     selected navigation
                                                     background

  `--color-primary`          `#17324D`               Primary actions, active
                                                     navigation, key
                                                     headings

  `--color-primary-hover`    `#244766`               Hover state for primary
                                                     actions

  `--color-accent`           `#0F766E`               Small emphasis, links
                                                     or secondary action
                                                     where appropriate

  `--color-text`             `#202B36`               Main body text

  `--color-text-muted`       `#627181`               Secondary text, labels,
                                                     timestamps

  `--color-border`           `#D7DEE5`               Dividers, input
                                                     borders, table
                                                     separators

  `--color-border-strong`    `#B8C4CF`               Focused or emphasized
                                                     boundaries

  `--color-focus`            `#2563EB`               Visible keyboard focus
                                                     ring

  `--color-danger`           `#B42318`               Destructive actions and
                                                     error states

  `--color-warning`          `#9A6700`               Warning states

  `--color-success`          `#18794E`               Successful completion

  `--color-info`             `#245EA8`               Informational states
  --------------------------------------------------------------------------

Use status colors with a pale, solid background and readable foreground.
Status must always include text; never rely on color alone.

Suggested semantic status mapping: - Ticket `Mới`: blue/info. -
`Đã phân công`: indigo/neutral-blue. - `Đang xử lý`: amber/warning. -
`Đã giải quyết`: green/success. - `Đã đóng`: neutral gray. - Asset
`Trong kho`: blue/info. - `Đang sử dụng`: green/success. -
Retired/unavailable asset states, if implemented: neutral or danger
according to meaning.

Do not use a different color for every category. Use the primary navy
for the main action, teal sparingly for supportive emphasis, and
semantic colors only for their intended meanings.

### Typography

-   Primary font stack: `Georgia, "Times New Roman", serif`.
-   Georgia is the default typeface across the interface, including
    navigation, headings, body copy, forms, tables, and buttons.
-   Use a monospace fallback only for code, raw identifiers, serial
    numbers, or technical log excerpts when it materially improves
    scanning.
-   Use weight and size to establish hierarchy; do not compensate for
    hierarchy with many colors.

Suggested scale: - Page title: 28--32 px, bold. - Section title: 20--22
px, bold. - Card/panel title: 16--18 px, bold. - Body and controls:
14--16 px. - Supporting text: 12--13 px, while remaining legible. -
Table content: usually 13--14 px; avoid shrinking to fit too many
columns.

Georgia has different metrics from common UI sans-serif fonts. Verify
line wrapping, button width, table density, and Vietnamese diacritics
after applying it. Do not silently switch the entire interface to
another font to solve spacing problems; adjust sizing and layout first.

### Spacing, shape, and elevation

Use a consistent 4 px base spacing system: - 4 px: icon/text
micro-gap. - 8 px: compact internal gap. - 12 px: form/control gap. - 16
px: standard component spacing. - 24 px: section spacing. - 32 px: major
page spacing.

Shape: - Inputs and buttons: 6--8 px radius. - Small panels: 8--10 px
radius. - Larger containers: 10--12 px radius. - Pills only for status
tags or compact filters when they improve scanning.

Borders should do most of the separation work. Use shadows sparingly for
overlays, dropdowns, and dialogs---not every card. Avoid excessive
nested cards.

## 5. Application shell

Use a consistent shell across authenticated pages: 1. A left sidebar for
primary navigation. 2. A compact top bar for page context, global search
if implemented, notifications if implemented, and user/account controls.
3. A main content region with a predictable max-width or full-width
layout appropriate to the page. 4. Breadcrumbs on deeper pages where
they help orientation. 5. A consistent page heading row: title and
concise description on the left; one primary action on the right when
relevant.

### Sidebar navigation

Group the navigation by the user's actual tasks: - **Overview**:
Dashboard. - **Service desk**: Tickets / My requests. - **Assets**:
Asset inventory; Assignments/history if implemented as a separate
page. - **Knowledge**: Knowledge base / Search. - **Administration**:
Users, categories, locations, departments, and settings only when
implemented and authorized.

Use simple, consistent line icons from one icon library already present
in the project. Do not use emoji as navigation icons. Show a clear
active state using a solid pale background and a left border or strong
text; do not use gradients.

The sidebar must be responsive. On narrow screens, collapse it into a
drawer or compact navigation pattern. Do not let it permanently consume
most of the viewport.

## 6. Page-level design requirements

### 6.1 Dashboard

Purpose: provide a useful operational summary, not a decorative
analytics wall.

Include only metrics supported by real API data, such as: - Total assets
and counts by asset status. - Tickets by status. - Tickets by
priority. - Recently created or recently updated tickets. - Useful
shortcuts such as "Create ticket" or "Add asset" according to role.

Rules: - Each metric needs a clear label and value; add a comparison
only when the backend provides a valid comparison. - Prefer a small
number of clear metric cards and one or two meaningful tables/charts. -
Charts must have titles, labels, legends where needed, and accessible
textual context. - Provide loading, empty, error, and partial-data
states. - Never fabricate production-looking statistics. If using mock
data for a prototype, label it as sample/demo data.

### 6.2 Asset inventory

The inventory is a data-management screen. Prioritize finding and acting
on records.

Recommended structure: - Page title and "Add asset" action for
authorized roles. - Search by asset code/name/serial number where
supported. - Filters for category, status, location, and assigned user
only when the API supports them. - A table with the most useful columns,
such as asset code, asset name, category, status, location, assigned
user, and warranty end. - Row actions in a consistent action menu or
compact action area. - Pagination and visible result count when
supported. - A clear empty state with an appropriate next step.

Avoid placing every possible field in the table. Put detailed attributes
on an asset detail page or drawer. Use explicit confirmation for
destructive or irreversible actions.

### 6.3 Asset details and assignment history

The detail view should separate: - Core asset information. - Current
assignment, if any. - Warranty and location information. -
Assignment/return history.

History must read as a chronological record. Show dates, recipient,
handler, condition at handover/return when available. Do not overwrite
historical events visually or imply that old assignments disappeared
when an asset changes owner.

Assignment and return forms must explain what will change, validate
required fields, and show success/failure feedback. Do not let a user
accidentally create multiple active assignments if the business rule
allows only one current assignment.

### 6.4 Tickets / service desk

Ticket list: - Make ticket code, title, status, priority, requester,
assignee, linked asset, and last update easy to scan. - Provide
practical filters for status, priority, assignee, and search where
supported. - Make status and priority visually distinct but
restrained. - Keep row actions predictable and keyboard accessible.

Ticket detail: - Show ticket code and title prominently. - Show status,
priority, requester, assignee, creation/update timestamps, and linked
asset in a compact summary. - Present the description and
conversation/comments in a readable main column. - Put assignment and
status controls in a clear side panel or action area on wide screens. -
Display status history as a chronological timeline or audit list. - Keep
adding a comment distinct from changing status or resolving/closing a
ticket. - Confirm consequential actions and communicate the result.

Use the project's defined workflow as the source of truth:
`Mới → Đã phân công → Đang xử lý → Đã giải quyết → Đã đóng`

If reopening is implemented, make it explicit. Do not offer invalid
transitions just because a button is easy to add; backend rules are
authoritative.

Ticket creation: - Keep the form focused: title, description, category
if implemented, priority, and linked asset if relevant. - Explain
required fields inline. - Preserve valid form input when a server error
occurs. - Avoid an unnecessarily long single-column form if a two-column
layout is more usable on desktop; collapse to one column on mobile.

### 6.5 Knowledge base

Build the knowledge base for finding and reading troubleshooting
instructions quickly.

Article list/search results should include: - Article title. - Short
excerpt or summary. - Category and keywords where available. - Updated
date where useful.

Article detail should use a readable article width, clear headings,
numbered steps for procedures, and visually distinct warnings or
prerequisites. Do not style an article like a dense admin table.

### 6.6 Keyword search and semantic-search experiment

The research module compares keyword retrieval with semantic retrieval.
The UI must not claim semantic search is better before the experiment
establishes that.

If the prototype exposes method selection: - Label methods clearly, for
example "Tìm theo từ khóa" and "Tìm kiếm ngữ nghĩa". - Keep the query
and document set consistent when comparing methods. - Show result title,
relevant excerpt, and a link to the article. - If experiment metadata is
available, display method, latency, rank, and relevance labels
clearly. - Avoid presenting a similarity score as a probability of
correctness. - Do not fabricate evaluation metrics or imply research
results that have not been measured. - Make loading, no-results, and
API-error states explicit.

The comparison screen should help a reviewer understand the experiment,
not merely look futuristic.

### 6.7 Administration

Use standard management patterns: - Table/list view. - Search and
supported filters. - Clear create/edit form. - Validation and
save/cancel actions. - Confirmation for destructive actions. - Clear
success/error feedback.

Only show administration pages and actions to appropriate roles. Hiding
the menu is not security enforcement.

## 7. Reusable component patterns

Create shared components where useful, such as: - `AppLayout`,
`Sidebar`, `Topbar`, `PageHeader`, `Breadcrumbs`. - `StatusTag`,
`PriorityTag`. - `DataTable` wrapper with consistent loading/empty/error
states. - `SearchFilterBar`. - `ConfirmAction`. - `FormSection`,
`FormActions`. - `EmptyState`, `ErrorState`, `LoadingState`. -
`TicketTimeline`, `AssignmentHistory`. - `MetricCard` for real dashboard
metrics.

Do not create abstractions for every tiny element. Reuse patterns when
there are at least two meaningful use cases or when consistency is
important.

### Tables

-   Align headers and values consistently.
-   Use a subtle header surface and clear row separators.
-   Avoid heavy borders around every cell.
-   Provide hover feedback without making rows look like buttons unless
    the row is actually clickable.
-   Keep actions visible and predictable.
-   Support horizontal overflow or responsive column prioritization on
    smaller screens.
-   Use pagination for long lists; do not render a huge inventory all at
    once.
-   Provide meaningful empty states and loading skeletons or indicators.
-   Ensure status tags and action icons have accessible text or labels.

### Forms

-   Every field needs a visible label; placeholders are examples, not
    labels.
-   Show required/optional state consistently.
-   Validate close to the relevant field.
-   Explain how to fix errors in plain language.
-   Preserve entered data after recoverable failures.
-   Distinguish primary submit from cancel.
-   Prevent duplicate submissions while a request is in progress.
-   Do not clear a form until the operation has succeeded or the user
    explicitly discards it.

### Dialogs and notifications

-   Use dialogs for focused decisions or short forms, not as a
    substitute for a full detail page.
-   Destructive confirmations must name the affected record and explain
    the consequence.
-   Use success notifications for completed operations and inline errors
    for field-specific issues.
-   Never show a success message before the API confirms success.

## 8. Interaction and accessibility

Target WCAG AA contrast wherever practical. At minimum: - Text and
controls must have sufficient contrast. - Keyboard focus must be
visible. - Every interactive control must be keyboard reachable and have
a clear accessible name. - Do not use color alone to communicate status,
error, or priority. - Use semantic headings in a logical order. -
Associate form labels and error messages with their fields. - Give
icon-only buttons accessible labels and tooltips where useful. - Respect
reduced-motion preferences; avoid unnecessary animation. - Ensure
dialogs manage focus and can be dismissed in a predictable way. -
Support Vietnamese text and diacritics correctly.

Interaction states must be designed deliberately: - Default. - Hover. -
Focus. - Active/selected. - Disabled. - Loading. - Empty. - Error. -
Success.

Use motion only to communicate state changes. Keep transitions short and
subtle; do not add motion merely for decoration.

## 9. Responsive behavior

Design desktop-first because asset tables and ticket workflows benefit
from width, but do not assume every user has a large monitor.

-   Wide desktop: persistent sidebar and spacious main content.
-   Laptop: compact sidebar and efficient content spacing.
-   Tablet: collapsible navigation; reduce low-value columns.
-   Mobile: single-column forms, drawer navigation, stacked detail
    sections, and tables that allow horizontal scroll or switch to
    record cards where appropriate.

Do not simply scale desktop content down. Preserve primary actions and
essential record information at every viewport.

## 10. React, TypeScript, and Ant Design implementation rules

-   Inspect the repository before changing its structure, dependencies,
    routes, or design system.
-   Reuse the installed Ant Design version and the project's existing
    routing/data-fetching patterns.
-   Centralize theme tokens and component overrides.
-   Apply Georgia through a global font rule and Ant Design
    theme/component tokens so built-in controls do not unexpectedly fall
    back to another font.
-   Verify popovers, dropdowns, modals, tables, and date pickers,
    because some UI libraries render overlays outside the normal
    component subtree.
-   Prefer small, typed, reusable components over a single giant page
    component.
-   Use explicit TypeScript types for API responses, form values, roles,
    statuses, and priorities.
-   Keep API/data logic separate from presentational components where
    reasonable.
-   Do not hardcode secrets or credentials.
-   Do not treat client-side role checks as authorization.
-   Do not add a new component library if Ant Design already meets the
    need.
-   Do not introduce a new state-management or data-fetching library
    without a concrete requirement.
-   Do not replace real API data with hardcoded arrays to make a screen
    look finished.
-   If an endpoint is not ready, isolate mock data behind a clearly
    named mock/service layer and label the UI as demo data.
-   Include error handling for failed requests and prevent duplicate
    mutations.
-   Use the existing linting, formatting, and testing tools.
-   Do not refactor unrelated code just to apply visual changes.

### Ant Design theming

Configure a centralized theme rather than styling components ad hoc. Map
the palette to Ant Design tokens such as primary color, text colors,
background/container colors, border color, border radius, font family,
and control height.

Use component-level overrides only where necessary, especially for: -
Buttons. - Inputs and selects. - Tables. - Menus. - Cards. -
Modal/drawer. - Tabs. - Tags/badges.

Verify the rendered UI after applying the theme. A token configuration
alone is not proof that every nested or portal-rendered element uses
Georgia and the intended colors.

## 11. Content and localization

-   Use Vietnamese for user-facing labels by default unless the product
    requirements specify otherwise.
-   Keep domain terminology consistent: "Tài sản", "Cấp phát", "Thu
    hồi", "Yêu cầu hỗ trợ", "Kỹ thuật viên", "Kho tri thức", "Mức ưu
    tiên", "Trạng thái".
-   Use concise, action-oriented button labels such as "Tạo yêu cầu",
    "Thêm tài sản", "Lưu thay đổi", and "Hủy".
-   Prefer understandable error messages over raw stack traces or
    unexplained HTTP codes.
-   Use consistent date/time formats throughout the application.
-   Do not mix English and Vietnamese arbitrarily. Technical identifiers
    may remain in English when appropriate.

## 12. Do not invent product scope

The current MVP includes authentication and roles, assets,
assignment/return history, support tickets and their histories/comments,
knowledge articles, keyword search, a semantic-search research
prototype, and a basic dashboard.

Treat the following as out of scope unless the project owner explicitly
approves them: - Automatic network scanning or installed endpoint
agents. - Full accounting, depreciation, purchasing, or procurement
workflows. - Replacing SAP/ERP or commercial ITSM products. - Native
mobile applications. - AI that closes tickets, runs commands, or changes
user-device configurations without confirmation. -
Microservices/Kubernetes without a demonstrated need. - SLA, advanced
approvals, and purchase workflows as core MVP features; these are
potential future extensions.

Do not add navigation items for unimplemented modules just to make the
product appear larger. If a future feature needs to be mentioned, mark
it clearly as planned/not available.

## 13. Definition of done for each screen

Before considering a page complete, check:

-   [ ] It serves a defined role and a real workflow from the
    specification.
-   [ ] Georgia is applied consistently, including relevant library
    controls and overlays.
-   [ ] No gradient appears anywhere.
-   [ ] Colors follow the shared tokens and status colors have text
    labels.
-   [ ] Layout, spacing, headings, and actions are consistent with other
    pages.
-   [ ] Data shown comes from the API or is explicitly labeled as demo
    data.
-   [ ] Loading, empty, error, success, and disabled states are handled
    where applicable.
-   [ ] Forms have labels, validation, and useful error messages.
-   [ ] Destructive actions require clear confirmation.
-   [ ] Role-aware navigation does not pretend to replace backend
    authorization.
-   [ ] Keyboard focus and contrast are usable.
-   [ ] The layout works at desktop/laptop and narrow viewport sizes.
-   [ ] TypeScript types and existing project conventions are respected.
-   [ ] No unrelated dependencies or speculative features were
    introduced.

## 14. Recommended workflow for the coding agent

When asked to build or revise a frontend screen:

1.  Inspect the existing repository, routes, dependencies, API
    contracts, and shared styles before coding.
2.  Identify the role, user task, data needed, and actions the screen
    must support.
3.  Reuse the design tokens and existing components; do not create a
    competing visual system.
4.  Implement the smallest complete workflow, including loading, empty,
    error, and success states.
5.  Check the screen against the definition-of-done checklist.
6.  Run the available type check, lint, and tests. Report anything that
    could not be run.
7.  Summarize files changed, user-visible behavior, API assumptions, and
    known limitations.

If requirements are ambiguous, choose the simplest design consistent
with the specification and explain the assumption. Ask a question only
when the ambiguity blocks safe or correct implementation.

## 15. Core design rule

**The interface should look like a coherent internal enterprise tool:
Georgia typography, solid navy and neutral surfaces, restrained teal
accents, clear data tables, and predictable workflows. No gradients, no
fake metrics, no decorative complexity, and no functionality invented
beyond the project scope.**
