# Interface Contracts: Idea Draft Management

**Phase**: 1 — Design
**Feature**: `specs/003-draft-management/spec.md`
**Date**: 2026-05-14

---

## IdeasController — New & Updated Actions

All actions require `[Authorize]` (any authenticated user). Ownership checks (submitter = current user) are enforced in the service layer.

---

### POST /Ideas/SaveDraft

**Purpose**: Save a new draft from the Create form without triggering required-field validation.

| Property | Value |
|---|---|
| HTTP Method | POST |
| Route | `/Ideas/SaveDraft` |
| Auth | `[Authorize]` |
| CSRF | `[ValidateAntiForgeryToken]` |

**Input**: `CreateIdeaViewModel` (same as Create — the Create form gains a second "Save as Draft" button posting to this action via `formaction` attribute)

**Validation**: **None** — `ModelState` is not checked. Any data provided is accepted as-is.

**Success**: Redirect to `Ideas/Edit/{draftId}` with `TempData["Success"] = "Draft saved."`

**Error** (file storage failure): Return `Create` view with `ModelState` error.

---

### GET /Ideas/Edit/{id}

**Purpose**: Load the draft edit form pre-populated with all saved field values.

| Property | Value |
|---|---|
| HTTP Method | GET |
| Route | `/Ideas/Edit/{id:guid}` |
| Auth | `[Authorize]` |

**Behaviour**:
- Calls `IdeaService.GetIdeaDetailAsync(id, userId, isAdmin: false)`
- If idea is null or `Status != Draft` or `SubmitterId != userId` → `NotFound()`
- Maps `IdeaDetailDTO` → `EditDraftViewModel` (populate all fields + `ExistingAttachment`)
- Returns `Ideas/Edit` view with the populated `EditDraftViewModel`

---

### POST /Ideas/UpdateDraft/{id}

**Purpose**: Save changes to an existing draft without required-field validation.

| Property | Value |
|---|---|
| HTTP Method | POST |
| Route | `/Ideas/UpdateDraft/{id:guid}` |
| Auth | `[Authorize]` |
| CSRF | `[ValidateAntiForgeryToken]` |

**Input**: `EditDraftViewModel`

**Validation**: **None** — `ModelState` is not checked.

**Success**: Redirect to `Ideas/Edit/{id}` with `TempData["Success"] = "Draft saved."`

**Error** (ownership/not found/file error): Redirect to `Ideas/Index` with `TempData["Error"]` or return Edit view with error.

---

### POST /Ideas/SubmitDraft/{id}

**Purpose**: Validate and submit a draft, transitioning it to `Submitted` status.

| Property | Value |
|---|---|
| HTTP Method | POST |
| Route | `/Ideas/SubmitDraft/{id:guid}` |
| Auth | `[Authorize]` |
| CSRF | `[ValidateAntiForgeryToken]` |

**Input**: `EditDraftViewModel`

**Validation**: Full `EditDraftValidator` runs (Category required, Title required, category-specific `When()` rules). If `!ModelState.IsValid` → re-render `Ideas/Edit` view with inline errors (no data loss — draft remains in Draft status).

**Success**: Redirect to `Ideas/Detail/{id}` with `TempData["Success"] = "Idea submitted successfully."`

**Error** (ownership/not found): `NotFound()` or `TempData["Error"]` redirect.

---

### POST /Ideas/DeleteDraft/{id}

**Purpose**: Permanently delete a draft and its associated files.

| Property | Value |
|---|---|
| HTTP Method | POST |
| Route | `/Ideas/DeleteDraft/{id:guid}` |
| Auth | `[Authorize]` |
| CSRF | `[ValidateAntiForgeryToken]` |

**Input**: Route parameter `id` only (no body required)

**Validation**: Service checks `Status == Draft` and `SubmitterId == userId` before deletion.

**Success**: Redirect to `Ideas/Index` with `TempData["Success"] = "Draft deleted."`

**Error** (not found / not a draft / not owner): `NotFound()` or `TempData["Error"]` redirect.

---

## IdeaService — Updated Contracts

### GetAllIdeasAsync (updated)

```
GetAllIdeasAsync(string? statusFilter, string? categoryFilter = null)
  → Always filters out Draft (Status == 0) before applying statusFilter
  → Admin can never see draft ideas regardless of statusFilter value
```

### SaveDraftAsync (new)

```
SaveDraftAsync(Guid submitterId, CreateIdeaViewModel vm)
  → Creates Idea with Status = Draft
  → Saves attachment if provided (with MIME/size validation)
  → Returns (Success, Error, DraftId)
```

### UpdateDraftAsync (new)

```
UpdateDraftAsync(Guid draftId, Guid submitterId, EditDraftViewModel vm)
  → Validates ownership and Status == Draft
  → Updates all editable fields
  → If RemoveAttachment == true: deletes file from disk + removes IdeaAttachment record
  → If new Attachment provided: validates MIME/size, saves file, replaces existing attachment
  → Updates LastModifiedDate
  → Returns (Success, Error)
```

### SubmitDraftAsync (new)

```
SubmitDraftAsync(Guid draftId, Guid submitterId, EditDraftViewModel vm)
  → Validates ownership and Status == Draft
  → Applies UpdateDraft logic (saves latest form state to DB)
  → Sets Status = Submitted, LastModifiedDate = UtcNow
  → Returns (Success, Error)
  → Caller (controller) runs EditDraftValidator BEFORE calling this method
```

### DeleteDraftAsync (new)

```
DeleteDraftAsync(Guid draftId, Guid submitterId)
  → Validates ownership and Status == Draft
  → Deletes all IdeaAttachment files from disk
  → Removes Idea record (cascade deletes IdeaAttachments via EF)
  → Returns (Success, Error)
```

---

## Validation Matrix

| Field | SaveDraft | UpdateDraft | SubmitDraft |
|---|---|---|---|
| Category | Optional | Optional | Required |
| Title | Optional | Optional | Required (max 200) |
| Description | Optional | Optional | Optional (max 2000) |
| TechArea | Optional | Optional | Required if Category=TechnicalImprovement |
| TechEffort | Optional | Optional | Required if Category=TechnicalImprovement |
| TechBenefit | Optional | Optional | Required (max 500) if Category=TechnicalImprovement |
| ProcDepartment | Optional | Optional | Required (max 100) if Category=ProcessImprovement |
| ProcPainPoint | Optional | Optional | Required (max 500) if Category=ProcessImprovement |
| ProcSavings | Optional | Optional | Optional (max 200) if Category=ProcessImprovement |
| ClientSegment | Optional | Optional | Required (max 200) if Category=ClientSolution |
| ClientProblem | Optional | Optional | Required (max 500) if Category=ClientSolution |
| ClientImpact | Optional | Optional | Required (max 300) if Category=ClientSolution |
| Attachment | Optional | Optional | Optional |
| RemoveAttachment | N/A | Respected | Respected |
