using InnovatEPAM.Portal.DTOs;
using System.ComponentModel.DataAnnotations;

namespace InnovatEPAM.Portal.ViewModels;

public class IdeaListViewModel
{
    public List<IdeaListItemDTO> Ideas { get; set; } = new();
    public string? StatusFilter { get; set; }
    public List<string> AvailableStatuses { get; set; } = new();
}

public class IdeaDetailViewModel
{
    public IdeaDetailDTO Idea { get; set; } = null!;
    public bool IsAdmin { get; set; }
}

public class CreateIdeaViewModel
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "Attachment (PDF, DOC, DOCX, XLS, XLSX, JPG, PNG — max 10 MB)")]
    public IFormFile? Attachment { get; set; }
}

public class AdminIdeaListViewModel
{
    public List<IdeaListItemDTO> Ideas { get; set; } = new();
    public string? StatusFilter { get; set; }
    public List<string> AvailableStatuses { get; set; } = new();
    public Dictionary<string, int> StatusSummary { get; set; } = new();
}

public class AdminIdeaDetailViewModel
{
    public IdeaDetailDTO Idea { get; set; } = null!;
    public List<string> AllowedStatuses { get; set; } = new();
}

public class UpdateStatusViewModel
{
    public Guid IdeaId { get; set; }

    [Required]
    public string NewStatus { get; set; } = string.Empty;
}
