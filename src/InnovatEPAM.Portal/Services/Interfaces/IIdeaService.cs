using InnovatEPAM.Portal.DTOs;
using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Services.Interfaces;

public interface IIdeaService
{
    Task<(bool Success, string? Error, Guid IdeaId)> CreateIdeaAsync(Guid submitterId, CreateIdeaViewModel vm);
    Task<List<IdeaListItemDTO>> GetMyIdeasAsync(Guid submitterId, string? statusFilter);
    Task<IdeaDetailDTO?> GetIdeaDetailAsync(Guid ideaId, Guid userId, bool isAdmin);
    /// <summary>
    /// Returns all ideas visible to admins, optionally filtered by status and category key.
    /// Each DTO includes a resolved <c>CategoryDisplayName</c>.
    /// </summary>
    Task<List<IdeaListItemDTO>> GetAllIdeasAsync(string? statusFilter, string? categoryFilter = null, string? searchQuery = null);
    Task<(bool Success, string? Error)> UpdateStatusAsync(Guid ideaId, string newStatus, Guid adminId);
    Task<(string FileName, byte[] Data, string ContentType)?> DownloadAttachmentAsync(Guid attachmentId, Guid userId, bool isAdmin);

    /// <summary>
    /// Saves a new draft idea without applying required-field validation.
    /// Any data provided is accepted as-is; only file MIME/size is validated.
    /// </summary>
    /// <param name="submitterId">The ID of the submitter creating the draft.</param>
    /// <param name="vm">The Create form view model containing partial idea data.</param>
    /// <returns>Success flag, optional error message, and the new draft's ID.</returns>
    Task<(bool Success, string? Error, Guid DraftId)> SaveDraftAsync(Guid submitterId, CreateIdeaViewModel vm);

    /// <summary>
    /// Updates an existing draft's fields and attachment without required-field validation.
    /// Validates ownership and that the idea is still in Draft status before modifying.
    /// </summary>
    /// <param name="draftId">The ID of the draft to update.</param>
    /// <param name="submitterId">The ID of the submitter; must match the draft's owner.</param>
    /// <param name="vm">The edit draft view model containing updated field values.</param>
    /// <returns>Success flag and optional error message.</returns>
    Task<(bool Success, string? Error)> UpdateDraftAsync(Guid draftId, Guid submitterId, EditDraftViewModel vm);

    /// <summary>
    /// Saves the latest draft state and transitions it to <see cref="IdeaStatus.Submitted"/>.
    /// The controller is responsible for running <c>EditDraftValidator</c> before calling this method.
    /// </summary>
    /// <param name="draftId">The ID of the draft to submit.</param>
    /// <param name="submitterId">The ID of the submitter; must match the draft's owner.</param>
    /// <param name="vm">The edit draft view model with final field values.</param>
    /// <returns>Success flag and optional error message.</returns>
    Task<(bool Success, string? Error)> SubmitDraftAsync(Guid draftId, Guid submitterId, EditDraftViewModel vm);

    /// <summary>
    /// Permanently deletes a draft and all associated attachment files from disk and the database.
    /// Only the owning submitter may delete a draft.
    /// </summary>
    /// <param name="draftId">The ID of the draft to delete.</param>
    /// <param name="submitterId">The ID of the submitter; must match the draft's owner.</param>
    /// <returns>Success flag and optional error message.</returns>
    Task<(bool Success, string? Error)> DeleteDraftAsync(Guid draftId, Guid submitterId);

    /// <summary>
    /// Permanently removes an idea owned by the submitter when allowed (draft, triage-only submission, or accepted/rejected).
    /// </summary>
    Task<(bool Success, string? Error)> DeleteMyIdeaAsync(Guid ideaId, Guid submitterId);

    /// <summary>
    /// Updates a submitted idea while still in triage (no review stage, no scores).
    /// </summary>
    Task<(bool Success, string? Error)> UpdateSubmittedIdeaAsync(Guid ideaId, Guid submitterId, EditDraftViewModel vm);

    /// <summary>
    /// Permanently deletes a submitted idea before review engagement.
    /// </summary>
    Task<(bool Success, string? Error)> WithdrawSubmittedIdeaAsync(Guid ideaId, Guid submitterId);
}
