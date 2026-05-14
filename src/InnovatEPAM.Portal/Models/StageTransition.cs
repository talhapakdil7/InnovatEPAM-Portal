namespace InnovatEPAM.Portal.Models;

/// <summary>
/// Records a single stage transition (advance or revert) in the multi-stage review workflow.
/// Rows are append-only: they cannot be deleted or edited after creation.
/// </summary>
public class StageTransition
{
    /// <summary>Unique identifier for this transition record.</summary>
    public Guid Id { get; set; }

    // ── Idea reference ──

    /// <summary>The idea this transition belongs to.</summary>
    public Guid IdeaId { get; set; }

    /// <summary>Navigation property to the parent idea.</summary>
    public Idea Idea { get; set; } = null!;

    // ── Transition details ──

    /// <summary>
    /// Stage the idea was in before this transition.
    /// Null when advancing from "no stage assigned" to the first stage.
    /// </summary>
    public ReviewStage? FromStage { get; set; }

    /// <summary>Stage the idea moved to as a result of this transition.</summary>
    public ReviewStage ToStage { get; set; }

    /// <summary>
    /// <c>true</c> for a forward advance; <c>false</c> for a backward revert.
    /// </summary>
    public bool IsAdvance { get; set; }

    /// <summary>
    /// Optional evaluation notes entered by the admin at the time of transition.
    /// Applies to both advances and reverts. Max 1000 characters.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Mandatory reason when <see cref="IsAdvance"/> is <c>false</c> (revert).
    /// Null for forward transitions. Max 500 characters.
    /// </summary>
    public string? RevertReason { get; set; }

    /// <summary>
    /// Final outcome recorded when <see cref="ToStage"/> is <see cref="ReviewStage.FinalDecision"/>
    /// and the admin confirms a decision. "Accepted" or "Rejected". Null for all other transitions.
    /// </summary>
    public string? Outcome { get; set; }

    // ── Audit ──

    /// <summary>The admin who performed this transition.</summary>
    public Guid TransitionedByAdminId { get; set; }

    /// <summary>Navigation property to the admin who performed this transition.</summary>
    public ApplicationUser TransitionedByAdmin { get; set; } = null!;

    /// <summary>UTC timestamp when the transition was recorded.</summary>
    public DateTime TransitionDate { get; set; } = DateTime.UtcNow;
}
