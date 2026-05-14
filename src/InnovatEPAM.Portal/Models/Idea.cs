namespace InnovatEPAM.Portal.Models;

public enum IdeaStatus
{
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

    public Guid SubmitterId { get; set; }
    public ApplicationUser Submitter { get; set; } = null!;

    public Guid? LastModifiedByAdminId { get; set; }
    public ApplicationUser? UpdatedByAdmin { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    public ICollection<IdeaAttachment> IdeaAttachments { get; set; } = new List<IdeaAttachment>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
