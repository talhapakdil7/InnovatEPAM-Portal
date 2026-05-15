using InnovatEPAM.Portal.Data;
using InnovatEPAM.Portal.DTOs;
using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Services.Interfaces;
using InnovatEPAM.Portal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InnovatEPAM.Portal.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IIdeaService _ideaService;
    private readonly IBlindReviewService _blindReviewService;
    private readonly IScoreService _scoreService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public AdminController(
        IIdeaService ideaService,
        IBlindReviewService blindReviewService,
        IScoreService scoreService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        _ideaService = ideaService;
        _blindReviewService = blindReviewService;
        _scoreService = scoreService;
        _userManager = userManager;
        _db = db;
    }

    /// <summary>GET — admin overview dashboard showing pipeline health and action queues.</summary>
    public async Task<IActionResult> Dashboard()
    {
        var adminId = Guid.Parse(_userManager.GetUserId(User)!);
        return View(await BuildAdminDashboardViewModelAsync(adminId));
    }

    /// <summary>Legacy route — merged into <see cref="Dashboard"/>. Preserves bookmarks.</summary>
    public IActionResult Analytics() =>
        LocalRedirect(Url.Action(nameof(Dashboard), "Admin")! + "#analytics");

    /// <summary>Activity feed — workflow events and recent submissions.</summary>
    public async Task<IActionResult> Activity() =>
        View(await BuildAdminActivityViewModelAsync());

    private async Task<AdminActivityViewModel> BuildAdminActivityViewModelAsync()
    {
        var allIdeas = await _ideaService.GetAllIdeasAsync(null, null, null);
        var isBlindReview = await _blindReviewService.IsEnabledAsync();
        _blindReviewService.ApplyMasking(allIdeas, isBlindReview);

        var recentAuditActions = await _db.AuditLogs
            .Include(a => a.Idea)
            .Include(a => a.ChangedByAdmin)
            .OrderByDescending(a => a.ChangedDate)
            .Take(15)
            .AsNoTracking()
            .ToListAsync();

        return new AdminActivityViewModel
        {
            IsBlindReviewActive = isBlindReview,
            LatestWorkflowActions = recentAuditActions.Select(a => new RecentWorkflowActionVm
            {
                IdeaId = a.IdeaId,
                IdeaTitle = a.Idea?.Title ?? "",
                Summary = $"Status: {a.OldStatus?.Replace("UnderReview","Under Review")} → {a.NewStatus?.Replace("UnderReview","Under Review")}",
                ActorName = a.ChangedByAdmin?.FullName ?? "",
                WhenUtc = a.ChangedDate,
                IsAdvance = true
            }).ToList(),
            RecentIdeas = allIdeas
                .OrderByDescending(i => i.CreatedDate)
                .Take(8)
                .ToList()
        };
    }

    /// <summary>Removed from navigation — board duplicated filters already on <see cref="Index"/>.</summary>
    public IActionResult Kanban() =>
        LocalRedirect(Url.Action(nameof(Index), "Admin")!);

    /// <summary>Historical list of decided ideas (accepted / rejected).</summary>
    public async Task<IActionResult> DecisionHistory()
    {
        var ideas = await _ideaService.GetAllIdeasAsync(null, null);
        var isBlindReview = await _blindReviewService.IsEnabledAsync();
        _blindReviewService.ApplyMasking(ideas, isBlindReview);

        var decided = ideas
            .Where(i => i.Status is "Accepted" or "Rejected")
            .OrderByDescending(i => i.LastModifiedDate)
            .ToList();

        return View(new DecisionHistoryViewModel
        {
            Items = decided,
            IsBlindReviewActive = isBlindReview
        });
    }

    private async Task<AdminDashboardViewModel> BuildAdminDashboardViewModelAsync(Guid adminId)
    {
        var allIdeas = await _ideaService.GetAllIdeasAsync(null, null);
        var isBlindReview = await _blindReviewService.IsEnabledAsync();
        _blindReviewService.ApplyMasking(allIdeas, isBlindReview);

        var aggregates = await _scoreService.GetAggregatesForIdeasAsync(allIdeas.Select(i => i.Id));
        foreach (var idea in allIdeas)
        {
            if (aggregates.TryGetValue(idea.Id, out var agg))
            {
                idea.AggregateScore = agg.OverallAverage;
                idea.ScorerCount = agg.ScorerCount;
            }
        }

        var recentAuditActions = await _db.AuditLogs
            .Include(a => a.Idea)
            .Include(a => a.ChangedByAdmin)
            .OrderByDescending(a => a.ChangedDate)
            .Take(15)
            .AsNoTracking()
            .ToListAsync();

        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        var dashboardUsers = users.Select(u => new DashboardUserRowVm
        {
            FullName = u.FullName,
            Email = u.Email ?? "",
        }).ToList();

        var scorableIdeas = allIdeas
            .Where(i => i.Status == "Submitted" || i.Status == "UnderReview")
            .ToList();
        var scorableIds = scorableIdeas.Select(i => i.Id).ToList();
        var myScoredIds = await _scoreService.GetIdeaIdsScoredByAdminAsync(adminId, scorableIds);

        var scoredScorableIdeas = scorableIdeas.Where(i => i.AggregateScore.HasValue).ToList();
        decimal? portfolioAvg = scoredScorableIdeas.Count > 0
            ? Math.Round(scoredScorableIdeas.Average(i => i.AggregateScore!.Value), 2)
            : null;

        var rankedScorableIdeas = scorableIdeas
            .OrderByDescending(i => i.AggregateScore ?? 0)
            .ThenByDescending(i => i.ScorerCount)
            .Take(10)
            .ToList();

        var scoring = new DashboardScoringVm
        {
            ScorableIdeasCount = scorableIdeas.Count,
            NeedMyScoreCount = scorableIdeas.Count(i => !myScoredIds.Contains(i.Id)),
            NoReviewerScoresYetCount = scorableIdeas.Count(i => i.ScorerCount == 0),
            PortfolioAverageScore = portfolioAvg,
            MyScoresOnScorableCount = myScoredIds.Count,
            RankedScorableIdeas = rankedScorableIdeas,
            MyScoredScorableIds = myScoredIds
        };

        return new AdminDashboardViewModel
        {
            TotalIdeas = allIdeas.Count,
            SubmittedCount = allIdeas.Count(i => i.Status == "Submitted"),
            UnderReviewCount = allIdeas.Count(i => i.Status == "UnderReview"),
            AcceptedCount = allIdeas.Count(i => i.Status == "Accepted"),
            RejectedCount = allIdeas.Count(i => i.Status == "Rejected"),
            IdeasByCategory = allIdeas
                .GroupBy(i => i.CategoryDisplayName ?? "Uncategorized")
                .Select(g => new CategorySliceVm { DisplayName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),
            AllIdeas = allIdeas
                .OrderByDescending(i => i.LastModifiedDate)
                .ToList(),
            AllUsers = dashboardUsers,
            LatestWorkflowActions = recentAuditActions.Select(a => new RecentWorkflowActionVm
            {
                IdeaId = a.IdeaId,
                IdeaTitle = a.Idea?.Title ?? "",
                Summary = $"Status: {a.OldStatus?.Replace("UnderReview","Under Review")} → {a.NewStatus?.Replace("UnderReview","Under Review")}",
                ActorName = a.ChangedByAdmin?.FullName ?? "",
                WhenUtc = a.ChangedDate,
                IsAdvance = true
            }).ToList(),
            IsBlindReviewActive = isBlindReview,
            Scoring = scoring
        };
    }

    public async Task<IActionResult> Index(string? statusFilter, string? categoryFilter, string? q)
    {
        var ideas = await _ideaService.GetAllIdeasAsync(statusFilter, categoryFilter, q);
        var isBlindReview = await _blindReviewService.IsEnabledAsync();
        _blindReviewService.ApplyMasking(ideas, isBlindReview);

        var aggregates = await _scoreService.GetAggregatesForIdeasAsync(ideas.Select(i => i.Id));
        foreach (var idea in ideas)
        {
            if (aggregates.TryGetValue(idea.Id, out var agg))
            {
                idea.AggregateScore = agg.OverallAverage;
                idea.ScorerCount = agg.ScorerCount;
            }
        }

        var statusSummary = ideas
            .GroupBy(i => i.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var vm = new AdminIdeaListViewModel
        {
            Ideas = ideas,
            SearchQuery = q,
            StatusFilter = statusFilter,
            CategoryFilter = categoryFilter,
            AvailableStatuses = Enum.GetNames<IdeaStatus>().Where(s => s != "Draft").ToList(),
            AvailableCategories = CategoryDefinitions.All.ToDictionary(kv => kv.Key, kv => kv.Value.DisplayName),
            StatusSummary = statusSummary,
            IsBlindReviewActive = isBlindReview
        };
        return View(vm);
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        var adminId = Guid.Parse(_userManager.GetUserId(User)!);
        var idea = await _ideaService.GetIdeaDetailAsync(id, adminId, isAdmin: true);
        if (idea == null) return NotFound();

        var isBlindReview = await _blindReviewService.IsEnabledAsync();
        _blindReviewService.ApplyMasking(idea, isBlindReview);

        var scoreSummary = await _scoreService.GetScoreSummaryAsync(id, isBlindReview);
        var myScore = await _scoreService.GetMyScoreAsync(id, adminId);
        var isScoringAllowed = idea.Status is "Submitted" or "UnderReview";

        var terminal = idea.Status is "Accepted" or "Rejected";
        var lifecycleStatuses = new List<string>
        {
            nameof(IdeaStatus.Submitted),
            nameof(IdeaStatus.UnderReview),
            nameof(IdeaStatus.Accepted),
            nameof(IdeaStatus.Rejected)
        };

        return View(new AdminIdeaDetailViewModel
        {
            Idea = idea,
            AllowedStatuses = lifecycleStatuses,
            CanManualLifecycleEdit = !terminal,
            IsBlindReviewActive = isBlindReview,
            ScoreSummary = scoreSummary.ScorerCount > 0 ? scoreSummary : null,
            ScoreForm = new SubmitScoreViewModel
            {
                IdeaId = id,
                Innovation = myScore?.Innovation,
                TechnicalFeasibility = myScore?.TechnicalFeasibility,
                BusinessImpact = myScore?.BusinessImpact,
                ImplementationValue = myScore?.ImplementationValue
            },
            IsScoringAllowed = isScoringAllowed
        });
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
