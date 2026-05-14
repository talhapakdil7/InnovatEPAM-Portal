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
    private readonly IReviewWorkflowService _workflowService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        IIdeaService ideaService,
        IReviewWorkflowService workflowService,
        UserManager<ApplicationUser> userManager)
    {
        _ideaService = ideaService;
        _workflowService = workflowService;
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
            AvailableStatuses = Enum.GetNames<IdeaStatus>().Where(s => s != "Draft").ToList(),
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

        var allowedStatuses = Enum.GetNames<IdeaStatus>().Where(s => s != "Draft").ToList();
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

    // ─────────────────────────────────────────────────────────────────────────
    // Multi-stage review workflow actions
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>GET — confirms advance to the next review stage.</summary>
    public async Task<IActionResult> AdvanceStage(Guid id)
    {
        var adminId = Guid.Parse(_userManager.GetUserId(User)!);
        var idea = await _ideaService.GetIdeaDetailAsync(id, adminId, isAdmin: true);
        if (idea == null) return NotFound();

        var currentStage = idea.CurrentReviewStageOrder > 0
            ? (ReviewStage)idea.CurrentReviewStageOrder
            : (ReviewStage?)null;

        ReviewStage? nextStage = currentStage.HasValue
            ? ReviewStageHelper.NextStage(currentStage.Value)
            : ReviewStage.InitialScreening;

        if (nextStage == null)
        {
            TempData["Error"] = "Idea is already at the Final Decision stage.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var vm = new AdvanceStageViewModel
        {
            IdeaId = id,
            IdeaTitle = idea.Title,
            CurrentStageName = idea.CurrentReviewStageName,
            NextStageName = ReviewStageHelper.DisplayName(nextStage.Value)
        };
        return View(vm);
    }

    /// <summary>POST — performs stage advance.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AdvanceStage(AdvanceStageViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var adminId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error) = await _workflowService.AdvanceStageAsync(vm.IdeaId, adminId, vm);

        if (!success)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Detail), new { id = vm.IdeaId });
        }

        TempData["Success"] = "Idea advanced to the next review stage.";
        return RedirectToAction(nameof(Detail), new { id = vm.IdeaId });
    }

    /// <summary>GET — confirms revert to the previous review stage.</summary>
    public async Task<IActionResult> RevertStage(Guid id)
    {
        var adminId = Guid.Parse(_userManager.GetUserId(User)!);
        var idea = await _ideaService.GetIdeaDetailAsync(id, adminId, isAdmin: true);
        if (idea == null) return NotFound();

        if (idea.CurrentReviewStageOrder == 0)
        {
            TempData["Error"] = "Idea has not yet entered the review pipeline.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var currentStage = (ReviewStage)idea.CurrentReviewStageOrder;
        if (ReviewStageHelper.IsFirstStage(currentStage))
        {
            TempData["Error"] = "Idea is already at the first stage and cannot be reverted.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var previousStage = (ReviewStage)(idea.CurrentReviewStageOrder - 1);

        var vm = new RevertStageViewModel
        {
            IdeaId = id,
            IdeaTitle = idea.Title,
            CurrentStageName = idea.CurrentReviewStageName,
            PreviousStageName = ReviewStageHelper.DisplayName(previousStage)
        };
        return View(vm);
    }

    /// <summary>POST — performs stage revert.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RevertStage(RevertStageViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var adminId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error) = await _workflowService.RevertStageAsync(vm.IdeaId, adminId, vm);

        if (!success)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Detail), new { id = vm.IdeaId });
        }

        TempData["Success"] = "Review stage reverted successfully.";
        return RedirectToAction(nameof(Detail), new { id = vm.IdeaId });
    }

    /// <summary>GET — presents the final decision form.</summary>
    public async Task<IActionResult> RecordDecision(Guid id)
    {
        var adminId = Guid.Parse(_userManager.GetUserId(User)!);
        var idea = await _ideaService.GetIdeaDetailAsync(id, adminId, isAdmin: true);
        if (idea == null) return NotFound();

        if (!idea.IsAtFinalStage)
        {
            TempData["Error"] = "Idea must be at the Final Decision stage to record a decision.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var vm = new RecordDecisionViewModel
        {
            IdeaId = id,
            IdeaTitle = idea.Title
        };
        return View(vm);
    }

    /// <summary>POST — records the final decision (Accepted / Rejected).</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordDecision(RecordDecisionViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var adminId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error) = await _workflowService.RecordDecisionAsync(vm.IdeaId, adminId, vm);

        if (!success)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Detail), new { id = vm.IdeaId });
        }

        TempData["Success"] = $"Final decision '{vm.Outcome}' recorded successfully.";
        return RedirectToAction(nameof(Detail), new { id = vm.IdeaId });
    }

    /// <summary>GET — filters admin idea list by a specific review stage.</summary>
    public async Task<IActionResult> ByStage(int stage)
    {
        var ideas = await _ideaService.GetAllIdeasAsync(statusFilter: null);
        var filtered = ideas.Where(i => i.CurrentReviewStageOrder == stage).ToList();

        var stageName = stage > 0 && Enum.IsDefined(typeof(ReviewStage), stage)
            ? ReviewStageHelper.DisplayName((ReviewStage)stage)
            : "Unknown Stage";

        ViewBag.StageName = stageName;
        ViewBag.StageOrder = stage;
        return View(filtered);
    }
}
