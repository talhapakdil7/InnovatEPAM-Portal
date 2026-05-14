# Data Model: Idea Scoring System

**Feature**: `006-idea-scoring-system`
**Phase**: 1 — Design

---

## §1 — Entity: `IdeaScore`

One row per `(IdeaId, AdminId)` pair. Stores a single admin's per-dimension scores for one idea.

### C# Model

```csharp
namespace InnovatEPAM.Portal.Models;

/// <summary>
/// Records one admin's evaluation scores for a single idea across the four fixed dimensions.
/// Composite primary key: (IdeaId, AdminId).
/// Partial scoring is supported — any dimension may be null when skipped.
/// </summary>
public class IdeaScore
{
    public Guid IdeaId { get; set; }
    public Idea Idea { get; set; } = null!;

    public Guid AdminId { get; set; }
    public ApplicationUser Admin { get; set; } = null!;

    /// <summary>Score for the Innovation dimension (1–5, nullable = not scored).</summary>
    public int? Innovation { get; set; }

    /// <summary>Score for the Technical Feasibility dimension (1–5, nullable = not scored).</summary>
    public int? TechnicalFeasibility { get; set; }

    /// <summary>Score for the Business Impact dimension (1–5, nullable = not scored).</summary>
    public int? BusinessImpact { get; set; }

    /// <summary>Score for the Implementation Value dimension (1–5, nullable = not scored).</summary>
    public int? ImplementationValue { get; set; }

    /// <summary>UTC timestamp when this record was first created.</summary>
    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the last update to any dimension on this record.</summary>
    public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
}
```

### EF Core Configuration

```csharp
builder.Entity<IdeaScore>(entity =>
{
    entity.ToTable("IdeaScores");
    entity.HasKey(s => new { s.IdeaId, s.AdminId });   // composite PK

    entity.HasOne(s => s.Idea)
          .WithMany(i => i.Scores)
          .HasForeignKey(s => s.IdeaId)
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(s => s.Admin)
          .WithMany()
          .HasForeignKey(s => s.AdminId)
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasIndex(s => s.IdeaId);
    entity.HasIndex(s => s.AdminId);
});
```

### `Idea` entity additions

```csharp
// Add to Idea.cs:
public ICollection<IdeaScore> Scores { get; set; } = new List<IdeaScore>();
```

---

## §2 — DTOs

### `ScoreSummaryDTO` *(derived / not persisted)*

```csharp
namespace InnovatEPAM.Portal.DTOs;

/// <summary>
/// Computed aggregate of all IdeaScore records for one idea.
/// Never persisted — recalculated on every request.
/// </summary>
public class ScoreSummaryDTO
{
    /// <summary>Number of admins who have submitted at least one scored dimension.</summary>
    public int ScorerCount { get; set; }

    /// <summary>Per-dimension average scores (null when no admin has scored that dimension).</summary>
    public decimal? AvgInnovation { get; set; }
    public decimal? AvgTechnicalFeasibility { get; set; }
    public decimal? AvgBusinessImpact { get; set; }
    public decimal? AvgImplementationValue { get; set; }

    /// <summary>
    /// Overall average across all non-null dimension averages.
    /// Null when no dimension has been scored by any admin.
    /// </summary>
    public decimal? OverallAverage { get; set; }

    /// <summary>
    /// Individual admin score rows (visible only in admin detail view;
    /// scorer names are masked when blind review is active).
    /// </summary>
    public List<AdminScoreRowDTO> AdminScores { get; set; } = new();
}

/// <summary>One admin's score row as displayed in the admin detail breakdown table.</summary>
public class AdminScoreRowDTO
{
    /// <summary>Admin's full name (or "Anonymous Reviewer" when blind review is active).</summary>
    public string AdminName { get; set; } = string.Empty;
    public int? Innovation { get; set; }
    public int? TechnicalFeasibility { get; set; }
    public int? BusinessImpact { get; set; }
    public int? ImplementationValue { get; set; }
    public decimal? RowAverage { get; set; }
    public DateTime SubmittedDate { get; set; }
}
```

