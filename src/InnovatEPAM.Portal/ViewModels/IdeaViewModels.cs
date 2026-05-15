using InnovatEPAM.Portal.DTOs;
using System.ComponentModel.DataAnnotations;

namespace InnovatEPAM.Portal.ViewModels;

public class IdeaListViewModel
{
    /// <summary>Submitted / in-review / decided ideas (never drafts). Filtered by <see cref="StatusFilter"/>.</summary>
    public List<IdeaListItemDTO> Ideas { get; set; } = new();

    /// <summary>All draft ideas for this user, always shown in the drafts section.</summary>
    public List<IdeaListItemDTO> DraftIdeas { get; set; } = new();

    /// <summary>Pipeline-only filter (Submitted, UnderReview, …). Never <c>Draft</c>.</summary>
    public string? StatusFilter { get; set; }
    public List<string> AvailableStatuses { get; set; } = new();

    /// <summary>Per-status counts for the submitter’s full portfolio (unfiltered). Keys match <see cref="Models.IdeaStatus"/> names.</summary>
    public Dictionary<string, int> StatusCounts { get; set; } = new();
}

public class IdeaDetailViewModel
{
    public IdeaDetailDTO Idea { get; set; } = null!;
    public bool IsAdmin { get; set; }

    /// <summary>True when the idea is in Draft status, enabling edit/delete actions in the view.</summary>
    public bool IsDraft { get; set; }

    /// <summary>Overall aggregate score visible to the submitter (null = not yet scored).</summary>
    public decimal? AggregateScore { get; set; }

    /// <summary>Number of admins who have scored this idea.</summary>
    public int ScorerCount { get; set; }
}

public class CreateIdeaViewModel
{
    /// <summary>Innovation category key. Required; drives dynamic form field visibility.</summary>
    public string? Category { get; set; }

    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "Attachment (PDF, DOC, DOCX, XLS, XLSX, JPG, PNG — max 10 MB)")]
    public IFormFile? Attachment { get; set; }

    // Technical Improvement fields
    public string? TechArea { get; set; }
    public string? TechEffort { get; set; }
    public string? TechBenefit { get; set; }

    // Process Improvement fields
    public string? ProcDepartment { get; set; }
    public string? ProcPainPoint { get; set; }
    public string? ProcSavings { get; set; }

    // Client Solution fields
    public string? ClientSegment { get; set; }
    public string? ClientProblem { get; set; }
    public string? ClientImpact { get; set; }
}

public class AdminIdeaListViewModel
{
    public List<IdeaListItemDTO> Ideas { get; set; } = new();
    /// <summary>Full-text filter on title/description (admin queue search).</summary>
    public string? SearchQuery { get; set; }
    public string? StatusFilter { get; set; }
    public List<string> AvailableStatuses { get; set; } = new();
    public Dictionary<string, int> StatusSummary { get; set; } = new();

    /// <summary>Selected category filter key. Null = all categories (admin only, S1).</summary>
    public string? CategoryFilter { get; set; }

    /// <summary>All available category keys and display names for the filter dropdown.</summary>
    public Dictionary<string, string> AvailableCategories { get; set; } = new();

    /// <summary>True when blind review mode is globally active; drives the info banner in the view.</summary>
    public bool IsBlindReviewActive { get; set; }
}

public class AdminIdeaDetailViewModel
{
    public IdeaDetailDTO Idea { get; set; } = null!;
    /// <summary>Statuses shown in the lifecycle dropdown — only triage vs in-flight.</summary>
    public List<string> AllowedStatuses { get; set; } = new();

    /// <summary>False when Accepted/Rejected — use Record final decision, not manual status.</summary>
    public bool CanManualLifecycleEdit { get; set; }

    /// <summary>True when blind review mode is globally active; drives the info banner in the view.</summary>
    public bool IsBlindReviewActive { get; set; }

    /// <summary>Full score summary for the idea. Null when no admin has scored yet — view shows "No scores yet".</summary>
    public ScoreSummaryDTO? ScoreSummary { get; set; }

    /// <summary>The viewing admin's own current score, pre-populated into the scoring form.</summary>
    public SubmitScoreViewModel ScoreForm { get; set; } = new();

    /// <summary>
    /// True when the idea's status allows scoring (Submitted or UnderReview).
    /// False for Draft, Accepted, or Rejected — form is replaced with a read-only badge.
    /// </summary>
    public bool IsScoringAllowed { get; set; }
}

public class UpdateStatusViewModel
{
    public Guid IdeaId { get; set; }

    [Required]
    public string NewStatus { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for editing and saving/submitting an existing draft idea.
/// </summary>
public class EditDraftViewModel
{
    /// <summary>The ID of the draft being edited.</summary>
    public Guid Id { get; set; }

    /// <summary>Innovation category key. Drives dynamic form field visibility.</summary>
    public string? Category { get; set; }

    /// <summary>Idea title. Required only on submit; optional for draft save.</summary>
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional extended description of the idea.</summary>
    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>Existing attachment to display. Null when the draft has no attachment.</summary>
    public IdeaAttachmentDTO? ExistingAttachment { get; set; }

    /// <summary>When true, the existing attachment is removed on save.</summary>
    public bool RemoveAttachment { get; set; }

    /// <summary>New file to upload; replaces existing attachment when provided.</summary>
    [Display(Name = "New Attachment (PDF, DOC, DOCX, XLS, XLSX, JPG, PNG — max 10 MB)")]
    public IFormFile? Attachment { get; set; }

    // ── Technical Improvement fields ──

    /// <summary>Technology area selection for Technical Improvement category.</summary>
    public string? TechArea { get; set; }

    /// <summary>Estimated implementation effort for Technical Improvement category.</summary>
    public string? TechEffort { get; set; }

    /// <summary>Expected technical benefit description for Technical Improvement category.</summary>
    public string? TechBenefit { get; set; }

    // ── Process Improvement fields ──

    /// <summary>Affected department or team for Process Improvement category.</summary>
    public string? ProcDepartment { get; set; }

    /// <summary>Description of the current process pain point.</summary>
    public string? ProcPainPoint { get; set; }

    /// <summary>Optional estimated savings for Process Improvement category.</summary>
    public string? ProcSavings { get; set; }

    // ── Client Solution fields ──

    /// <summary>Target client segment for Client Solution category.</summary>
    public string? ClientSegment { get; set; }

    /// <summary>Description of the client problem being solved.</summary>
    public string? ClientProblem { get; set; }

    /// <summary>Expected business impact of the client solution.</summary>
    public string? ClientImpact { get; set; }

    /// <summary>When true, the form edits a submitted idea (pre-review window) instead of a draft.</summary>
    public bool IsSubmittedEditable { get; set; }
}
