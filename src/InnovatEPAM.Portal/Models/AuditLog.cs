namespace InnovatEPAM.Portal.Models;

/// <summary>
/// Audit trail entry for every idea status change performed by an admin.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid IdeaId { get; set; }
    public Idea Idea { get; set; } = null!;

    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;

    public Guid ChangedByAdminId { get; set; }
    public ApplicationUser ChangedByAdmin { get; set; } = null!;

    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
}
