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

    public async Task<IActionResult> Index(string? focus)
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction(nameof(AdminController.Index), "Admin");

        ViewData["IdeasListFocus"] = focus ?? "";

        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var allMine = await _ideaService.GetMyIdeasAsync(userId, null);

        var aggregates = await _scoreService.GetAggregatesForIdeasAsync(allMine.Select(i => i.Id));
        foreach (var idea in allMine)
        {
            if (aggregates.TryGetValue(idea.Id, out var agg))
            {
                idea.AggregateScore = agg.OverallAverage;
                idea.ScorerCount = agg.ScorerCount;
            }
        }

        const string draft = nameof(IdeaStatus.Draft);

        var statusCounts = Enum.GetNames<IdeaStatus>()
            .ToDictionary(s => s, s => Enum.TryParse<IdeaStatus>(s, out var st)
                ? allMine.Count(i => i.Status == st.ToString())
                : 0);

        var draftIdeas = allMine
            .Where(i => i.Status == draft)
            .OrderByDescending(i => i.LastModifiedDate)
            .ToList();

        var pipeline = allMine
            .Where(i => i.Status != draft)
            .OrderByDescending(i => i.CreatedDate)
            .ToList();

        var vm = new IdeaListViewModel
        {
            Ideas = pipeline,
            DraftIdeas = draftIdeas,
            StatusFilter = null,
            AvailableStatuses = Enum.GetNames<IdeaStatus>().ToList(),
            StatusCounts = statusCounts
        };
        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (User.IsInRole("Admin"))
        {
            TempData["Error"] = "Admins review submissions only; submitting new ideas is for submitter accounts.";
            return RedirectToAction(nameof(AdminController.Index), "Admin");
        }

        return View(new CreateIdeaViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateIdeaViewModel vm)
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction(nameof(AdminController.Index), "Admin");
        if (!ModelState.IsValid) return View(vm);

        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error, ideaId) = await _ideaService.CreateIdeaAsync(userId, vm);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "The idea could not be created.");
            return View(vm);
        }

        TempData["Success"] = "Your idea was submitted successfully.";
        return RedirectToAction(nameof(Detail), new { id = ideaId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDraft(CreateIdeaViewModel vm)
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction(nameof(AdminController.Index), "Admin");

        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error, draftId) = await _ideaService.SaveDraftAsync(userId, vm);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "The draft could not be saved.");
            return View("Create", vm);
        }

        TempData["Success"] = "Draft saved.";
        return RedirectToAction(nameof(Edit), new { id = draftId });
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction(nameof(AdminController.Detail), "Admin", new { id });

        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var idea = await _ideaService.GetIdeaDetailAsync(id, userId, isAdmin: false);

        if (idea == null) return NotFound();

        ViewData["IdeasListFocus"] = idea.Status == "Draft" ? "drafts" : "";

        // Submitter sees only the overall aggregate (no breakdown, no scorer names — FR-009)
        var scoreSummary = await _scoreService.GetScoreSummaryAsync(id, isBlindReviewActive: false);

        return View(new IdeaDetailViewModel
        {
            Idea = idea,
            IsAdmin = false,
            IsDraft = idea.Status == "Draft",
            AggregateScore = scoreSummary.OverallAverage,
            ScorerCount = scoreSummary.ScorerCount
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMine(Guid id)
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction(nameof(AdminController.Index), "Admin");

        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error) = await _ideaService.DeleteMyIdeaAsync(id, userId);

        if (!success)
            TempData["Error"] = error ?? "This idea could not be deleted.";
        else
            TempData["Success"] = "Idea removed.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDraft(Guid id)
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction(nameof(AdminController.Index), "Admin");

        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error) = await _ideaService.DeleteDraftAsync(id, userId);

        if (!success)
            TempData["Error"] = error ?? "The draft could not be deleted.";
        else
            TempData["Success"] = "Draft deleted.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction(nameof(AdminController.Detail), "Admin", new { id });

        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var idea = await _ideaService.GetIdeaDetailAsync(id, userId, isAdmin: false);

        if (idea == null) return NotFound();

        var isDraft = idea.Status == "Draft";
        var isEditableSubmitted = idea.Status == "Submitted" && idea.CanAmendSubmitted;
        if (!isDraft && !isEditableSubmitted) return NotFound();

        ViewData["IdeasListFocus"] = isDraft ? "drafts" : "";

        var vm = BuildEditDraftViewModel(id, idea);
        vm.IsSubmittedEditable = isEditableSubmitted;

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDraft(Guid id, EditDraftViewModel vm)
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction(nameof(AdminController.Index), "Admin");

        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var head = await _ideaService.GetIdeaDetailAsync(id, userId, isAdmin: false);
        if (head == null || head.Status != "Draft") return NotFound();

        var (success, error) = await _ideaService.UpdateDraftAsync(id, userId, vm);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "The draft could not be updated.");
            vm.Id = id;
            vm.ExistingAttachment = head.Attachments.FirstOrDefault();
            vm.IsSubmittedEditable = false;
            return View("Edit", vm);
        }

        TempData["Success"] = "Draft saved.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSubmitted(Guid id, EditDraftViewModel vm)
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction(nameof(AdminController.Index), "Admin");

        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var idea = await _ideaService.GetIdeaDetailAsync(id, userId, isAdmin: false);
        if (idea == null || !idea.CanAmendSubmitted) return NotFound();

        vm.Id = id;
        if (!ModelState.IsValid)
        {
            vm.ExistingAttachment = idea.Attachments.FirstOrDefault();
            vm.IsSubmittedEditable = true;
            return View("Edit", vm);
        }

        var (success, error) = await _ideaService.UpdateSubmittedIdeaAsync(id, userId, vm);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "The update could not be saved.");
            vm = BuildEditDraftViewModel(id, idea);
            vm.IsSubmittedEditable = true;
            return View("Edit", vm);
        }

        TempData["Success"] = "Submission updated.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> WithdrawSubmitted(Guid id)
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction(nameof(AdminController.Index), "Admin");

        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var (success, error) = await _ideaService.WithdrawSubmittedIdeaAsync(id, userId);
        if (!success)
            TempData["Error"] = error ?? "Could not withdraw this idea.";
        else
            TempData["Success"] = "Idea withdrawn and removed from the queue.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitDraft(Guid id, EditDraftViewModel vm)
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction(nameof(AdminController.Index), "Admin");

        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var head = await _ideaService.GetIdeaDetailAsync(id, userId, isAdmin: false);
        if (head == null || head.Status != "Draft") return NotFound();

        vm.Id = id;
        if (!ModelState.IsValid)
        {
            vm.ExistingAttachment = head.Attachments.FirstOrDefault();
            vm.IsSubmittedEditable = false;
            return View("Edit", vm);
        }

        var (success, error) = await _ideaService.SubmitDraftAsync(id, userId, vm);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "The draft could not be submitted.");
            vm.ExistingAttachment = head.Attachments.FirstOrDefault();
            vm.IsSubmittedEditable = false;
            return View("Edit", vm);
        }

        TempData["Success"] = "Your idea was submitted successfully.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    public async Task<IActionResult> Download(Guid attachmentId, [FromQuery] bool preview = false)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _ideaService.DownloadAttachmentAsync(attachmentId, userId, isAdmin);

        if (result == null) return NotFound();

        if (preview && result.Value.ContentType != "application/octet-stream")
            return File(result.Value.Data, result.Value.ContentType);

        return File(result.Value.Data, result.Value.ContentType, result.Value.FileName);
    }

    private static EditDraftViewModel BuildEditDraftViewModel(Guid id, IdeaDetailDTO idea)
    {
        var vm = new EditDraftViewModel
        {
            Id = id,
            Category = idea.Category,
            Title = idea.Title,
            Description = idea.Description,
            ExistingAttachment = idea.Attachments.FirstOrDefault()
        };

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
                    case "TechArea": vm.TechArea = matchingEntry.Value; break;
                    case "TechEffort": vm.TechEffort = matchingEntry.Value; break;
                    case "TechBenefit": vm.TechBenefit = matchingEntry.Value; break;
                    case "ProcDepartment": vm.ProcDepartment = matchingEntry.Value; break;
                    case "ProcPainPoint": vm.ProcPainPoint = matchingEntry.Value; break;
                    case "ProcSavings": vm.ProcSavings = matchingEntry.Value; break;
                    case "ClientSegment": vm.ClientSegment = matchingEntry.Value; break;
                    case "ClientProblem": vm.ClientProblem = matchingEntry.Value; break;
                    case "ClientImpact": vm.ClientImpact = matchingEntry.Value; break;
                }
            }
        }

        return vm;
    }
}
