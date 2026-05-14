namespace InnovatEPAM.Portal.DTOs;

public class CreateIdeaDTO
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IFormFile? Attachment { get; set; }
}
