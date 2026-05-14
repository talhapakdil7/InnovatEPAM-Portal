using InnovatEPAM.Portal.Models;

namespace InnovatEPAM.Portal.ViewModels;

/// <summary>
/// View model for the Admin action that advances an idea to the next review stage.
/// </summary>
public class AdvanceStageViewModel
{
    /// <summary>The idea being advanced.</summary>
    public Guid IdeaId { get; set; }

    /// <summary>
    /// Optional evaluation notes for this transition (max 1000 chars).
    /// </summary>
    public string? Notes { get; set; }

    // ── Read-only display fields (populated by controller GET) ──

    /// <summary>Idea title shown on the confirmation form.</summary>
    public string IdeaTitle { get; set; } = string.Empty;

    /// <summary>Human-readable name of the current stage before advancing.</summary>
    public string CurrentStageName { get; set; } = string.Empty;

    /// <summary>Human-readable name of the target (next) stage.</summary>
    public string NextStageName { get; set; } = string.Empty;
}

/// <summary>
/// View model for the Admin action that reverts an idea to the previous review stage.
/// </summary>
public class RevertStageViewModel
{
    /// <summary>The idea being reverted.</summary>
    public Guid IdeaId { get; set; }

    /// <summary>
    /// Mandatory reason explaining why the stage is being reverted (max 500 chars).
    /// </summary>
    public string RevertReason { get; set; } = string.Empty;

    /// <summary>Optional additional notes for this revert transition (max 1000 chars).</summary>
    public string? Notes { get; set; }

    // ── Read-only display fields (populated by controller GET) ──

    /// <summary>Idea title shown on the confirmation form.</summary>
    public string IdeaTitle { get; set; } = string.Empty;

    /// <summary>Human-readable name of the current stage before reverting.</summary>
    public string CurrentStageName { get; set; } = string.Empty;

    /// <summary>Human-readable name of the previous stage the idea will revert to.</summary>
    public string PreviousStageName { get; set; } = string.Empty;
}

/// <summary>
/// View model for the Admin action that records the final decision (Accepted / Rejected)
/// when the idea is at the <see cref="ReviewStage.FinalDecision"/> stage.
/// </summary>
public class RecordDecisionViewModel
{
    /// <summary>The idea receiving the final decision.</summary>
    public Guid IdeaId { get; set; }

    /// <summary>
    /// Outcome value: must be either "Accepted" or "Rejected".
    /// </summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>Optional notes accompanying the final decision (max 1000 chars).</summary>
    public string? Notes { get; set; }

    // ── Read-only display fields ──

    /// <summary>Idea title shown on the decision form.</summary>
    public string IdeaTitle { get; set; } = string.Empty;
}
