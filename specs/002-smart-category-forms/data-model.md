# Data Model: Smart Category-Adaptive Submission Forms

**Feature**: `specs/002-smart-category-forms/spec.md`
**Date**: 2026-05-14
**Extends**: `specs/001-innovation-ideas/data-model.md`

---

## Summary of Changes

This feature extends the existing `Idea` entity with two nullable columns and introduces a static `CategoryDefinitions` model class. **No new database tables are created.** Existing entities (`ApplicationUser`, `IdeaAttachment`, `AuditLog`) are unchanged.

---

## Modified Entity: Idea

### New Columns

| Column | Type | Nullable | Max Length | Default | Description |
|---|---|---|---|---|---|
| `Category` | `string` | Yes | 50 | `null` | Category key: `TechnicalImprovement`, `ProcessImprovement`, `ClientSolution`. Null for legacy ideas. |
| `CategoryData` | `string` | Yes | — | `null` | JSON-serialized `Dictionary<string, string>` of category-specific field answers. Null when no category. |

### Updated Entity Definition

```csharp
public class Idea
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;       // max 200, required
    public string? Description { get; set; }                  // max 2000, optional
    public IdeaStatus Status { get; set; } = IdeaStatus.Submitted;

    // NEW — nullable for backward compat with pre-category ideas
    public string? Category { get; set; }                    // max 50
    public string? CategoryData { get; set; }                // JSON text, no length limit

    public Guid SubmitterId { get; set; }
    public ApplicationUser Submitter { get; set; } = null!;
    public Guid? LastModifiedByAdminId { get; set; }
    public ApplicationUser? UpdatedByAdmin { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public ICollection<IdeaAttachment> IdeaAttachments { get; set; }
    public ICollection<AuditLog> AuditLogs { get; set; }
}
```

### EF Core Configuration (ApplicationDbContext)

```csharp
entity.Property(i => i.Category)
    .HasMaxLength(50)
    .IsRequired(false);

entity.Property(i => i.CategoryData)
    .IsRequired(false);

entity.HasIndex(i => i.Category);  // supports admin category filter queries
```

### Migration

A new EF Core migration `AddIdeaCategoryFields` adds:

```sql
ALTER TABLE "Ideas" ADD COLUMN "Category" character varying(50) NULL;
ALTER TABLE "Ideas" ADD COLUMN "CategoryData" text NULL;
CREATE INDEX "IX_Ideas_Category" ON "Ideas" ("Category");
```

---

## New Model Class: CategoryDefinitions

**File**: `src/InnovatEPAM.Portal/Models/CategoryDefinitions.cs`

This is a static, code-only class (no database mapping). It is the single source of truth for category metadata used by validators, services, and views.

### CategoryFieldDefinition

```csharp
public class CategoryFieldDefinition
{
    public string Key { get; init; }           // ViewModel property name
    public string Label { get; init; }         // Display label
    public string InputType { get; init; }     // "select", "text", "textarea"
    public List<string> Options { get; init; } // For select inputs
    public bool IsRequired { get; init; }
    public int MaxLength { get; init; }
    public string GuidanceHint { get; init; }
}
```

### CategoryDefinition

```csharp
public class CategoryDefinition
{
    public string Key { get; init; }
    public string DisplayName { get; init; }
    public List<CategoryFieldDefinition> Fields { get; init; }
}
```

### CategoryDefinitions (static registry)

```csharp
public static class CategoryDefinitions
{
    public static readonly IReadOnlyDictionary<string, CategoryDefinition> All = ...

    // Keys for direct reference
    public const string TechnicalImprovement = "TechnicalImprovement";
    public const string ProcessImprovement    = "ProcessImprovement";
    public const string ClientSolution        = "ClientSolution";
}
```

### Field Definitions per Category

**Technical Improvement** (`Key = "TechnicalImprovement"`, `DisplayName = "Technical Improvement"`):

| Field Key | Label | Type | Options | Required | MaxLength | Guidance |
|---|---|---|---|---|---|---|
| `TechArea` | Technology Area | select | Backend, Frontend, Infrastructure, Security, Data/Analytics, Other | Yes | — | Select the primary technology domain your idea addresses. |
| `TechEffort` | Estimated Implementation Effort | select | Small — days, Medium — weeks, Large — months | Yes | — | Estimate the engineering effort needed to implement this idea. |
| `TechBenefit` | Expected Technical Benefit | textarea | — | Yes | 500 | Describe the measurable technical improvement: performance gain, reliability, maintainability, or security improvement. |

**Process Improvement** (`Key = "ProcessImprovement"`, `DisplayName = "Process Improvement"`):

| Field Key | Label | Type | Options | Required | MaxLength | Guidance |
|---|---|---|---|---|---|---|
| `ProcDepartment` | Affected Department or Team | text | — | Yes | 100 | Name the team or department that would benefit most from this improvement. |
| `ProcPainPoint` | Current Process Pain Point | textarea | — | Yes | 500 | Describe the specific inefficiency, bottleneck, or friction point that this idea addresses. |
| `ProcSavings` | Estimated Savings | text | — | No | 200 | Optionally estimate time or cost savings (e.g., "2 hours/week per team member"). |

**Client Solution** (`Key = "ClientSolution"`, `DisplayName = "Client Solution"`):

