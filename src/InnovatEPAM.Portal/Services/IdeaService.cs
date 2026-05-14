using System.Text.Json;
using AutoMapper;
using InnovatEPAM.Portal.Data;
using InnovatEPAM.Portal.DTOs;
using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Repositories.Interfaces;
using InnovatEPAM.Portal.Services.Interfaces;
using InnovatEPAM.Portal.Utilities;
using InnovatEPAM.Portal.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InnovatEPAM.Portal.Services;

public class IdeaService : IIdeaService
{
    private readonly IIdeaRepository _ideaRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<IdeaService> _logger;

    public IdeaService(
        IIdeaRepository ideaRepo,
        IAuditLogRepository auditRepo,
        ApplicationDbContext db,
        IMapper mapper,
        IWebHostEnvironment env,
        ILogger<IdeaService> logger)
    {
        _ideaRepo = ideaRepo;
        _auditRepo = auditRepo;
        _db = db;
        _mapper = mapper;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new innovation idea, serializes category-specific field answers into JSON,
    /// and persists any uploaded attachment to secure storage.
    /// </summary>
    public async Task<(bool Success, string? Error, Guid IdeaId)> CreateIdeaAsync(Guid submitterId, CreateIdeaViewModel vm)
    {
        var idea = new Idea
        {
            Id = Guid.NewGuid(),
            Title = vm.Title,
            Description = vm.Description,
            SubmitterId = submitterId,
            Status = IdeaStatus.Submitted,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            Category = vm.Category,
            CategoryData = BuildCategoryData(vm)
        };

        if (vm.Attachment != null)
        {
            var detectedMime = await FileStorageHelper.DetectMimeTypeAsync(vm.Attachment);
            if (!FileStorageHelper.IsAllowedMimeType(detectedMime))
                return (false, "File type is not allowed (MIME validation failed).", Guid.Empty);

            var relativePath = FileStorageHelper.GetSecureStoragePath(idea.Id, vm.Attachment.FileName);
            var absolutePath = Path.Combine(_env.ContentRootPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

            await using var stream = new FileStream(absolutePath, FileMode.Create);
            await vm.Attachment.CopyToAsync(stream);

            idea.IdeaAttachments.Add(new IdeaAttachment
            {
                Id = Guid.NewGuid(),
                IdeaId = idea.Id,
                FileName = vm.Attachment.FileName,
                FilePath = relativePath,
                FileSize = vm.Attachment.Length,
                UploadedDate = DateTime.UtcNow
            });
        }

        await _ideaRepo.AddAsync(idea);
        _logger.LogInformation("Idea {IdeaId} created by {SubmitterId}", idea.Id, submitterId);
        return (true, null, idea.Id);
    }

    /// <summary>
    /// Serializes category-specific field answers from the view model into a JSON string.
    /// Returns null when no category is selected or the category key is unrecognized.
    /// </summary>
    private static string? BuildCategoryData(CreateIdeaViewModel vm)
    {
        if (string.IsNullOrEmpty(vm.Category) || !CategoryDefinitions.All.ContainsKey(vm.Category))
            return null;

        var data = new Dictionary<string, string?>();

        switch (vm.Category)
        {
            case CategoryDefinitions.TechnicalImprovement:
                data["TechArea"] = vm.TechArea;
                data["TechEffort"] = vm.TechEffort;
                data["TechBenefit"] = vm.TechBenefit;
                break;

            case CategoryDefinitions.ProcessImprovement:
                data["ProcDepartment"] = vm.ProcDepartment;
                data["ProcPainPoint"] = vm.ProcPainPoint;
                data["ProcSavings"] = vm.ProcSavings;
                break;

            case CategoryDefinitions.ClientSolution:
                data["ClientSegment"] = vm.ClientSegment;
                data["ClientProblem"] = vm.ClientProblem;
                data["ClientImpact"] = vm.ClientImpact;
                break;
        }

        return JsonSerializer.Serialize(data);
    }

    /// <summary>
    /// Enriches a list of DTOs by resolving each item's CategoryDisplayName from the static registry.
    /// </summary>
    private static void EnrichCategoryDisplayNames(IEnumerable<IdeaListItemDTO> dtos)
    {
        foreach (var dto in dtos)
        {
            if (dto.Category != null && CategoryDefinitions.All.TryGetValue(dto.Category, out var def))
                dto.CategoryDisplayName = def.DisplayName;
        }
    }

    public async Task<List<IdeaListItemDTO>> GetMyIdeasAsync(Guid submitterId, string? statusFilter)
    {
        var ideas = await _ideaRepo.GetBySubmitterAsync(submitterId);

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<IdeaStatus>(statusFilter, out var status))
            ideas = ideas.Where(i => i.Status == status).ToList();

        var dtos = _mapper.Map<List<IdeaListItemDTO>>(ideas);
        EnrichCategoryDisplayNames(dtos);
        return dtos;
    }

    public async Task<IdeaDetailDTO?> GetIdeaDetailAsync(Guid ideaId, Guid userId, bool isAdmin)
    {
        var idea = await _ideaRepo.GetByIdAsync(ideaId);
        if (idea == null) return null;
        if (!isAdmin && idea.SubmitterId != userId) return null;

        var dto = _mapper.Map<IdeaDetailDTO>(idea);

        if (idea.Category != null && CategoryDefinitions.All.TryGetValue(idea.Category, out var catDef))
        {
            dto.CategoryDisplayName = catDef.DisplayName;

            if (!string.IsNullOrEmpty(idea.CategoryData))
            {
                var raw = JsonSerializer.Deserialize<Dictionary<string, string>>(idea.CategoryData)
                          ?? new Dictionary<string, string>();

                dto.CategoryDataFields = catDef.Fields
                    .Where(f => raw.TryGetValue(f.Key, out var val) && !string.IsNullOrEmpty(val))
                    .ToDictionary(f => f.Label, f => raw[f.Key]);
            }
        }

        return dto;
    }

    public async Task<List<IdeaListItemDTO>> GetAllIdeasAsync(string? statusFilter, string? categoryFilter = null)
    {
        var ideas = await _ideaRepo.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<IdeaStatus>(statusFilter, out var status))
            ideas = ideas.Where(i => i.Status == status).ToList();

        if (!string.IsNullOrWhiteSpace(categoryFilter) && CategoryDefinitions.All.ContainsKey(categoryFilter))
            ideas = ideas.Where(i => i.Category == categoryFilter).ToList();

        var dtos = _mapper.Map<List<IdeaListItemDTO>>(ideas);
        EnrichCategoryDisplayNames(dtos);
        return dtos;
    }

