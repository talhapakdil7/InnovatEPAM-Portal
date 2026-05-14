namespace InnovatEPAM.Portal.DTOs;

/// <summary>
/// Read-only snapshot of a single <see cref="Models.StageTransition"/> row,
/// used to render stage history in views without exposing entity internals.
/// </summary>
public class StageTransitionDTO
{
    public Guid Id { get; set; }
    public Guid IdeaId { get; set; }

    public string? FromStageName { get; set; }
    public string ToStageName { get; set; } = string.Empty;
    public bool IsAdvance { get; set; }

    public string? Notes { get; set; }
    public string? RevertReason { get; set; }
    public string? Outcome { get; set; }

    public string TransitionedByAdminName { get; set; } = string.Empty;
    public DateTime TransitionDate { get; set; }
}
