using Microsoft.AspNetCore.Identity;

namespace InnovatEPAM.Portal.Models;

/// <summary>
/// Custom application user extending ASP.NET Core Identity with employee-specific properties.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<Idea> SubmittedIdeas { get; set; } = new List<Idea>();
    public ICollection<Idea> UpdatedIdeas { get; set; } = new List<Idea>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public string FullName => $"{FirstName} {LastName}";
}
