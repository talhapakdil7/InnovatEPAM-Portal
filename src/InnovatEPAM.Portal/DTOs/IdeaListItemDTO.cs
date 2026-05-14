namespace InnovatEPAM.Portal.DTOs;

public class IdeaListItemDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string SubmitterName { get; set; } = string.Empty;

    /// <summary>Raw category key stored in the database. Null for pre-feature ideas.</summary>
    public string? Category { get; set; }

    /// <summary>Human-readable category name resolved by the service layer. Null for uncategorized ideas.</summary>
    public string? CategoryDisplayName { get; set; }
}
