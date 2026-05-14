using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Services.Interfaces;
using InnovatEPAM.Portal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InnovatEPAM.Portal.Controllers;

[Authorize]
public class IdeasController : Controller
{
    private readonly IIdeaService _ideaService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdeasController(IIdeaService ideaService, UserManager<ApplicationUser> userManager)
    {
        _ideaService = ideaService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? statusFilter)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var ideas = await _ideaService.GetMyIdeasAsync(userId, statusFilter);

        var vm = new IdeaListViewModel
        {
            Ideas = ideas,
            StatusFilter = statusFilter,
            AvailableStatuses = Enum.GetNames<IdeaStatus>().ToList()
        };
        return View(vm);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateIdeaViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateIdeaViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error, ideaId) = await _ideaService.CreateIdeaAsync(userId, vm);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to create idea.");
            return View(vm);
        }

        TempData["Success"] = "Idea submitted successfully.";
        return RedirectToAction(nameof(Detail), new { id = ideaId });
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var isAdmin = User.IsInRole("Admin");
        var idea = await _ideaService.GetIdeaDetailAsync(id, userId, isAdmin);

        if (idea == null) return NotFound();

        return View(new IdeaDetailViewModel { Idea = idea, IsAdmin = isAdmin });
    }

    public async Task<IActionResult> Download(Guid attachmentId)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _ideaService.DownloadAttachmentAsync(attachmentId, userId, isAdmin);

        if (result == null) return NotFound();

        return File(result.Value.Data, result.Value.ContentType, result.Value.FileName);
    }
}