### `IdeaListItemDTO` additions

```csharp
// Append to existing IdeaListItemDTO:
/// <summary>Overall aggregate score (null = no scores yet).</summary>
public decimal? AggregateScore { get; set; }

/// <summary>Number of admins who have scored this idea.</summary>
public int ScorerCount { get; set; }
```

### `IdeaDetailDTO` additions

```csharp
// Append to existing IdeaDetailDTO:
/// <summary>Full score summary including per-dimension averages and admin breakdown.</summary>
public ScoreSummaryDTO? ScoreSummary { get; set; }

/// <summary>Current requesting admin's own IdeaScore (null if not yet scored). Null for submitter views.</summary>
public AdminScoreRowDTO? MyScore { get; set; }
```

---

## §3 — Repository Interface: `IIdeaScoreRepository`

```csharp
namespace InnovatEPAM.Portal.Repositories.Interfaces;

/// <summary>
/// Data access for IdeaScore records.
/// Supports upsert, delete, and bulk-read patterns required by IScoreService.
/// </summary>
public interface IIdeaScoreRepository
{
    /// <summary>Returns the score submitted by <paramref name="adminId"/> for <paramref name="ideaId"/>, or null.</summary>
    Task<IdeaScore?> GetAsync(Guid ideaId, Guid adminId);

    /// <summary>Returns all score records for <paramref name="ideaId"/>, including Admin navigation property.</summary>
    Task<List<IdeaScore>> GetAllForIdeaAsync(Guid ideaId);

    /// <summary>Inserts or updates the admin's score record for the given idea.</summary>
    Task UpsertAsync(IdeaScore score);

    /// <summary>Deletes the score record for (ideaId, adminId). No-op if record does not exist.</summary>
    Task DeleteAsync(Guid ideaId, Guid adminId);
}
```

---

## §4 — Service Interface: `IScoreService`

```csharp
namespace InnovatEPAM.Portal.Services.Interfaces;

/// <summary>
/// Manages the full idea scoring lifecycle: submit, update, retract, and aggregate calculation.
/// Applies blind review masking to scorer names when active.
/// </summary>
public interface IScoreService
{
    /// <summary>
    /// Submits or updates the calling admin's score for an idea.
    /// Throws if the idea status does not permit scoring (Draft, Accepted, Rejected).
    /// </summary>
    Task SubmitScoreAsync(Guid ideaId, Guid adminId, SubmitScoreViewModel vm);

    /// <summary>
    /// Retracts the calling admin's score for an idea.
    /// No-op if the admin has not previously scored this idea.
    /// </summary>
    Task RetractScoreAsync(Guid ideaId, Guid adminId);

    /// <summary>
    /// Returns the full score summary (aggregates + admin breakdown) for an idea.
    /// Applies blind review masking to scorer names when <paramref name="isBlindReviewActive"/> is true.
    /// </summary>
    Task<ScoreSummaryDTO> GetScoreSummaryAsync(Guid ideaId, bool isBlindReviewActive);

    /// <summary>
    /// Returns the calling admin's own score record for a given idea, or null if not yet scored.
    /// Used to pre-populate the scoring form.
    /// </summary>
    Task<IdeaScore?> GetMyScoreAsync(Guid ideaId, Guid adminId);

    /// <summary>
    /// Returns a dictionary of overall aggregate scores keyed by IdeaId.
    /// Used to populate aggregate score columns in the admin list view efficiently.
    /// </summary>
    Task<Dictionary<Guid, (decimal? OverallAverage, int ScorerCount)>> GetAggregatesForIdeasAsync(IEnumerable<Guid> ideaIds);
}
```

---

## §5 — ViewModels

### `SubmitScoreViewModel`

