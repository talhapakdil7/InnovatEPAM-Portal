namespace InnovatEPAM.Portal.Models;

public enum IdeaStatus
{
    /// <summary>Saved by the submitter but not yet submitted to admin review.</summary>
    Draft = 0,
    Submitted = 1,
    UnderReview = 2,
    Accepted = 3,
    Rejected = 4
}

/// <summary>
/// Innovation idea submitted by an employee.
/// </summary>
public class Idea
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IdeaStatus Status { get; set; } = IdeaStatus.Submitted;

    /// <summary>
    /// Innovation category key. Null for ideas submitted before this feature was introduced (displayed as "Uncategorized").
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// JSON-serialized key-value pairs of category-specific field answers. Null when no category is assigned.
    /// </summary>
    public string? CategoryData { get; set; }

    public Guid SubmitterId { get; set; }
    public ApplicationUser Submitter { get; set; } = null!;

    public Guid? LastModifiedByAdminId { get; set; }
    public ApplicationUser? UpdatedByAdmin { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    public ICollection<IdeaAttachment> IdeaAttachments { get; set; } = new List<IdeaAttachment>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    /// <summary>Per-admin evaluation scores submitted for this idea.</summary>
    public ICollection<IdeaScore> Scores { get; set; } = new List<IdeaScore>();
}
