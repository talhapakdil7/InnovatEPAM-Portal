# Interface Contracts: Multi-Stage Innovation Review Workflow

**Phase**: 1 — Design
**Feature**: `specs/004-multi-stage-review/spec.md`
**Date**: 2026-05-14

---

## AdminController — New Actions

All new actions require `[Authorize(Roles = "Admin")]` and `[ValidateAntiForgeryToken]`.
Ownership and business-rule checks are enforced in `ReviewWorkflowService`, not the controller.

---

### POST /Admin/AdvanceStage

**Purpose**: Advance an idea to the next review stage. If the idea is in Submitted status, it is automatically transitioned to Under Review.

| Property | Value |
|---|---|
| HTTP Method | POST |
| Route | `/Admin/AdvanceStage` |
| Auth | `[Authorize(Roles = "Admin")]` |
| CSRF | `[ValidateAntiForgeryToken]` |

**Input**: `AdvanceStageViewModel` (IdeaId, optional Notes max 1000 chars)

**Validation**: FluentValidation via `AdvanceStageValidator`. Service-level preconditions: Status ∈ {Submitted, UnderReview}, CurrentStage ≠ FinalDecision, Status ∉ {Accepted, Rejected, Draft}.

**Success**: Redirect to `Admin/Detail/{IdeaId}` with `TempData["Success"] = "Stage advanced to {StageName}."`

**Error (business rule)**: Redirect to `Admin/Detail/{IdeaId}` with `TempData["Error"] = {errorMessage}`

---

### POST /Admin/RevertStage

**Purpose**: Revert an idea to a specified previous review stage. Requires a mandatory revert reason.

| Property | Value |
|---|---|
| HTTP Method | POST |
| Route | `/Admin/RevertStage` |
| Auth | `[Authorize(Roles = "Admin")]` |
| CSRF | `[ValidateAntiForgeryToken]` |

**Input**: `RevertStageViewModel` (IdeaId, TargetStage, RevertReason max 500 chars, optional Notes max 1000 chars)

**Validation**: FluentValidation via `RevertStageValidator`. Service preconditions: Status = UnderReview, CurrentStage > InitialScreening, TargetStage < CurrentStage.

**Success**: Redirect to `Admin/Detail/{IdeaId}` with `TempData["Success"] = "Stage reverted to {StageName}."`

**Error**: Redirect to `Admin/Detail/{IdeaId}` with `TempData["Error"] = {errorMessage}`

---

### POST /Admin/RecordDecision

**Purpose**: Record the final outcome (Accepted or Rejected) from the Final Decision stage. Sets the overall idea status accordingly.

| Property | Value |
|---|---|
| HTTP Method | POST |
| Route | `/Admin/RecordDecision` |
| Auth | `[Authorize(Roles = "Admin")]` |
| CSRF | `[ValidateAntiForgeryToken]` |

**Input**: `RecordDecisionViewModel` (IdeaId, Outcome ∈ {"Accepted","Rejected"}, optional Notes max 1000 chars)

**Validation**: FluentValidation via `RecordDecisionValidator`. Service preconditions: Status = UnderReview, CurrentStage = FinalDecision.

**Success**: Redirect to `Admin/Detail/{IdeaId}` with `TempData["Success"] = "Idea marked as {Outcome}."`

**Error**: Redirect to `Admin/Detail/{IdeaId}` with `TempData["Error"] = {errorMessage}`

---

## Updated `GetAllIdeasAsync` Signature

```
GetAllIdeasAsync(string? statusFilter, string? categoryFilter = null, string? reviewStageFilter = null)
  → Always filters out Draft (Status == 0) — unchanged from Spec 003
  → If reviewStageFilter is non-null, additionally filters by CurrentReviewStageName == reviewStageFilter
```

---

## IReviewWorkflowService Contracts

### AdvanceStageAsync

```
AdvanceStageAsync(Guid ideaId, Guid adminId, string? notes)
  → Load idea; verify Status ∈ {Submitted, UnderReview} AND Status ∉ {Accepted, Rejected, Draft}
  → Verify CurrentReviewStage ≠ FinalDecision (cannot advance beyond last stage)
  → Compute nextStage = ReviewStageHelper.NextStage(CurrentReviewStage ?? 0) → InitialScreening if null
  → If idea.Status == Submitted: call IIdeaService.UpdateStatusAsync → UnderReview (creates AuditLog)
  → Update idea.CurrentReviewStage = nextStage
  → Persist StageTransition { FromStage = old, ToStage = nextStage, IsAdvance = true, Notes, TransitionedByAdminId = adminId }
  → Returns (true, null) on success; (false, errorMessage) on precondition failure
```

### RevertStageAsync

```
RevertStageAsync(Guid ideaId, ReviewStage targetStage, Guid adminId, string revertReason, string? notes)
  → Load idea; verify Status = UnderReview AND Status ∉ {Accepted, Rejected}
  → Verify CurrentReviewStage is not null AND is not InitialScreening
  → Verify targetStage < CurrentReviewStage (can only revert to earlier stage)
  → Update idea.CurrentReviewStage = targetStage
  → Persist StageTransition { FromStage = old, ToStage = targetStage, IsAdvance = false, RevertReason, Notes, TransitionedByAdminId = adminId }
  → Returns (true, null) on success; (false, errorMessage) on failure
```

### RecordFinalDecisionAsync

```
RecordFinalDecisionAsync(Guid ideaId, Guid adminId, string outcome, string? notes)
  → Load idea; verify Status = UnderReview AND CurrentReviewStage = FinalDecision
  → Verify outcome ∈ {"Accepted", "Rejected"}
  → Call IIdeaService.UpdateStatusAsync → outcome (creates AuditLog entry)
  → Persist StageTransition { FromStage = FinalDecision, ToStage = FinalDecision, IsAdvance = true, Outcome = outcome, Notes, TransitionedByAdminId = adminId }
  → Returns (true, null) on success; (false, errorMessage) on failure
```

---

## Validation Matrix

| Field | AdvanceStage | RevertStage | RecordDecision |
|---|---|---|---|
| IdeaId | Required (Guid) | Required (Guid) | Required (Guid) |
| Notes | Optional, max 1000 | Optional, max 1000 | Optional, max 1000 |
| TargetStage | N/A | Required (ReviewStage enum) | N/A |
| RevertReason | N/A | Required, max 500 | N/A |
| Outcome | N/A | N/A | Required ∈ {Accepted, Rejected} |
