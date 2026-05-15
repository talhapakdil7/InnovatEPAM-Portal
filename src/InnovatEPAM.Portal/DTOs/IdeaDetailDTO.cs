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

    /// <summary>Status audit history for this idea.</summary>
    public List<AuditLogDTO> StageHistory { get; set; } = new();

    /// <summary>Full score summary including per-dimension averages and admin breakdown. Null when no admin has scored yet.</summary>
    public ScoreSummaryDTO? ScoreSummary { get; set; }

    /// <summary>The requesting admin's own current score record. Null for submitter views or when admin has not yet scored.</summary>
    public AdminScoreRowDTO? MyScore { get; set; }

    /// <summary>True when the submitter may edit this submission before it enters pipeline stages.</summary>
    public bool CanAmendSubmitted { get; set; }

    /// <summary>True when the owning submitter may delete or withdraw.</summary>
    public bool CanDeleteAsOwner { get; set; }

    /// <summary>Short reason when <see cref="CanDeleteAsOwner"/> is false.</summary>
    public string? DeleteBlockedHint { get; set; }
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
