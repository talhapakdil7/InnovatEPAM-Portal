# Data Model: Multi-Stage Innovation Review Workflow

**Phase**: 1 — Design
**Feature**: `specs/004-multi-stage-review/spec.md`
**Date**: 2026-05-14

---

## 1. New Enum

### `ReviewStage`

```csharp
// src/InnovatEPAM.Portal/Models/ReviewStage.cs
public enum ReviewStage
{
    InitialScreening           = 1,
    TechnicalReview            = 2,
    BusinessImpactAssessment   = 3,
    FinalDecision              = 4
}
```

**Notes**:
- Values start at 1 (not 0) so that a `NULL` database column unambiguously means "no stage assigned yet"
- Ordering is sequential; the integer value drives advance/revert validation in the service layer
- Adding a new stage in the future requires only a new enum member and a migration column value — no data-model changes to existing records

---

## 2. New Entity: `StageTransition`

```csharp
// src/InnovatEPAM.Portal/Models/StageTransition.cs

/// <summary>
/// Records a single stage transition (advance or revert) in the multi-stage review workflow.
/// Each row is append-only and cannot be deleted or edited.
/// </summary>
public class StageTransition
{
    public Guid Id { get; set; }

    // ── Idea reference ──

    public Guid IdeaId { get; set; }
    public Idea Idea { get; set; } = null!;

    // ── Transition details ──

    /// <summary>Stage the idea was in before this transition. Null when advancing from "no stage".</summary>
    public ReviewStage? FromStage { get; set; }

    /// <summary>Stage the idea moved to.</summary>
    public ReviewStage ToStage { get; set; }

    /// <summary>True = advance (forward), False = revert (backward).</summary>
    public bool IsAdvance { get; set; }

    /// <summary>Optional evaluation notes. Max 1000 characters. Applies to both advances and reverts.</summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Mandatory reason when IsAdvance = false (revert). Max 500 characters.
    /// Null for forward transitions.
    /// </summary>
    public string? RevertReason { get; set; }

    /// <summary>
    /// Final outcome recorded when ToStage = FinalDecision and the admin confirms a decision.
    /// "Accepted" or "Rejected". Null for all other stage transitions.
    /// </summary>
    public string? Outcome { get; set; }

    // ── Audit ──

    public Guid TransitionedByAdminId { get; set; }
    public ApplicationUser TransitionedByAdmin { get; set; } = null!;

    public DateTime TransitionDate { get; set; } = DateTime.UtcNow;
}
```

**Database table**: `StageTransitions`

**Indexes**:
- `IX_StageTransitions_IdeaId` (for history lookup)
- `IX_StageTransitions_TransitionedByAdminId`
- `IX_StageTransitions_TransitionDate`

---

## 3. Updated Entity: `Idea`

```csharp
// src/InnovatEPAM.Portal/Models/Idea.cs — additions only

/// <summary>
/// Current review stage in the evaluation workflow.
/// Null when the idea has not yet been picked up for review.
/// </summary>
public ReviewStage? CurrentReviewStage { get; set; }

public ICollection<StageTransition> StageTransitions { get; set; } = new List<StageTransition>();
```

**Database column on `Ideas` table**:
- Column name: `CurrentReviewStage`
- Type: `integer` (nullable)
- Default: `NULL`

**Migration notes**:
- All existing rows get `NULL` for `CurrentReviewStage` — backward-compatible
- No data migration required

---

## 4. New DTOs

### `StageTransitionDTO`

```csharp
// src/InnovatEPAM.Portal/DTOs/StageTransitionDTO.cs

/// <summary>Read-only representation of a stage transition for display in history views.</summary>
public class StageTransitionDTO
{
    public string FromStageName { get; set; } = string.Empty;   // "None" when first stage
    public string ToStageName { get; set; } = string.Empty;
    public int ToStageOrder { get; set; }                        // 1–4
    public bool IsAdvance { get; set; }
    public string? Notes { get; set; }
    public string? RevertReason { get; set; }
    public string? Outcome { get; set; }
    public string TransitionedByAdmin { get; set; } = string.Empty;
    public DateTime TransitionDate { get; set; }
}
```

---

## 5. Updated DTOs

### `IdeaDetailDTO` — new properties

```csharp
/// <summary>Current review stage name. Null when no stage has been assigned.</summary>
public string? CurrentReviewStageName { get; set; }

/// <summary>Numeric order of the current review stage (1–4). Null when no stage assigned.</summary>
public int? CurrentReviewStageOrder { get; set; }

/// <summary>Full stage transition history, ordered by TransitionDate ascending.</summary>
public List<StageTransitionDTO> StageHistory { get; set; } = new();
```

### `IdeaListItemDTO` — new property

```csharp
/// <summary>Current review stage name for the admin list stage filter. Null when no stage assigned.</summary>
public string? CurrentReviewStageName { get; set; }
```

---

## 6. New ViewModels

### `AdvanceStageViewModel`

