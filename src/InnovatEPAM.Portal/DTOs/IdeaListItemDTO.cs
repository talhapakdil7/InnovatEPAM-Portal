namespace InnovatEPAM.Portal.DTOs;

public class IdeaListItemDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    /// <summary>UTC last modification (status, stage, or content updates).</summary>
    public DateTime LastModifiedDate { get; set; }

    public string SubmitterName { get; set; } = string.Empty;

    /// <summary>Raw category key stored in the database. Null for pre-feature ideas.</summary>
    public string? Category { get; set; }

    /// <summary>Human-readable category name resolved by the service layer. Null for uncategorized ideas.</summary>
    public string? CategoryDisplayName { get; set; }

    /// <summary>Human-readable current review stage name. Kept for backward compat; always "In Review" when UnderReview.</summary>
    public string CurrentReviewStageName { get; set; } = "";

    /// <summary>Always 0 — stage order removed; kept for view compat during transition.</summary>
    public int CurrentReviewStageOrder { get; set; } = 0;

    /// <summary>Overall aggregate score (null = no scores yet).</summary>
    public decimal? AggregateScore { get; set; }

    /// <summary>Number of admins who have scored this idea.</summary>
    public int ScorerCount { get; set; }

    /// <summary>Number of file attachments.</summary>
    public int AttachmentCount { get; set; }

    /// <summary>True when the owning submitter may delete or withdraw this row from the workspace.</summary>
    public bool CanDeleteAsOwner { get; set; }

    /// <summary>Short reason when <see cref="CanDeleteAsOwner"/> is false.</summary>
    public string? DeleteBlockedHint { get; set; }
}
