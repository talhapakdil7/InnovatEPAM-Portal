# Data Model: Idea Draft Management

**Phase**: 1 — Design
**Feature**: `specs/003-draft-management/spec.md`
**Date**: 2026-05-14

---

## 1. Model Changes

### IdeaStatus Enum — add `Draft = 0`

```csharp
// src/InnovatEPAM.Portal/Models/Idea.cs
public enum IdeaStatus
{
    Draft     = 0,   // ← NEW: saved but not submitted to admin review
    Submitted = 1,
    UnderReview = 2,
    Accepted  = 3,
    Rejected  = 4
}
```

**Impact**: No database migration needed — the `Status` column already stores an `integer`; value `0` was unused. Default value on `Idea` entity remains `IdeaStatus.Submitted` for the standard create path.

### Idea Entity — no new columns needed

The existing `Category`, `CategoryData`, `Title`, `Description`, `IdeaAttachments` and all audit columns are already present. A Draft is an Idea with `Status = IdeaStatus.Draft`.

---

## 2. New ViewModel

### `EditDraftViewModel`

Location: `src/InnovatEPAM.Portal/ViewModels/IdeaViewModels.cs`

```csharp
/// <summary>
/// ViewModel for editing and saving/submitting an existing draft idea.
/// </summary>
public class EditDraftViewModel
{
    /// <summary>The ID of the draft being edited.</summary>
    public Guid Id { get; set; }

    // ── Category & common fields (identical to CreateIdeaViewModel) ──

    public string? Category { get; set; }

    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    // ── Attachment management ──

    /// <summary>Existing attachment to display (null when draft has no attachment).</summary>
    public IdeaAttachmentDTO? ExistingAttachment { get; set; }

    /// <summary>When true, the existing attachment is removed on save.</summary>
    public bool RemoveAttachment { get; set; }

    [Display(Name = "New Attachment (PDF, DOC, DOCX, XLS, XLSX, JPG, PNG — max 10 MB)")]
    public IFormFile? Attachment { get; set; }

    // ── Technical Improvement fields ──
    public string? TechArea { get; set; }
    public string? TechEffort { get; set; }
    public string? TechBenefit { get; set; }

    // ── Process Improvement fields ──
    public string? ProcDepartment { get; set; }
    public string? ProcPainPoint { get; set; }
    public string? ProcSavings { get; set; }

    // ── Client Solution fields ──
    public string? ClientSegment { get; set; }
    public string? ClientProblem { get; set; }
    public string? ClientImpact { get; set; }
}
```

---

## 3. Updated ViewModels

### `IdeaListViewModel` — no change needed

Submitters already see all their ideas via `GetMyIdeasAsync`. Draft ideas appear with `Status = "Draft"` in the list; the view renders a distinct badge color.

### `IdeaDetailViewModel` — add `IsDraft` helper

```csharp
public class IdeaDetailViewModel
{
    public IdeaDetailDTO Idea { get; set; } = null!;
    public bool IsAdmin { get; set; }
    /// <summary>True when the idea is in Draft status, enabling edit/delete actions in the view.</summary>
    public bool IsDraft { get; set; }
}
```

---

## 4. Updated DTOs

### `IdeaListItemDTO` — no change needed

`CategoryDisplayName` and `Category` already present. `Status` string will now include `"Draft"`.

### `IdeaDetailDTO` — no change needed

Already has `Category`, `CategoryDisplayName`, `CategoryDataFields`, `Attachments`. Draft detail view reuses this DTO.

---

## 5. New & Updated Service Methods

### `IIdeaService` additions

```csharp
/// <summary>Saves a new draft idea without applying required-field validation.</summary>
Task<(bool Success, string? Error, Guid DraftId)> SaveDraftAsync(Guid submitterId, CreateIdeaViewModel vm);

/// <summary>Updates an existing draft's fields and attachment without required-field validation.</summary>
Task<(bool Success, string? Error)> UpdateDraftAsync(Guid draftId, Guid submitterId, EditDraftViewModel vm);

/// <summary>
/// Validates the draft fully and transitions it to Submitted status.
/// Returns validation errors if required fields are missing.
/// </summary>
Task<(bool Success, string? Error)> SubmitDraftAsync(Guid draftId, Guid submitterId, EditDraftViewModel vm);

/// <summary>Permanently deletes a draft and all associated attachments. Only the owning submitter may call this.</summary>
Task<(bool Success, string? Error)> DeleteDraftAsync(Guid draftId, Guid submitterId);
```

### `IIdeaService.GetAllIdeasAsync` — updated filter

The admin-facing method always excludes `Draft` status ideas regardless of the `statusFilter` argument. The filter is applied server-side in `IdeaService` before any further filtering.

---

## 6. Validation

### Existing `CreateIdeaValidator` — reuse for Submit Draft

When `SubmitDraftAsync` is called, the controller builds an `EditDraftViewModel` from the POST body and runs `IValidator<EditDraftViewModel>` against it.

### New `EditDraftValidator`

```csharp
// src/InnovatEPAM.Portal/Validators/EditDraftValidator.cs
// Used ONLY for SubmitDraftAsync — same rules as CreateIdeaValidator:
// Category required, title required, category-specific When() rules.
// For SaveDraft / UpdateDraft: no validator is invoked (FR-001).
```

---

## 7. EF Core Migration

A new migration `AddDraftStatus` is required to:
- Update the `ApplicationDbContextModelSnapshot.cs` comments/checks if any hardcoded status integer checks exist
- No new columns or tables — only the C# enum gains the `Draft = 0` value

> Because the database column type is `integer` and value `0` was previously unused, this is a zero-downtime, backward-compatible change. Existing rows are unaffected.

---

## 8. Status Badge Colors (UI)

| Status     | Bootstrap class           |
|------------|--------------------------|
| Draft      | `bg-secondary bg-opacity-50` (muted) |
| Submitted  | `bg-secondary`            |
| UnderReview | `bg-warning text-dark`   |
| Accepted   | `bg-success`              |
| Rejected   | `bg-danger`               |