    public async Task<(bool Success, string? Error)> UpdateStatusAsync(Guid ideaId, string newStatus, Guid adminId)
    {
        if (!Enum.TryParse<IdeaStatus>(newStatus, out var parsedStatus))
            return (false, "Invalid status value.");

        var idea = await _ideaRepo.GetByIdAsync(ideaId);
        if (idea == null) return (false, "Idea not found.");

        var oldStatus = idea.Status.ToString();
        idea.Status = parsedStatus;
        idea.LastModifiedByAdminId = adminId;
        idea.LastModifiedDate = DateTime.UtcNow;

        await _ideaRepo.UpdateAsync(idea);

        await _auditRepo.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            IdeaId = ideaId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedByAdminId = adminId,
            ChangedDate = DateTime.UtcNow
        });

        _logger.LogInformation("Idea {IdeaId} status changed from {Old} to {New} by admin {AdminId}",
            ideaId, oldStatus, newStatus, adminId);

        return (true, null);
    }

    public async Task<(string FileName, byte[] Data, string ContentType)?> DownloadAttachmentAsync(
        Guid attachmentId, Guid userId, bool isAdmin)
    {
        var attachment = await _db.IdeaAttachments
            .Include(a => a.Idea)
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        if (attachment == null) return null;
        if (!isAdmin && attachment.Idea.SubmitterId != userId) return null;

        var absolutePath = Path.Combine(_env.ContentRootPath, attachment.FilePath);
        if (!File.Exists(absolutePath)) return null;

        var data = await File.ReadAllBytesAsync(absolutePath);
        var extension = Path.GetExtension(attachment.FileName).ToLowerInvariant();
        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };

        return (attachment.FileName, data, contentType);
    }
}
