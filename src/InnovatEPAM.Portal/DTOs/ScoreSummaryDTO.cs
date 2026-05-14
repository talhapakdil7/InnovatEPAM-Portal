namespace InnovatEPAM.Portal.DTOs;

/// <summary>
/// Computed aggregate of all IdeaScore records for one idea.
/// Never persisted — recalculated on every request from raw IdeaScore rows.
/// </summary>
public class ScoreSummaryDTO
{
    /// <summary>Number of admins who have submitted at least one scored dimension.</summary>
    public int ScorerCount { get; set; }

    /// <summary>Average of all non-null Innovation scores across admins. Null when no admin has scored this dimension.</summary>
    public decimal? AvgInnovation { get; set; }

    /// <summary>Average of all non-null Technical Feasibility scores across admins.</summary>
    public decimal? AvgTechnicalFeasibility { get; set; }

    /// <summary>Average of all non-null Business Impact scores across admins.</summary>
    public decimal? AvgBusinessImpact { get; set; }

    /// <summary>Average of all non-null Implementation Value scores across admins.</summary>
    public decimal? AvgImplementationValue { get; set; }

    /// <summary>
    /// Overall average calculated as the mean of all non-null dimension averages.
    /// Null when no dimension has been scored by any admin.
    /// Rounded to 2 decimal places.
    /// </summary>
    public decimal? OverallAverage { get; set; }

    /// <summary>
    /// Individual admin score rows for the breakdown table shown in admin detail view.
    /// Scorer names are masked to "Anonymous Reviewer" when blind review mode is active.
    /// </summary>
    public List<AdminScoreRowDTO> AdminScores { get; set; } = new();
}

/// <summary>
/// One admin's per-dimension score row as displayed in the admin idea detail scoring breakdown table.
/// </summary>
public class AdminScoreRowDTO
{
    /// <summary>
    /// Admin's full name. Set to "Anonymous Reviewer" when blind review mode is active.
    /// </summary>
    public string AdminName { get; set; } = string.Empty;

    /// <summary>This admin's Innovation score (1–5, null = not scored).</summary>
    public int? Innovation { get; set; }

    /// <summary>This admin's Technical Feasibility score (1–5, null = not scored).</summary>
    public int? TechnicalFeasibility { get; set; }

    /// <summary>This admin's Business Impact score (1–5, null = not scored).</summary>
    public int? BusinessImpact { get; set; }

    /// <summary>This admin's Implementation Value score (1–5, null = not scored).</summary>
    public int? ImplementationValue { get; set; }

    /// <summary>Mean of this admin's non-null dimension scores. Null when all dimensions skipped.</summary>
    public decimal? RowAverage { get; set; }

    /// <summary>UTC timestamp when this admin last updated their score.</summary>
    public DateTime SubmittedDate { get; set; }
}