```csharp
// src/InnovatEPAM.Portal/ViewModels/ReviewWorkflowViewModels.cs

/// <summary>ViewModel for advancing an idea to the next review stage.</summary>
public class AdvanceStageViewModel
{
    public Guid IdeaId { get; set; }

    /// <summary>Optional evaluation notes. Max 1000 characters.</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}
```

### `RevertStageViewModel`

```csharp
/// <summary>ViewModel for reverting an idea to a previous review stage.</summary>
public class RevertStageViewModel
{
    public Guid IdeaId { get; set; }

    /// <summary>The target stage to revert to.</summary>
    public ReviewStage TargetStage { get; set; }

    /// <summary>Mandatory reason for reverting. Max 500 characters.</summary>
    [Required, StringLength(500, MinimumLength = 1)]
    public string RevertReason { get; set; } = string.Empty;

    /// <summary>Optional evaluation notes. Max 1000 characters.</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}
```

### `RecordDecisionViewModel`

```csharp
/// <summary>ViewModel for recording the final Accepted or Rejected outcome from the Final Decision stage.</summary>
public class RecordDecisionViewModel
{
    public Guid IdeaId { get; set; }

    /// <summary>Final outcome: "Accepted" or "Rejected".</summary>
    [Required]
    public string Outcome { get; set; } = string.Empty;

    /// <summary>Optional evaluation notes. Max 1000 characters.</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}
```

---

## 7. New Service Interface: `IReviewWorkflowService`

```csharp
// src/InnovatEPAM.Portal/Services/Interfaces/IReviewWorkflowService.cs

public interface IReviewWorkflowService
{
    /// <summary>
    /// Advances the idea to the next review stage in sequence.
    /// Automatically sets the overall status to UnderReview if currently Submitted.
    /// </summary>
    Task<(bool Success, string? Error)> AdvanceStageAsync(Guid ideaId, Guid adminId, string? notes);

    /// <summary>
    /// Reverts the idea to the specified previous review stage.
    /// RevertReason is mandatory.
    /// </summary>
    Task<(bool Success, string? Error)> RevertStageAsync(Guid ideaId, ReviewStage targetStage, Guid adminId, string revertReason, string? notes);

    /// <summary>
    /// Records the final decision (Accepted or Rejected) from the FinalDecision stage,
    /// setting the overall idea status to match.
    /// </summary>
    Task<(bool Success, string? Error)> RecordFinalDecisionAsync(Guid ideaId, Guid adminId, string outcome, string? notes);

    /// <summary>
    /// Returns the complete stage transition history for an idea, ordered by TransitionDate ascending.
    /// </summary>
    Task<List<StageTransitionDTO>> GetStageHistoryAsync(Guid ideaId);
}
```

---

## 8. Service Validation Rules

| Operation | Precondition | Notes Validation | Outcome Validation |
|---|---|---|---|
| AdvanceStage | Status ∈ {Submitted, UnderReview} AND Status ∉ {Accepted, Rejected, Draft} AND CurrentStage ≠ FinalDecision | Optional, max 1000 chars | N/A |
| RevertStage | Status = UnderReview AND CurrentStage > InitialScreening AND Status ∉ {Accepted, Rejected} | Optional, max 1000 chars | RevertReason required, max 500 |
| RecordDecision | Status = UnderReview AND CurrentStage = FinalDecision | Optional, max 1000 chars | Must be "Accepted" or "Rejected" |

---

## 9. Stage Helper — `ReviewStageHelper`

```csharp
// src/InnovatEPAM.Portal/Models/ReviewStageHelper.cs

/// <summary>Utility methods for the ReviewStage enum.</summary>
public static class ReviewStageHelper
{
    public static readonly IReadOnlyList<ReviewStage> Stages =
        Enum.GetValues<ReviewStage>().OrderBy(s => (int)s).ToList();

    public static ReviewStage? NextStage(ReviewStage current) =>
        Stages.Cast<ReviewStage?>().FirstOrDefault(s => s.HasValue && (int)s.Value == (int)current + 1);

    public static bool IsFirstStage(ReviewStage stage) => stage == ReviewStage.InitialScreening;
    public static bool IsLastStage(ReviewStage stage)  => stage == ReviewStage.FinalDecision;

    public static string DisplayName(ReviewStage stage) => stage switch
    {
        ReviewStage.InitialScreening         => "Initial Screening",
        ReviewStage.TechnicalReview          => "Technical Review",
        ReviewStage.BusinessImpactAssessment => "Business Impact Assessment",
        ReviewStage.FinalDecision            => "Final Decision",
        _ => stage.ToString()
    };

    public static string DisplayName(ReviewStage? stage) =>
        stage.HasValue ? DisplayName(stage.Value) : "Pending Review";
}
```

---

## 10. EF Core Migration

**Migration name**: `AddStageTransitions`

Changes:
1. Add nullable column `CurrentReviewStage integer` to `Ideas` table
2. Create `StageTransitions` table with all columns and foreign keys listed above
3. Add indexes on `StageTransitions`

No breaking changes to existing data. All existing `Ideas` rows get `CurrentReviewStage = NULL`.
