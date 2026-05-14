using InnovatEPAM.Portal.DTOs;
using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Services.Interfaces;

public interface IIdeaService
{
    Task<(bool Success, string? Error, Guid IdeaId)> CreateIdeaAsync(Guid submitterId, CreateIdeaViewModel vm);
    Task<List<IdeaListItemDTO>> GetMyIdeasAsync(Guid submitterId, string? statusFilter);
    Task<IdeaDetailDTO?> GetIdeaDetailAsync(Guid ideaId, Guid userId, bool isAdmin);
    Task<List<IdeaListItemDTO>> GetAllIdeasAsync(string? statusFilter);
    Task<(bool Success, string? Error)> UpdateStatusAsync(Guid ideaId, string newStatus, Guid adminId);
    Task<(string FileName, byte[] Data, string ContentType)?> DownloadAttachmentAsync(Guid attachmentId, Guid userId, bool isAdmin);
}
