using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Repositories.Interfaces;
using InnovatEPAM.Portal.Services.Interfaces;
using InnovatEPAM.Portal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InnovatEPAM.Portal.Controllers;

/// <summary>
/// Manages system-wide admin settings, including blind review mode.
/// All actions require the Admin role.
/// </summary>
[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly IBlindReviewService _blindReviewService;
    private readonly ISystemSettingRepository _settingRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    public SettingsController(
        IBlindReviewService blindReviewService,
        ISystemSettingRepository settingRepo,
        UserManager<ApplicationUser> userManager)
    {
        _blindReviewService = blindReviewService;
        _settingRepo = settingRepo;
        _userManager = userManager;
    }

    /// <summary>GET — displays the blind review settings page with current state.</summary>
    public async Task<IActionResult> BlindReview()
    {
        var isEnabled = await _blindReviewService.IsEnabledAsync();
        var setting = await _settingRepo.GetByKeyAsync(SystemSettingKeys.BlindReviewEnabled);

        var vm = new BlindReviewSettingsViewModel
        {
            IsEnabled = isEnabled,
            LastModifiedDate = setting?.LastModifiedDate,
            LastModifiedByAdminName = setting?.LastModifiedByAdmin?.FullName
        };

        return View(vm);
    }

    /// <summary>POST — saves the blind review mode toggle and redirects back to the settings page.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BlindReview(BlindReviewSettingsViewModel vm)
    {
        var adminId = Guid.Parse(_userManager.GetUserId(User)!);
        await _blindReviewService.SetEnabledAsync(vm.IsEnabled, adminId);

        TempData["Success"] = vm.IsEnabled
            ? "Blind review mode has been enabled. Submitter identities are now hidden during evaluation."
            : "Blind review mode has been disabled. Submitter identities are now visible.";

        return RedirectToAction(nameof(BlindReview));
    }
}
