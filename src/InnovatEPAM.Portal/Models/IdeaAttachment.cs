namespace InnovatEPAM.Portal.Models;

/// <summary>
/// File attachment associated with an innovation idea.
/// </summary>
public class IdeaAttachment
{
    public Guid Id { get; set; }
    public Guid IdeaId { get; set; }
    public Idea Idea { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
}
