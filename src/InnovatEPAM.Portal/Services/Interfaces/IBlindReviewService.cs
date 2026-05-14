using InnovatEPAM.Portal.DTOs;

namespace InnovatEPAM.Portal.Services.Interfaces;

/// <summary>
/// Manages the blind review mode global setting and applies submitter identity masking
/// to idea DTOs before they are rendered in admin-facing views.
/// Masking is presentation-layer only — underlying data is never altered.
/// </summary>
public interface IBlindReviewService
{
    /// <summary>
    /// Returns <c>true</c> when blind review mode is currently enabled system-wide.
    /// </summary>
    Task<bool> IsEnabledAsync();

    /// <summary>
    /// Enables or disables blind review mode and persists the change immediately.
    /// </summary>
    /// <param name="enabled">The desired state.</param>
    /// <param name="adminId">The admin who is making the change (for audit).</param>
    Task SetEnabledAsync(bool enabled, Guid adminId);

    /// <summary>
    /// Masks the submitter identity fields of a single idea detail DTO when blind review
    /// is active and the idea has not yet reached a concluded status.
    /// Mutates <paramref name="dto"/> in-place; does not persist any data.
    /// </summary>
    /// <param name="dto">The DTO to mask.</param>
    /// <param name="isBlindReviewEnabled">Current global blind review flag.</param>
    void ApplyMasking(IdeaDetailDTO dto, bool isBlindReviewEnabled);

    /// <summary>
    /// Masks submitter identity fields across a collection of list-item DTOs.
    /// Mutates each item in-place; does not persist any data.
    /// </summary>
    /// <param name="dtos">The DTOs to mask.</param>
    /// <param name="isBlindReviewEnabled">Current global blind review flag.</param>
    void ApplyMasking(IEnumerable<IdeaListItemDTO> dtos, bool isBlindReviewEnabled);

    /// <summary>
    /// Returns <c>true</c> when the idea's status indicates that evaluation has concluded
    /// (Accepted or Rejected), meaning submitter identity should be visible even during
    /// blind review mode.
    /// </summary>
    bool ShouldRevealIdentity(string ideaStatus);
}
