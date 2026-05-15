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
        for (var i = 0; i < ideas.Count; i++)
        {
            dtos[i].CanDeleteAsOwner = OwnerMayDeleteIdea(ideas[i]);
            dtos[i].DeleteBlockedHint = dtos[i].CanDeleteAsOwner ? null : DeleteBlockedHint(ideas[i]);
        }

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

        dto.StageHistory = idea.AuditLogs
            .OrderBy(t => t.ChangedDate)
            .Select(t => _mapper.Map<AuditLogDTO>(t))
            .ToList();

        if (!isAdmin)
            dto.CanAmendSubmitted = SubmitterMayAmendSubmitted(idea);

        if (!isAdmin)
            dto.CanDeleteAsOwner = OwnerMayDeleteIdea(idea);

        return dto;
    }

    /// <summary>
    /// Submitter may edit/withdraw a submitted idea only before any reviewer scores it.
    /// </summary>
    private static bool SubmitterMayAmendSubmitted(Idea idea)
    {
        if (idea.Status != IdeaStatus.Submitted) return false;
        if (idea.Scores.Count > 0) return false;
        return true;
    }

    /// <summary>
    /// Whether the submitter may permanently remove this idea (draft, triage-only submission, or finished decision).
    /// </summary>
    private static bool OwnerMayDeleteIdea(Idea idea) => idea.Status switch
    {
        IdeaStatus.Draft => true,
        IdeaStatus.Submitted => SubmitterMayAmendSubmitted(idea),
        IdeaStatus.UnderReview => false,
        IdeaStatus.Accepted => true,
        IdeaStatus.Rejected => true,
        _ => false
    };

    private static string DeleteBlockedHint(Idea idea) => idea.Status switch
    {
        IdeaStatus.UnderReview => "Cannot delete while this idea is under review.",
        IdeaStatus.Submitted => "Review has started; contact an administrator if you need it removed.",
        _ => "This idea cannot be deleted right now."
    };

    private async Task RemoveIdeaCoreAsync(Idea idea)
    {
        foreach (var att in idea.IdeaAttachments)
        {
            var absPath = Path.Combine(_env.ContentRootPath, att.FilePath);
            if (File.Exists(absPath)) File.Delete(absPath);
        }

        _db.Set<Idea>().Remove(idea);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Permanently removes an idea owned by the submitter when policy allows (see <see cref="OwnerMayDeleteIdea"/>).
    /// </summary>
    public async Task<(bool Success, string? Error)> DeleteMyIdeaAsync(Guid ideaId, Guid submitterId)
    {
        var idea = await _ideaRepo.GetByIdAsync(ideaId);
        if (idea == null || idea.SubmitterId != submitterId)
            return (false, "Not found or access denied.");
        if (!OwnerMayDeleteIdea(idea))
            return (false, DeleteBlockedHint(idea));

        await RemoveIdeaCoreAsync(idea);
        _logger.LogInformation("Idea {IdeaId} deleted by owner {SubmitterId}", ideaId, submitterId);
        return (true, null);
    }

    /// <summary>
    /// Returns all ideas visible to admins, optionally filtered by status and category key.
    /// Draft ideas (Status == 0) are always excluded regardless of the statusFilter argument (FR-010).
    /// Each DTO includes a resolved CategoryDisplayName.
    /// </summary>
    public async Task<List<IdeaListItemDTO>> GetAllIdeasAsync(string? statusFilter, string? categoryFilter = null, string? searchQuery = null)
    {
        var ideas = await _ideaRepo.GetAllAsync();

        // FR-010: admins must never see draft ideas
        ideas = ideas.Where(i => i.Status != IdeaStatus.Draft).ToList();

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<IdeaStatus>(statusFilter, out var status))
            ideas = ideas.Where(i => i.Status == status).ToList();

        if (!string.IsNullOrWhiteSpace(categoryFilter) && CategoryDefinitions.All.ContainsKey(categoryFilter))
            ideas = ideas.Where(i => i.Category == categoryFilter).ToList();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var q = searchQuery.Trim();
            ideas = ideas.Where(i =>
                    i.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (i.Description != null && i.Description.Contains(q, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        var dtos = _mapper.Map<List<IdeaListItemDTO>>(ideas);
        EnrichCategoryDisplayNames(dtos);
        return dtos;
    }

    /// <summary>
    /// Saves a new draft idea without applying required-field validation.
    /// Only file MIME type and size are validated.
    /// </summary>
    public async Task<(bool Success, string? Error, Guid DraftId)> SaveDraftAsync(Guid submitterId, CreateIdeaViewModel vm)
    {
        var idea = new Idea
        {
            Id = Guid.NewGuid(),
            Title = vm.Title,
            Description = vm.Description,
            SubmitterId = submitterId,
            Status = IdeaStatus.Draft,
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
        _logger.LogInformation("Draft {DraftId} saved by {SubmitterId}", idea.Id, submitterId);
        return (true, null, idea.Id);
    }

    /// <summary>
    /// Updates an existing draft's fields and attachment without required-field validation.
    /// Validates ownership and Draft status before applying changes.
    /// </summary>
    public async Task<(bool Success, string? Error)> UpdateDraftAsync(Guid draftId, Guid submitterId, EditDraftViewModel vm)
    {
        var idea = await _ideaRepo.GetByIdAsync(draftId);
        if (idea == null || idea.Status != IdeaStatus.Draft || idea.SubmitterId != submitterId)
            return (false, "Not found or access denied.");

        var applied = await ApplyEditViewModelToIdeaAsync(idea, vm);
        if (!applied.Success) return applied;

        try
        {
            await _ideaRepo.UpdateAsync(idea);
            _logger.LogInformation("Draft {DraftId} updated by {SubmitterId}", draftId, submitterId);
            return (true, null);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Concurrency error updating draft {DraftId}: {Message}", draftId, ex.Message);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateSubmittedIdeaAsync(
        Guid ideaId, Guid submitterId, EditDraftViewModel vm)
    {
        var idea = await _ideaRepo.GetByIdAsync(ideaId);
        if (idea == null || idea.SubmitterId != submitterId)
            return (false, "Not found or access denied.");
        if (!SubmitterMayAmendSubmitted(idea))
            return (false, "This idea can no longer be changed — review has already started.");

        var applied = await ApplyEditViewModelToIdeaAsync(idea, vm);
        if (!applied.Success) return applied;

        try
        {
            await _ideaRepo.UpdateAsync(idea);
            _logger.LogInformation("Submitted idea {IdeaId} updated by submitter {SubmitterId} before review", ideaId, submitterId);
            return (true, null);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Concurrency error updating submitted idea {IdeaId}: {Message}", ideaId, ex.Message);
            return (false, ex.Message);
        }
    }

    /// <summary>Shared body for draft updates and pre-review submitted edits.</summary>
    private async Task<(bool Success, string? Error)> ApplyEditViewModelToIdeaAsync(Idea idea, EditDraftViewModel vm)
    {
        idea.Title = vm.Title;
        idea.Description = vm.Description;
        idea.Category = vm.Category;
        idea.CategoryData = BuildCategoryDataFromEdit(vm);
        idea.LastModifiedDate = DateTime.UtcNow;

        if (vm.RemoveAttachment && idea.IdeaAttachments.Any())
        {
            foreach (var att in idea.IdeaAttachments.ToList())
            {
                var absPath = Path.Combine(_env.ContentRootPath, att.FilePath);
                if (File.Exists(absPath)) File.Delete(absPath);
            }
            _db.Set<IdeaAttachment>().RemoveRange(idea.IdeaAttachments);
            idea.IdeaAttachments.Clear();
        }

        if (vm.Attachment != null)
        {
            var detectedMime = await FileStorageHelper.DetectMimeTypeAsync(vm.Attachment);
            if (!FileStorageHelper.IsAllowedMimeType(detectedMime))
                return (false, "File type is not allowed (MIME validation failed).");

            foreach (var att in idea.IdeaAttachments.ToList())
            {
                var absPath = Path.Combine(_env.ContentRootPath, att.FilePath);
                if (File.Exists(absPath)) File.Delete(absPath);
            }
            _db.Set<IdeaAttachment>().RemoveRange(idea.IdeaAttachments);
            idea.IdeaAttachments.Clear();

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

        return (true, null);
    }

    /// <summary>
    /// Saves the latest draft state and transitions it to Submitted status.
    /// The controller must run EditDraftValidator and confirm ModelState.IsValid before calling this.
    /// </summary>
    public async Task<(bool Success, string? Error)> SubmitDraftAsync(Guid draftId, Guid submitterId, EditDraftViewModel vm)
    {
        var (updateSuccess, updateError) = await UpdateDraftAsync(draftId, submitterId, vm);
        if (!updateSuccess) return (false, updateError);

        var idea = await _ideaRepo.GetByIdAsync(draftId);
        if (idea == null) return (false, "Draft not found after update.");

        idea.Status = IdeaStatus.Submitted;
        idea.LastModifiedDate = DateTime.UtcNow;
        
        try
        {
            await _ideaRepo.UpdateAsync(idea);
            _logger.LogInformation("Draft {DraftId} submitted by {SubmitterId}", draftId, submitterId);
            return (true, null);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Concurrency error submitting draft {DraftId}: {Message}", draftId, ex.Message);
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Permanently deletes a draft and all associated attachment files from disk and the database.
    /// Only the owning submitter may delete a draft.
    /// </summary>
    public async Task<(bool Success, string? Error)> DeleteDraftAsync(Guid draftId, Guid submitterId)
    {
        var idea = await _ideaRepo.GetByIdAsync(draftId);
        if (idea == null || idea.Status != IdeaStatus.Draft || idea.SubmitterId != submitterId)
            return (false, "Not found or access denied.");

        await RemoveIdeaCoreAsync(idea);
        _logger.LogInformation("Draft {DraftId} deleted by {SubmitterId}", draftId, submitterId);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> WithdrawSubmittedIdeaAsync(Guid ideaId, Guid submitterId)
    {
        var idea = await _ideaRepo.GetByIdAsync(ideaId);
        if (idea == null || idea.SubmitterId != submitterId)
            return (false, "Not found or access denied.");
        if (!SubmitterMayAmendSubmitted(idea))
            return (false, "This idea can no longer be withdrawn — review has already started.");

        await RemoveIdeaCoreAsync(idea);
        _logger.LogInformation("Submitted idea {IdeaId} withdrawn by submitter {SubmitterId}", ideaId, submitterId);
        return (true, null);
    }

    /// <summary>
    /// Serializes category-specific field answers from an <see cref="EditDraftViewModel"/> into a JSON string.
    /// Returns null when no category is selected or the category key is unrecognized.
    /// </summary>
    private static string? BuildCategoryDataFromEdit(EditDraftViewModel vm)
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

    public async Task<(bool Success, string? Error)> UpdateStatusAsync(Guid ideaId, string newStatus, Guid adminId)
    {
        if (!Enum.TryParse<IdeaStatus>(newStatus, out var parsedStatus))
            return (false, "Invalid status value.");

        if (parsedStatus is IdeaStatus.Draft)
            return (false, "Draft cannot be assigned from the admin panel.");

        var idea = await _ideaRepo.GetByIdAsync(ideaId);
        if (idea == null) return (false, "Idea not found.");

        if (idea.Status is IdeaStatus.Accepted or IdeaStatus.Rejected)
            return (false, "Decided ideas cannot be changed.");

        if (idea.Status == parsedStatus)
            return (true, null);

        var oldStatus = idea.Status.ToString();
        idea.Status = parsedStatus;
        idea.LastModifiedByAdminId = adminId;
        idea.LastModifiedDate = DateTime.UtcNow;

        try
        {
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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Concurrency error updating status for idea {IdeaId}: {Message}", ideaId, ex.Message);
            return (false, ex.Message);
        }
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
