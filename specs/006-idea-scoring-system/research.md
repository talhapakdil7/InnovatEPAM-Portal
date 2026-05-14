# Research: Idea Scoring System

**Feature**: `006-idea-scoring-system`
**Phase**: 0 — Research & Decision Log

---

## Decision 1: Score Storage Strategy — One Row Per (Idea, Admin) vs. One Row Per (Idea, Admin, Dimension)

**Decision**: One row per `(IdeaId, AdminId)` in an `IdeaScores` table, with four nullable integer columns for the four fixed dimensions.

**Rationale**:
- The four dimensions are fixed per spec (not user-configurable). A normalized EAV (Entity–Attribute–Value) model adds query complexity for no benefit.
- A single-row upsert is simpler than managing 4 separate dimension rows per admin.
- Nullable columns elegantly model "partial scoring" (FR-003) — a null dimension value means the admin skipped that dimension.
- Composite unique constraint on `(IdeaId, AdminId)` enforces the one-record-per-admin-per-idea invariant at the database level.
- Aggregate queries are simple aggregations (`AVG`, `COUNT`) over a compact table.

**Alternatives considered**:
- EAV (separate table per dimension value): Rejected — over-engineering for a fixed 4-dimension model.
- One row per dimension (4 rows per admin per idea): Rejected — more complex upserts and queries; no benefit here.
- Storing scores as JSON on the `Idea` entity: Rejected — harder to query, aggregate, and validate at the DB level.

---

## Decision 2: Aggregate Calculation — Application Layer vs. Database Computed Column

**Decision**: Calculate aggregates in the application layer (service layer), not as a PostgreSQL stored/computed column or view.

**Rationale**:
- Per spec Assumption: "The aggregate score is calculated in the application layer."
- All existing business logic lives in services (Constitution §I, §III). Putting aggregation in PostgreSQL would split domain logic.
- Application-layer calculation is immediately consistent after any score change (no trigger latency).
- `IScoreService.GetScoreSummaryAsync(ideaId)` returns a `ScoreSummaryDTO` computed from all `IdeaScore` rows for that idea — clean, testable, reusable.

**Alternatives considered**:
- PostgreSQL view: Rejected — bypasses service layer; hard to maintain as dimensions change.
- Cached/materialized aggregate: Rejected — adds complexity; SC-002 requires real-time recalculation on page reload.

---

## Decision 3: Where to Surface Scoring UI — Embedded in Admin Detail Page vs. Separate Score Page

**Decision**: Scoring form embedded directly in the Admin idea detail page (`Admin/Detail.cshtml`). Score submission posts to a new `Admin/SubmitScore` POST endpoint.

**Rationale**:
- SC-001 requires completing scoring in under 60 seconds without navigating away.
- SC-003 requires all four dimensions visible on a single page.
- The existing `Admin/Detail.cshtml` already aggregates all workflow actions (advance stage, revert, record decision) — scoring is another workflow action naturally grouped there.
- Separate page would violate SC-001 and SC-003.

**Alternatives considered**:
- Dedicated `Score/Submit` page: Rejected — extra navigation step; violates SC-001 and SC-003.
- Modal dialog: Viable but adds Bootstrap JS complexity; inline form is simpler and equally effective.

---

## Decision 4: Blind Review Mode Integration — Scorer Name Masking

**Decision**: When blind review mode is active, display scorer names as "Anonymous Reviewer" in the admin score breakdown section. Aggregated scores are always visible to admins (masking only applies to identity, not data quality).

**Rationale**:
- Spec Assumption: "Blind review mode applies to scores: scorer names are not displayed in admin score views when active."
- `IBlindReviewService` already handles name masking; the same service call pattern is reused for `IdeaScore` scorer names.
- `ScoreSummaryDTO` scorer names are masked at the service layer before the ViewModel is populated — consistent with existing masking pattern.

**Alternatives considered**:
- Always show scorer names regardless of blind review: Rejected — violates spec Assumptions and UX consistency.
- Hide entire score section when blind review active: Rejected — overkill; aggregate scores do not reveal identity.

---

## Decision 5: Score Retraction — Soft Delete vs. Hard Delete

**Decision**: Hard delete the `IdeaScore` row on retraction.

**Rationale**:
- FR-008: "retraction recalculates all aggregates immediately" — hard delete is the simplest path.
- FR-012 (audit): Audit requirement is for "which admin submitted which score," not for history of retracted scores. The `IdeaScore` row itself records the scorer. Retraction is a business action that removes the record.
- Soft delete adds `IsDeleted` overhead and complicates aggregate queries.

**Alternatives considered**:
- Soft delete with `IsDeleted` flag: Rejected — spec does not require retraction history; adds filter complexity to all aggregate queries.
- Score history table: Rejected — MVP scope does not include score change history.

---

## Decision 6: Controller Placement — New `ScoreController` vs. Extensions to `AdminController`

**Decision**: New `ScoreController` with `[Authorize(Roles = "Admin")]` for all mutating score actions (SubmitScore, RetractScore). Score data is loaded within the existing `AdminController.Detail` action via `IScoreService`.

**Rationale**:
- Constitution §I: thin controllers, single responsibility. `AdminController` is already large (Spec 001–005 actions).
- A dedicated `ScoreController` keeps scoring actions cohesive and independently testable.
- The Admin Detail page still renders all score data — just uses `IScoreService` as an additional injected service dependency.
- `IdeasController` (submitter-facing) reads aggregate from `IScoreService` but has no POST scoring actions.

**Alternatives considered**:
- Add all scoring actions to `AdminController`: Rejected — would make an already-large controller even larger; violates single-responsibility spirit.
- Separate API controller (JSON endpoints): Rejected — project uses MVC with Razor views, not SPA/AJAX; consistent with existing patterns.

---

## Decision 7: `IdeaListItemDTO` Score Field — Full Summary vs. Overall Average Only

**Decision**: Add only `AggregateScore` (nullable `decimal?`) and `ScorerCount` (int) to `IdeaListItemDTO`. Full dimension breakdown is loaded only on the detail page.

**Rationale**:
- FR-007 requires only the "overall aggregate score" in the list view.
- Loading full 4-dimension summaries for every idea in the list is wasteful.
- `IdeaService.GetAllIdeasAsync` can join `IdeaScores` with a GROUP BY to compute per-idea averages efficiently.

**Alternatives considered**:
- Load full `ScoreSummaryDTO` per list item: Rejected — over-fetching; list only needs the headline average.
- Separate AJAX call for list scores: Rejected — no AJAX in the current architecture.

---

## Decision 8: FluentValidation for Score Submission

**Decision**: `SubmitScoreViewModel` is validated by a `SubmitScoreValidator` using FluentValidation. Each dimension score: `InclusiveBetween(1, 5)` when not null; at least one dimension must be non-null (partial scoring allowed but empty form rejected).

**Rationale**:
- FR-002: values outside 1–5 rejected with user-facing message.
- FR-003: partial scoring allowed — validator only enforces the range, not that all 4 are filled.
- Consistent with existing validators (`AdvanceStageValidator`, `RecordDecisionValidator`, etc.) per ADR-006.

**Alternatives considered**:
- DataAnnotations only: Rejected — FluentValidation is the project standard (ADR-006).
- Validate all 4 dimensions required: Rejected — FR-003 explicitly allows partial scoring.
