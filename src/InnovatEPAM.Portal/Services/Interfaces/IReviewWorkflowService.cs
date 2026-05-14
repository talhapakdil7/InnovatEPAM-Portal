using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Services.Interfaces;

/// <summary>
/// Encapsulates all business logic for the multi-stage innovation review pipeline.
/// Caller (AdminController) never touches repositories directly.
/// </summary>
public interface IReviewWorkflowService
{
    /// <summary>
    /// Advances the idea to the next review stage, recording the optional notes.
    /// </summary>
    /// <returns>
    /// A tuple of (Success, Error):
    /// <list type="bullet">
    ///   <item><description>Success = true, Error = null on success.</description></item>
    ///   <item><description>Success = false, Error = user-facing message on failure.</description></item>
    /// </list>
    /// </returns>
    Task<(bool Success, string? Error)> AdvanceStageAsync(Guid ideaId, Guid adminId, AdvanceStageViewModel vm);

    /// <summary>
    /// Reverts the idea to the previous review stage, recording the mandatory reason and optional notes.
    /// </summary>
    Task<(bool Success, string? Error)> RevertStageAsync(Guid ideaId, Guid adminId, RevertStageViewModel vm);

    /// <summary>
    /// Records the final outcome ("Accepted" or "Rejected") for an idea at the Final Decision stage
    /// and synchronises the idea's <see cref="Models.IdeaStatus"/> accordingly.
    /// </summary>
    Task<(bool Success, string? Error)> RecordDecisionAsync(Guid ideaId, Guid adminId, RecordDecisionViewModel vm);
}
