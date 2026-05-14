namespace InnovatEPAM.Portal.DTOs;

public class IdeaListItemDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string SubmitterName { get; set; } = string.Empty;
}
