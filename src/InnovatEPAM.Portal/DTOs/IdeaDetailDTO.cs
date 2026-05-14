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
