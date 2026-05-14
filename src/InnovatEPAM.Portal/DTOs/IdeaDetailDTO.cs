namespace InnovatEPAM.Portal.DTOs;

public class IdeaDetailDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string SubmitterName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public List<IdeaAttachmentDTO> Attachments { get; set; } = new();
    public List<AuditLogDTO> AuditHistory { get; set; } = new();

    /// <summary>Raw category key. Null for pre-feature ideas.</summary>
    public string? Category { get; set; }

    /// <summary>Human-readable category name resolved by the service layer.</summary>
    public string? CategoryDisplayName { get; set; }

    /// <summary>
    /// Category-specific field answers keyed by their human-readable label.
    /// Example: {"Technology Area": "Backend", "Estimated Implementation Effort": "Medium — weeks"}.
    /// Null when no category is assigned.
    /// </summary>
    public Dictionary<string, string>? CategoryDataFields { get; set; }

    // ── Multi-stage review fields ──

    /// <summary>Human-readable name of the current review stage, or "Pending Review" when none assigned.</summary>
    public string CurrentReviewStageName { get; set; } = "Pending Review";

    /// <summary>Integer order of the current review stage (1–4), or 0 when no stage is assigned.</summary>
    public int CurrentReviewStageOrder { get; set; }

    /// <summary>Total number of stages in the pipeline (constant = 4).</summary>
    public int TotalReviewStages { get; set; } = 4;

    /// <summary>True when the idea is at the last review stage (Final Decision).</summary>
    public bool IsAtFinalStage { get; set; }

    /// <summary>Ordered list of all stage transitions recorded for this idea, oldest first.</summary>
    public List<StageTransitionDTO> StageHistory { get; set; } = new();

    /// <summary>Full score summary including per-dimension averages and admin breakdown. Null when no admin has scored yet.</summary>
    public ScoreSummaryDTO? ScoreSummary { get; set; }

    /// <summary>The requesting admin's own current score record. Null for submitter views or when admin has not yet scored.</summary>
    public AdminScoreRowDTO? MyScore { get; set; }
}

public class IdeaAttachmentDTO
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedDate { get; set; }
}

public class AuditLogDTO
{
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string ChangedByAdmin { get; set; } = string.Empty;
    public DateTime ChangedDate { get; set; }
}
