# Contract: Ideas — Category Extension

**Feature**: `specs/002-smart-category-forms/spec.md`
**Date**: 2026-05-14
**Extends**: `specs/001-innovation-ideas/contracts/ideas.md`

---

## Overview

This contract documents the updated interfaces for the Ideas feature after adding smart category-adaptive form support. All existing contracts from `specs/001-innovation-ideas/contracts/ideas.md` remain valid. This document adds only the category-specific extensions.

---

## Updated Controller Actions

### IdeasController.Create (GET)

**Route**: `GET /Ideas/Create`

**Returns**: `View(new CreateIdeaViewModel())`

**ViewModel**: `CreateIdeaViewModel` — unchanged signature, new nullable fields pre-populated as empty.

---

### IdeasController.Create (POST)

**Route**: `POST /Ideas/Create`

**Input**: `CreateIdeaViewModel` (model-bound from form POST)

**New fields in form body**:

| Field | HTML Name | Type | Required | Constraints |
|---|---|---|---|---|
| Category | `Category` | select | Yes | Must be one of: `TechnicalImprovement`, `ProcessImprovement`, `ClientSolution` |
| TechArea | `TechArea` | select | Conditional | Required if Category = TechnicalImprovement |
| TechEffort | `TechEffort` | select | Conditional | Required if Category = TechnicalImprovement |
| TechBenefit | `TechBenefit` | textarea | Conditional | Required if Category = TechnicalImprovement; max 500 chars |
| ProcDepartment | `ProcDepartment` | text | Conditional | Required if Category = ProcessImprovement; max 100 chars |
| ProcPainPoint | `ProcPainPoint` | textarea | Conditional | Required if Category = ProcessImprovement; max 500 chars |
| ProcSavings | `ProcSavings` | text | No | Optional if Category = ProcessImprovement; max 200 chars |
| ClientSegment | `ClientSegment` | text | Conditional | Required if Category = ClientSolution; max 200 chars |
| ClientProblem | `ClientProblem` | textarea | Conditional | Required if Category = ClientSolution; max 500 chars |
| ClientImpact | `ClientImpact` | text | Conditional | Required if Category = ClientSolution; max 300 chars |

**Validation errors** (FluentValidation, inline display):

- Missing category: `"Please select a category."`
- Missing required category field: `"[Field label] is required."`
- Exceeds max length: `"Max [N] characters."`

**Success response**: `RedirectToAction("Detail", new { id = ideaId })`

**Failure response**: `View(vm)` with `ModelState` errors

---

### IdeasController.Index (GET)

**Route**: `GET /Ideas?statusFilter=...&categoryFilter=...`

**New query parameter**:

| Parameter | Type | Required | Description |
|---|---|---|---|
| `categoryFilter` | string | No | Filter by category key. Empty = all categories including uncategorized. |

**ViewModel changes**:
- `IdeaListViewModel.CategoryFilter` — the active filter value
- `IdeaListViewModel.AvailableCategories` — list of category display names for filter dropdown

---

### IdeasController.Detail (GET)

**Route**: `GET /Ideas/Detail/{id}`

**ViewModel changes** (via `IdeaDetailDTO`):
- `Category` — category key or `null`
- `CategoryDisplayName` — resolved human-readable name (e.g., "Technical Improvement") or "Uncategorized"
- `CategoryDataFields` — `Dictionary<string, string>` of label → value pairs for display. Empty fields excluded.

---

### AdminController.Index (GET)

**Route**: `GET /Admin?statusFilter=...&categoryFilter=...`

**New query parameter**:

| Parameter | Type | Required | Description |
|---|---|---|---|
| `categoryFilter` | string | No | Filter by category key. Empty = all categories. |

**ViewModel changes** (same as `IdeaListViewModel` above, applied to `AdminIdeaListViewModel`):
- `CategoryFilter`
- `AvailableCategories`

---

### AdminController.Detail (GET)

**Route**: `GET /Admin/Detail/{id}`

**ViewModel changes**: same as `IdeasController.Detail` — category section shown in admin review panel.

---

## Updated Service Contracts

### IIdeaService

```
CreateIdeaAsync(submitterId: Guid, vm: CreateIdeaViewModel)
  → (Success: bool, Error: string?, IdeaId: Guid)

  New behavior:
  - Reads vm.Category and category-specific fields
  - Builds CategoryData JSON: serialize only fields matching the selected category
  - Persists Category and CategoryData on the new Idea record
  - Legacy behavior unchanged when Category is null/empty (validation prevents this at runtime)

GetMyIdeasAsync(submitterId: Guid, statusFilter: string?, categoryFilter: string? = null)
  → List<IdeaListItemDTO>

  New behavior:
  - If categoryFilter is non-null/non-empty, filter results by IdeaListItemDTO.Category

GetAllIdeasAsync(statusFilter: string?, categoryFilter: string? = null)
  → List<IdeaListItemDTO>

  New behavior:
  - Same as above, applied to admin query

GetIdeaDetailAsync(ideaId: Guid, userId: Guid, isAdmin: bool)
  → IdeaDetailDTO?

  New behavior:
  - Maps Category → CategoryDisplayName via CategoryDefinitions lookup
  - Deserializes CategoryData JSON → Dictionary<string, string> → CategoryDataFields
  - Replaces field keys with human-readable labels from CategoryDefinitions
  - Null Category → CategoryDisplayName = "Uncategorized", CategoryDataFields = empty
```

---

## CategoryData Serialization / Deserialization

### Serialization (on Create)

```
Input: vm.Category = "ProcessImprovement"
       vm.ProcDepartment = "Engineering"
       vm.ProcPainPoint = "Manual releases..."
       vm.ProcSavings = ""    (optional, left blank)

Output CategoryData (JSON string):
{
  "ProcDepartment": "Engineering",
  "ProcPainPoint": "Manual releases...",
  "ProcSavings": ""
}
```

Only fields belonging to the selected category are included. Fields from other categories are ignored.

### Deserialization (on Detail)

```
Input: idea.Category = "ProcessImprovement"
       idea.CategoryData = '{"ProcDepartment":"Engineering","ProcPainPoint":"..."}'

Output IdeaDetailDTO.CategoryDataFields:
{
  "Affected Department or Team": "Engineering",
  "Current Process Pain Point": "...",
}
// ProcSavings excluded (empty string → not displayed)
```

The service layer resolves field keys to human-readable labels using `CategoryDefinitions.All[category].Fields`.

---

## Validation Test Matrix

| Scenario | Category | Submitted Fields | Expected Result |
|---|---|---|---|
| No category selected | — | (none) | Form rejected: "Please select a category." |
| TechnicalImprovement — all fields | TechnicalImprovement | TechArea, TechEffort, TechBenefit | ✅ Created |
| TechnicalImprovement — missing TechArea | TechnicalImprovement | TechEffort, TechBenefit | Rejected: "Technology Area is required." |
| ProcessImprovement — all required fields | ProcessImprovement | ProcDepartment, ProcPainPoint | ✅ Created (ProcSavings optional) |
| ProcessImprovement — missing ProcPainPoint | ProcessImprovement | ProcDepartment | Rejected: "Current Process Pain Point is required." |
| ClientSolution — all fields | ClientSolution | ClientSegment, ClientProblem, ClientImpact | ✅ Created |
| ClientSolution — TechBenefit > 500 chars | ClientSolution | ClientProblem = 501 chars | Rejected: "Max 500 characters." |
| Legacy idea (no category) — detail view | — | — | Shows "Uncategorized"; no category section |

---

**Version**: 1.0.0 | **Created**: 2026-05-14
