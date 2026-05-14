# Research: Multi-Stage Innovation Review Workflow

**Phase**: 0 — Research
**Feature**: `specs/004-multi-stage-review/spec.md`
**Date**: 2026-05-14

---

## Decision 1: ReviewStage Representation

**Question**: Should the four review stages be represented as a C# enum (integer column), a lookup table, or strings?

**Decision**: C# `enum ReviewStage` stored as `integer` on the `Ideas` table via EF Core value conversion.

**Rationale**:
- FR-001 explicitly states stages are fixed and non-configurable; a lookup table adds complexity without benefit
- Mirrors the existing `IdeaStatus` enum pattern already in the project (ADR-004)
- Type-safe at compile time; no magic strings in service logic
- Zero database overhead — single nullable `integer` column on the `Ideas` table

**Alternatives Considered**:
| Alternative | Rejected Because |
|---|---|
| Separate `ReviewStages` lookup table | Overkill for a fixed enum; adds a JOIN to every idea query |
| String column on `Ideas` | No compile-time safety; risk of typos; no ordering guarantee |
| Flags enum (bitmask) | Stages are sequential, not combinatorial |

---

## Decision 2: StageTransition Storage

**Question**: Should stage transitions be persisted in the existing `AuditLogs` table or a new dedicated table?

**Decision**: New `StageTransitions` table — separate entity `StageTransition` with its own repository.

**Rationale**:
- `StageTransition` has different fields than `AuditLog`: direction (advance/revert), notes (up to 1000 chars), revert reason (up to 500 chars), optional `Outcome` for the Final Decision stage
- Sharing the `AuditLogs` table would add 4+ nullable columns, making the schema wider and harder to query
- Keeps the existing `AuditLog` (status history) and the new `StageTransition` (workflow history) as separate concerns, consistent with Constitution Principle III (single responsibility)
- Follows the same Repository pattern as `AuditLogRepository`

**Alternatives Considered**:
| Alternative | Rejected Because |
|---|---|
| Extend `AuditLogs` with stage columns | Wide table anti-pattern; breaks existing AuditLog queries |
| Event sourcing / CQRS | Over-engineering for an MVP portal; contradicts ADR-001 |
| JSON blob in `Ideas.CategoryData`-style column | No queryability; stage filter (FR-013) requires indexed column |

---

## Decision 3: Service Boundary

**Question**: Should review workflow logic be added to `IdeaService` or extracted to a new `IReviewWorkflowService`?

**Decision**: New dedicated `IReviewWorkflowService` + `ReviewWorkflowService`.

**Rationale**:
- Constitution Principle III: "Each service MUST have a single responsibility"
- `IdeaService` already owns create/read/update/delete/attach/draft — adding advance/revert/finalDecision would make it a God object
- `ReviewWorkflowService` can be independently tested without mocking the full idea lifecycle
- Consistent with how `AuditLogRepository` is separate from `IdeaRepository`

**Alternatives Considered**:
| Alternative | Rejected Because |
|---|---|
| Add 3–4 methods to `IIdeaService` | Violates SRP; `IdeaService.cs` already has 9 public methods |
| Inline in `AdminController` | Violates Constitution Principle I (no business logic in controllers) |

---

## Decision 4: Controller Placement

**Question**: Should workflow actions live in `AdminController` or a new `ReviewWorkflowController`?

**Decision**: Extend `AdminController` with 3 new POST actions (`AdvanceStage`, `RevertStage`, `RecordDecision`).

**Rationale**:
- Workflow actions are exclusively admin-facing; logically belong in the admin area
- 3 actions do not exceed controller cohesion — thin actions delegating to `ReviewWorkflowService`
- Avoids creating a new route prefix for what is conceptually an admin operation
- Constitution: controllers are "thin orchestration only"

**Alternatives Considered**:
| Alternative | Rejected Because |
|---|---|
| New `ReviewWorkflowController` | Unnecessary route proliferation for 3 actions |
| AJAX / API endpoints | Not consistent with MVC form-post pattern (ADR-001); no SPA frontend |

---

## Decision 5: Stage Filter Integration

**Question**: Should `GetAllIdeasAsync` be extended with a `reviewStageFilter` parameter, or should a new method be added?

**Decision**: Extend `GetAllIdeasAsync` signature with `string? reviewStageFilter = null` — the same pattern used for `categoryFilter` in Spec 002.

**Rationale**:
- Maintains API consistency with how Spec 002 extended the same method
- Admin list view already builds a filter chain; adding one more predicate is straightforward
- Avoids interface proliferation

---

## Decision 6: Final Decision & Existing Status Flow Integration

**Question**: How does the Final Decision outcome (Accepted/Rejected) interact with the existing `IdeaStatus` enum and `UpdateStatusAsync`?

**Decision**: `ReviewWorkflowService.RecordFinalDecisionAsync` internally calls `IIdeaService.UpdateStatusAsync` to update the overall status to `Accepted` or `Rejected`, then records a `StageTransition` with `Outcome` set.

**Rationale**:
- Reuses the existing `UpdateStatusAsync` logic (audit log entry, admin tracking) to avoid duplication
- The existing `AuditLog` entry for the status change remains intact for backward-compatibility
- `StageTransition` with `Outcome` provides the richer workflow-level record
- No changes to `IdeaStatus` enum required

---

## Decision 7: Submitter Stage Visibility

**Question**: How is the read-only stage displayed to submitters (FR-008)?

**Decision**: `IdeaDetailDTO` gains two new nullable properties: `CurrentReviewStageName` (string?) and `CurrentReviewStageOrder` (int?). The submitter's `Detail.cshtml` renders a Bootstrap progress indicator when these are non-null.

**Rationale**:
- Reuses the existing DTO and `GetIdeaDetailAsync` call without a new service method
- `CurrentReviewStageOrder` (1–4) drives a visual progress bar / step indicator in the view
- Null = no stage assigned yet → "Pending Review" message shown instead

---

## Decision 8: Concurrency Handling

**Question**: What happens when two admins transition the same idea simultaneously?

**Decision**: Last-write-wins via EF Core optimistic concurrency (no `RowVersion` column added at this stage). The transition is logged with the responsible admin's identity.

**Rationale**:
- Single-admin assumption documented in spec Assumptions: "A single admin performs all stages"
- Adding a `RowVersion` / concurrency token would require a new migration column and UI handling; deferred to a future phase if multi-reviewer support is added
- Serilog structured logging already captures admin identity per transition

---

## Complexity Summary

No constitution violations. All decisions follow existing ADR patterns. New files are additive; no existing interfaces are broken.
