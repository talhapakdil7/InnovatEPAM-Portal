using InnovatEPAM.Portal.DTOs;
using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Repositories.Interfaces;
using InnovatEPAM.Portal.Services.Interfaces;

namespace InnovatEPAM.Portal.Services;

/// <summary>
/// Implements blind review mode: reads and writes the system-wide setting,
/// and applies presentation-layer identity masking to idea DTOs.
/// </summary>
public class BlindReviewService : IBlindReviewService
{
    private const string AnonymousLabel = "Anonymous Submitter";

    private readonly ISystemSettingRepository _settingRepo;
    private readonly ILogger<BlindReviewService> _logger;

    public BlindReviewService(
        ISystemSettingRepository settingRepo,
        ILogger<BlindReviewService> logger)
    {
        _settingRepo = settingRepo;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<bool> IsEnabledAsync()
    {
        var setting = await _settingRepo.GetByKeyAsync(SystemSettingKeys.BlindReviewEnabled);
        return string.Equals(setting?.Value, "true", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public async Task SetEnabledAsync(bool enabled, Guid adminId)
    {
        var setting = new SystemSetting
        {
            Key = SystemSettingKeys.BlindReviewEnabled,
            Value = enabled ? "true" : "false",
            LastModifiedDate = DateTime.UtcNow,
            LastModifiedByAdminId = adminId
        };

        await _settingRepo.UpsertAsync(setting);

        _logger.LogInformation(
            "Blind review mode {State} by admin {AdminId}",
            enabled ? "enabled" : "disabled",
            adminId);
    }

    /// <inheritdoc/>
    public bool ShouldRevealIdentity(string ideaStatus) =>
        ideaStatus is "Accepted" or "Rejected";

    /// <inheritdoc/>
    public void ApplyMasking(IdeaDetailDTO dto, bool isBlindReviewEnabled)
    {
        if (isBlindReviewEnabled && !ShouldRevealIdentity(dto.Status))
            dto.SubmitterName = AnonymousLabel;
    }

    /// <inheritdoc/>
    public void ApplyMasking(IEnumerable<IdeaListItemDTO> dtos, bool isBlindReviewEnabled)
    {
        foreach (var dto in dtos)
        {
            if (isBlindReviewEnabled && !ShouldRevealIdentity(dto.Status))
                dto.SubmitterName = AnonymousLabel;
        }
    }
}
