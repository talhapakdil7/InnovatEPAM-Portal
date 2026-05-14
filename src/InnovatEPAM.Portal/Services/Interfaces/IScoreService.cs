using InnovatEPAM.Portal.DTOs;
using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Services.Interfaces;

/// <summary>
/// Manages the full idea scoring lifecycle: submit, update, retract, and aggregate calculation.
/// Applies blind review masking to scorer names when blind review mode is active.
/// </summary>
public interface IScoreService
{
    /// <summary>
    /// Submits or updates the calling admin's score for the specified idea.
    /// Throws <see cref="InvalidOperationException"/> if the idea's status does not permit
    /// scoring (Draft, Accepted, or Rejected).
    /// </summary>
    /// <param name="ideaId">The idea to score.</param>
    /// <param name="adminId">The admin submitting the score.</param>
    /// <param name="vm">The score form values.</param>
    Task SubmitScoreAsync(Guid ideaId, Guid adminId, SubmitScoreViewModel vm);

    /// <summary>
    /// Retracts the calling admin's score for the specified idea.
    /// Silent no-op when the admin has not previously scored this idea.
    /// </summary>
    /// <param name="ideaId">The idea whose score is retracted.</param>
    /// <param name="adminId">The admin retracting their score.</param>
    Task RetractScoreAsync(Guid ideaId, Guid adminId);

    /// <summary>
    /// Returns the full score summary including per-dimension averages and the admin breakdown table.
    /// Scorer names are replaced with "Anonymous Reviewer" when <paramref name="isBlindReviewActive"/> is true.
    /// Returns a <see cref="ScoreSummaryDTO"/> with <c>ScorerCount = 0</c> and all null averages when no scores exist.
    /// </summary>
    Task<ScoreSummaryDTO> GetScoreSummaryAsync(Guid ideaId, bool isBlindReviewActive);

    /// <summary>
    /// Returns the calling admin's own score record for the given idea, or null if not yet scored.
    /// Used to pre-populate the scoring form on the admin detail page.
    /// </summary>
    Task<IdeaScore?> GetMyScoreAsync(Guid ideaId, Guid adminId);

    /// <summary>
    /// Returns a dictionary of overall aggregate scores keyed by IdeaId.
    /// Used to populate aggregate score columns in the admin list view efficiently (no N+1).
    /// Keys not present in the result have zero scorers.
    /// </summary>
    Task<Dictionary<Guid, (decimal? OverallAverage, int ScorerCount)>> GetAggregatesForIdeasAsync(IEnumerable<Guid> ideaIds);
}