| Field Key | Label | Type | Options | Required | MaxLength | Guidance |
|---|---|---|---|---|---|---|
| `ClientSegment` | Target Client Segment | text | — | Yes | 200 | Name the client type or segment this solution is designed for. |
| `ClientProblem` | Client Problem Being Solved | textarea | — | Yes | 500 | Describe the client's unmet need or pain point that this idea addresses. |
| `ClientImpact` | Expected Business Impact | text | — | Yes | 300 | Describe the measurable business outcome for the client (revenue, retention, satisfaction). |

---

## Updated DTOs

### IdeaListItemDTO (extended)

```csharp
public class IdeaListItemDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public string? Category { get; set; }       // NEW — null → shown as "Uncategorized"
    public string? CategoryDisplayName { get; set; } // NEW — resolved from CategoryDefinitions
    public DateTime CreatedDate { get; set; }
    public string SubmitterName { get; set; }
}
```

### IdeaDetailDTO (extended)

```csharp
public class IdeaDetailDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; }
    public string? Category { get; set; }                            // NEW
    public string? CategoryDisplayName { get; set; }                // NEW
    public Dictionary<string, string> CategoryDataFields { get; set; } = new(); // NEW — deserialized
    public string SubmitterName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public List<IdeaAttachmentDTO> Attachments { get; set; }
    public List<AuditLogDTO> AuditHistory { get; set; }
}
```

---

## Updated ViewModels

### CreateIdeaViewModel (extended)

```csharp
public class CreateIdeaViewModel
{
    // EXISTING
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; }
    [StringLength(2000)]
    public string? Description { get; set; }
    public IFormFile? Attachment { get; set; }

    // NEW — Category selector
    public string? Category { get; set; }

    // NEW — Technical Improvement fields
    public string? TechArea { get; set; }
    public string? TechEffort { get; set; }
    public string? TechBenefit { get; set; }

    // NEW — Process Improvement fields
    public string? ProcDepartment { get; set; }
    public string? ProcPainPoint { get; set; }
    public string? ProcSavings { get; set; }

    // NEW — Client Solution fields
    public string? ClientSegment { get; set; }
    public string? ClientProblem { get; set; }
    public string? ClientImpact { get; set; }
}
```

### AdminIdeaListViewModel (extended)

```csharp
public class AdminIdeaListViewModel
{
    public List<IdeaListItemDTO> Ideas { get; set; }
    public string? StatusFilter { get; set; }
    public string? CategoryFilter { get; set; }             // NEW
    public List<string> AvailableStatuses { get; set; }
    public List<string> AvailableCategories { get; set; }   // NEW
    public Dictionary<string, int> StatusSummary { get; set; }
}
```

### IdeaListViewModel (extended)

```csharp
public class IdeaListViewModel
{
    public List<IdeaListItemDTO> Ideas { get; set; }
    public string? StatusFilter { get; set; }
    public string? CategoryFilter { get; set; }             // NEW
    public List<string> AvailableStatuses { get; set; }
    public List<string> AvailableCategories { get; set; }   // NEW
}
```

---

## Validation Rules (CreateIdeaValidator additions)

```
Category:
  - Required → "Please select a category."

When Category == "TechnicalImprovement":
  - TechArea: Required → "Technology Area is required."
  - TechEffort: Required → "Estimated Effort is required."
  - TechBenefit: Required, MaxLength(500) → "Expected Technical Benefit is required." / "Max 500 characters."

When Category == "ProcessImprovement":
  - ProcDepartment: Required, MaxLength(100) → "Affected Department or Team is required." / "Max 100 characters."
  - ProcPainPoint: Required, MaxLength(500) → "Current Process Pain Point is required." / "Max 500 characters."
  - ProcSavings: Optional, MaxLength(200) → "Max 200 characters."

When Category == "ClientSolution":
  - ClientSegment: Required, MaxLength(200) → "Target Client Segment is required." / "Max 200 characters."
  - ClientProblem: Required, MaxLength(500) → "Client Problem Being Solved is required." / "Max 500 characters."
  - ClientImpact: Required, MaxLength(300) → "Expected Business Impact is required." / "Max 300 characters."
```

---

## IService Interface Changes

```csharp
// Updated signatures (default parameters maintain backward compatibility)
Task<List<IdeaListItemDTO>> GetMyIdeasAsync(
    Guid submitterId, string? statusFilter, string? categoryFilter = null);

Task<List<IdeaListItemDTO>> GetAllIdeasAsync(
    string? statusFilter, string? categoryFilter = null);
```

---

## CategoryData Serialization Contract

When an idea is created with `Category = "TechnicalImprovement"` and the user fills in all fields, `CategoryData` stores:

```json
{
  "TechArea": "Backend",
  "TechEffort": "Medium — weeks",
  "TechBenefit": "Reduces API response time by 40% through connection pooling."
}
```

When an idea is created with `Category = "ProcessImprovement"` and `ProcSavings` is left empty (optional):

```json
{
  "ProcDepartment": "Engineering",
  "ProcPainPoint": "Manual deployment process takes 3 hours per release.",
  "ProcSavings": ""
}
```

Display logic: empty string values are skipped in the detail view (not shown to user).

---

**Version**: 1.0.0 | **Created**: 2026-05-14
