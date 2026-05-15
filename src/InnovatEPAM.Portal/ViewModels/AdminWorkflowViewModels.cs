using InnovatEPAM.Portal.DTOs;

namespace InnovatEPAM.Portal.ViewModels;

/// <summary>Decided ideas only — accepted or rejected.</summary>
public class DecisionHistoryViewModel
{
    public List<IdeaListItemDTO> Items { get; set; } = new();
    public bool IsBlindReviewActive { get; set; }
}

/// <summary>Admin topbar bell: triage / pipeline counts.</summary>
public class AdminWorkqueueSummaryVm
{
    public int TriageCount { get; set; }
    public int InPipelineCount { get; set; }

    /// <summary>Alias used by the workqueue view component view.</summary>
    public int UnderReviewCount => InPipelineCount;
}
