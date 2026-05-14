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
    /// <summary>Innovation category key. Required; drives dynamic form field visibility.</summary>
    public string? Category { get; set; }

    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "Attachment (PDF, DOC, DOCX, XLS, XLSX, JPG, PNG — max 10 MB)")]
    public IFormFile? Attachment { get; set; }

    // Technical Improvement fields
    public string? TechArea { get; set; }
    public string? TechEffort { get; set; }
    public string? TechBenefit { get; set; }

    // Process Improvement fields
    public string? ProcDepartment { get; set; }
    public string? ProcPainPoint { get; set; }
    public string? ProcSavings { get; set; }

    // Client Solution fields
    public string? ClientSegment { get; set; }
    public string? ClientProblem { get; set; }
    public string? ClientImpact { get; set; }
}

public class AdminIdeaListViewModel
{
    public List<IdeaListItemDTO> Ideas { get; set; } = new();
    public string? StatusFilter { get; set; }
    public List<string> AvailableStatuses { get; set; } = new();
    public Dictionary<string, int> StatusSummary { get; set; } = new();

    /// <summary>Selected category filter key. Null = all categories (admin only, S1).</summary>
    public string? CategoryFilter { get; set; }

    /// <summary>All available category keys and display names for the filter dropdown.</summary>
    public Dictionary<string, string> AvailableCategories { get; set; } = new();
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
