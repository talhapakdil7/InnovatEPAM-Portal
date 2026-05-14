using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Services.Interfaces;
using InnovatEPAM.Portal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using InnovatEPAM.Portal.DTOs;

namespace InnovatEPAM.Portal.Controllers;

[Authorize]
public class IdeasController : Controller
{
    private readonly IIdeaService _ideaService;
    private readonly IScoreService _scoreService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdeasController(
        IIdeaService ideaService,
        IScoreService scoreService,
        UserManager<ApplicationUser> userManager)
    {
        _ideaService = ideaService;
        _scoreService = scoreService;
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

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDraft(CreateIdeaViewModel vm)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error, draftId) = await _ideaService.SaveDraftAsync(userId, vm);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to save draft.");
            return View("Create", vm);
        }

        TempData["Success"] = "Draft saved.";
        return RedirectToAction(nameof(Edit), new { id = draftId });
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var isAdmin = User.IsInRole("Admin");
        var idea = await _ideaService.GetIdeaDetailAsync(id, userId, isAdmin);

        if (idea == null) return NotFound();

        // Submitter sees only the overall aggregate (no breakdown, no scorer names — FR-009)
        var scoreSummary = await _scoreService.GetScoreSummaryAsync(id, isBlindReviewActive: false);

        return View(new IdeaDetailViewModel
        {
            Idea = idea,
            IsAdmin = isAdmin,
            IsDraft = idea.Status == "Draft",
            AggregateScore = scoreSummary.OverallAverage,
            ScorerCount = scoreSummary.ScorerCount
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDraft(Guid id)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error) = await _ideaService.DeleteDraftAsync(id, userId);

        if (!success)
            TempData["Error"] = error ?? "Failed to delete draft.";
        else
            TempData["Success"] = "Draft deleted.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var idea = await _ideaService.GetIdeaDetailAsync(id, userId, isAdmin: false);

        if (idea == null || idea.Status != "Draft") return NotFound();

        var vm = new EditDraftViewModel
        {
            Id = id,
            Category = idea.Category,
            Title = idea.Title,
            Description = idea.Description,
            ExistingAttachment = idea.Attachments.FirstOrDefault()
        };

        // Populate category fields from CategoryDataFields (key = label, value = answer)
        if (idea.CategoryDataFields != null && idea.Category != null
            && CategoryDefinitions.All.TryGetValue(idea.Category, out var catDef))
        {
            foreach (var field in catDef.Fields)
            {
                var matchingEntry = idea.CategoryDataFields
                    .FirstOrDefault(kv => kv.Key == field.Label);

                if (matchingEntry.Key == null) continue;

                switch (field.Key)
                {
                    case "TechArea":     vm.TechArea = matchingEntry.Value; break;
                    case "TechEffort":   vm.TechEffort = matchingEntry.Value; break;
                    case "TechBenefit":  vm.TechBenefit = matchingEntry.Value; break;
                    case "ProcDepartment": vm.ProcDepartment = matchingEntry.Value; break;
                    case "ProcPainPoint":  vm.ProcPainPoint = matchingEntry.Value; break;
                    case "ProcSavings":    vm.ProcSavings = matchingEntry.Value; break;
                    case "ClientSegment":  vm.ClientSegment = matchingEntry.Value; break;
                    case "ClientProblem":  vm.ClientProblem = matchingEntry.Value; break;
                    case "ClientImpact":   vm.ClientImpact = matchingEntry.Value; break;
                }
            }
        }

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDraft(Guid id, EditDraftViewModel vm)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error) = await _ideaService.UpdateDraftAsync(id, userId, vm);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update draft.");
            vm.Id = id;
            return View("Edit", vm);
        }

        TempData["Success"] = "Draft saved.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitDraft(Guid id, EditDraftViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Id = id;
            var userId2 = Guid.Parse(_userManager.GetUserId(User)!);
            var existing = await _ideaService.GetIdeaDetailAsync(id, userId2, isAdmin: false);
            vm.ExistingAttachment = existing?.Attachments.FirstOrDefault();
            return View("Edit", vm);
        }

        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error) = await _ideaService.SubmitDraftAsync(id, userId, vm);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to submit draft.");
            vm.Id = id;
            return View("Edit", vm);
        }

        TempData["Success"] = "Idea submitted successfully.";
        return RedirectToAction(nameof(Detail), new { id });
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
