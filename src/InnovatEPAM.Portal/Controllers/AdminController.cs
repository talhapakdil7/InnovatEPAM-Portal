using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Services.Interfaces;
using InnovatEPAM.Portal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InnovatEPAM.Portal.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IIdeaService _ideaService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(IIdeaService ideaService, UserManager<ApplicationUser> userManager)
    {
        _ideaService = ideaService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? statusFilter, string? categoryFilter)
    {
        var ideas = await _ideaService.GetAllIdeasAsync(statusFilter, categoryFilter);

        var statusSummary = ideas
            .GroupBy(i => i.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var vm = new AdminIdeaListViewModel
        {
            Ideas = ideas,
            StatusFilter = statusFilter,
            CategoryFilter = categoryFilter,
            AvailableStatuses = Enum.GetNames<IdeaStatus>().ToList(),
            AvailableCategories = CategoryDefinitions.All.ToDictionary(kv => kv.Key, kv => kv.Value.DisplayName),
            StatusSummary = statusSummary
        };
        return View(vm);
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var idea = await _ideaService.GetIdeaDetailAsync(id, userId, isAdmin: true);

        if (idea == null) return NotFound();

        var allowedStatuses = Enum.GetNames<IdeaStatus>().ToList();
        return View(new AdminIdeaDetailViewModel { Idea = idea, AllowedStatuses = allowedStatuses });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(UpdateStatusViewModel vm)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Detail), new { id = vm.IdeaId });

        var adminId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error) = await _ideaService.UpdateStatusAsync(vm.IdeaId, vm.NewStatus, adminId);

        if (!success)
            TempData["Error"] = error;
        else
            TempData["Success"] = "Status updated successfully.";

        return RedirectToAction(nameof(Detail), new { id = vm.IdeaId });
    }
}