```csharp
namespace InnovatEPAM.Portal.ViewModels;

/// <summary>
/// Form model for an admin submitting or updating their idea score.
/// All four dimension scores are optional (partial scoring permitted per FR-003).
/// At least one dimension must be non-null (server-side validation).
/// </summary>
public class SubmitScoreViewModel
{
    public Guid IdeaId { get; set; }

    [Range(1, 5)] public int? Innovation { get; set; }
    [Range(1, 5)] public int? TechnicalFeasibility { get; set; }
    [Range(1, 5)] public int? BusinessImpact { get; set; }
    [Range(1, 5)] public int? ImplementationValue { get; set; }
}
```

### `AdminIdeaDetailViewModel` additions

```csharp
// Append to existing AdminIdeaDetailViewModel:
/// <summary>Full score summary for this idea (null if no admin has scored yet).</summary>
public ScoreSummaryDTO? ScoreSummary { get; set; }

/// <summary>The viewing admin's own current score, pre-populated into the scoring form.</summary>
public SubmitScoreViewModel ScoreForm { get; set; } = new();

/// <summary>True when the idea's status allows scoring (not Draft, Accepted, or Rejected).</summary>
public bool IsScoringAllowed { get; set; }
```

### `AdminIdeaListViewModel` additions

```csharp
// IdeaListItemDTO already carries AggregateScore + ScorerCount (see §2).
// No additional ViewModel changes required.
```

---

## §6 — FluentValidation: `SubmitScoreValidator`

```csharp
namespace InnovatEPAM.Portal.Validators;

public class SubmitScoreValidator : AbstractValidator<SubmitScoreViewModel>
{
    public SubmitScoreValidator()
    {
        // At least one dimension must be scored
        RuleFor(x => x)
            .Must(x => x.Innovation.HasValue || x.TechnicalFeasibility.HasValue
                    || x.BusinessImpact.HasValue || x.ImplementationValue.HasValue)
            .WithMessage("At least one evaluation dimension must be scored.");

        // Range validation for each provided score
        When(x => x.Innovation.HasValue, () =>
            RuleFor(x => x.Innovation!.Value).InclusiveBetween(1, 5)
                .WithMessage("Innovation score must be between 1 and 5."));

        When(x => x.TechnicalFeasibility.HasValue, () =>
            RuleFor(x => x.TechnicalFeasibility!.Value).InclusiveBetween(1, 5)
                .WithMessage("Technical Feasibility score must be between 1 and 5."));

        When(x => x.BusinessImpact.HasValue, () =>
            RuleFor(x => x.BusinessImpact!.Value).InclusiveBetween(1, 5)
                .WithMessage("Business Impact score must be between 1 and 5."));

        When(x => x.ImplementationValue.HasValue, () =>
            RuleFor(x => x.ImplementationValue!.Value).InclusiveBetween(1, 5)
                .WithMessage("Implementation Value score must be between 1 and 5."));
    }
}
```

---

## §7 — Aggregate Calculation Logic

`ScoreService.GetScoreSummaryAsync` computes the following in the application layer:

```
For each dimension D in {Innovation, TechnicalFeasibility, BusinessImpact, ImplementationValue}:
    AvgD = AVG of non-null D values across all IdeaScore rows for the idea
         = null if all values for D are null

OverallAverage = mean of all non-null AvgD values
               = null if no dimension has been scored by any admin

ScorerCount = count of IdeaScore rows for the idea (one row per admin, regardless of partial scoring)

RowAverage (per AdminScoreRowDTO) = mean of non-null dimension values in that row
```

Displayed precision: `OverallAverage` and dimension averages are rounded to 2 decimal places for display.

---

## §8 — Migrations

One new migration: **`AddIdeaScores`**

Changes:
- New table: `IdeaScores` (composite PK, FK to `Ideas`, FK to `Users`)
- New index: `IdeaScores.IdeaId`
- New index: `IdeaScores.AdminId`
- `Ideas` table: no schema change (navigation property only, no new columns)
