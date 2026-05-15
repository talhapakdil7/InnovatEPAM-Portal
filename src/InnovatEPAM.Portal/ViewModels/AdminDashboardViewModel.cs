using InnovatEPAM.Portal.DTOs;

namespace InnovatEPAM.Portal.ViewModels;

/// <summary>
/// Aggregated view model for the admin overview/dashboard page.
/// Provides a top-level summary of the innovation pipeline state.
/// </summary>
public class AdminDashboardViewModel
{
    // ── Pipeline totals ──────────────────────────────────────
    public int TotalIdeas { get; set; }
    public int SubmittedCount { get; set; }
    public int UnderReviewCount { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }

    /// <summary>Number of ideas per review stage order (1–4). Used to render pipeline health bars.</summary>
    public Dictionary<int, int> IdeasByStage { get; set; } = new();

    /// <summary>Operational breakdown by innovation category (or "Uncategorized").</summary>
    public List<CategorySliceVm> IdeasByCategory { get; set; } = new();

    // ── Action queues ────────────────────────────────────────

    /// <summary>Every non-draft idea for the dashboard master table.</summary>
    public List<IdeaListItemDTO> AllIdeas { get; set; } = new();

    /// <summary>All identity users for the dashboard directory.</summary>
    public List<DashboardUserRowVm> AllUsers { get; set; } = new();

    /// <summary>Latest status changes across the system (audit log feed).</summary>
    public List<RecentWorkflowActionVm> LatestWorkflowActions { get; set; } = new();

    // ── System flags ─────────────────────────────────────────
    public bool IsBlindReviewActive { get; set; }

    // ── Scoring analytics ────────────────────────────────────
    public DashboardScoringVm Scoring { get; set; } = new();

    // ── Computed helpers ─────────────────────────────────────
    public int PipelineThroughput => AcceptedCount + RejectedCount;
    public int ActiveWorkload => SubmittedCount + UnderReviewCount;
}

/// <summary>Scoring analytics for the admin dashboard — scoped to scorable (Submitted + UnderReview) ideas.</summary>
public class DashboardScoringVm
{
    /// <summary>Total count of ideas in Submitted or UnderReview status (eligible for scoring).</summary>
    public int ScorableIdeasCount { get; set; }

    /// <summary>Number of scorable ideas where the current admin has not yet submitted a score.</summary>
    public int NeedMyScoreCount { get; set; }

    /// <summary>Number of scorable ideas that have not been scored by any admin.</summary>
    public int NoReviewerScoresYetCount { get; set; }

    /// <summary>Overall average score across all scorable ideas that have at least one score.</summary>
    public decimal? PortfolioAverageScore { get; set; }

    /// <summary>Count of scorable ideas that the current admin has scored.</summary>
    public int MyScoresOnScorableCount { get; set; }

    /// <summary>Scorable ideas sorted by aggregate score descending (top 10 for the dashboard widget).</summary>
    public List<IdeaListItemDTO> RankedScorableIdeas { get; set; } = new();

    /// <summary>IDs of scorable ideas the current admin has already scored.</summary>
    public HashSet<Guid> MyScoredScorableIds { get; set; } = new();
}

/// <summary>Single row for category distribution on the admin overview.</summary>
public class CategorySliceVm
{
    public string DisplayName { get; set; } = "";
    public int Count { get; set; }
}

/// <summary>Compact row for the cross-idea workflow activity feed.</summary>
public class RecentWorkflowActionVm
{
    public Guid IdeaId { get; set; }
    public string IdeaTitle { get; set; } = "";
    public string Summary { get; set; } = "";
    public string ActorName { get; set; } = "";
    public DateTime WhenUtc { get; set; }
    public bool IsAdvance { get; set; }
}

/// <summary>Single user row on the admin dashboard directory.</summary>
public class DashboardUserRowVm
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
}

/// <summary>Lightweight model for the Activity page — workflow feed + recent ideas only.</summary>
public class AdminActivityViewModel
{
    public bool IsBlindReviewActive { get; set; }
    public List<RecentWorkflowActionVm> LatestWorkflowActions { get; set; } = new();
    public List<IdeaListItemDTO> RecentIdeas { get; set; } = new();
}
