using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Services.Interfaces;
using InnovatEPAM.Portal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InnovatEPAM.Portal.Controllers;

/// <summary>
/// Handles idea score submission and retraction by admins.
/// All actions require the Admin role (FR-011).
/// </summary>
[Authorize(Roles = "Admin")]
public class ScoreController : Controller
{
    private readonly IScoreService _scoreService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ScoreController(IScoreService scoreService, UserManager<ApplicationUser> userManager)
    {
        _scoreService = scoreService;
        _userManager = userManager;
    }

    /// <summary>
    /// POST — submits or updates the calling admin's score for an idea.
    /// Redirects back to the admin detail page on success or validation failure.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitScoreViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please correct the validation errors before saving your score.";
            return RedirectToAction("Detail", "Admin", new { id = vm.IdeaId });
        }

        var adminId = Guid.Parse(_userManager.GetUserId(User)!);

        try
        {
            await _scoreService.SubmitScoreAsync(vm.IdeaId, adminId, vm);
            TempData["Success"] = "Your score has been saved.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Detail", "Admin", new { id = vm.IdeaId });
    }

    /// <summary>
    /// POST — retracts the calling admin's score for the specified idea.
    /// Silent no-op when no score record exists.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken, Route("Score/Retract/{ideaId}")]
    public async Task<IActionResult> Retract(Guid ideaId)
    {
        var adminId = Guid.Parse(_userManager.GetUserId(User)!);
        await _scoreService.RetractScoreAsync(ideaId, adminId);
        TempData["Success"] = "Your score has been removed.";
        return RedirectToAction("Detail", "Admin", new { id = ideaId });
    }
}
