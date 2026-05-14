using InnovatEPAM.Portal.Models;

namespace InnovatEPAM.Portal.Repositories.Interfaces;

/// <summary>
/// Data access for <see cref="IdeaScore"/> records.
/// Supports upsert, delete, and bulk-read patterns required by <see cref="Services.Interfaces.IScoreService"/>.
/// </summary>
public interface IIdeaScoreRepository
{
    /// <summary>
    /// Returns the score record submitted by <paramref name="adminId"/> for <paramref name="ideaId"/>,
    /// including the <see cref="IdeaScore.Admin"/> navigation property.
    /// Returns null when no matching record exists.
    /// </summary>
    Task<IdeaScore?> GetAsync(Guid ideaId, Guid adminId);

    /// <summary>
    /// Returns all score records for <paramref name="ideaId"/>,
    /// including the <see cref="IdeaScore.Admin"/> navigation property for each row.
    /// Returns an empty list when no admin has scored this idea.
    /// </summary>
    Task<List<IdeaScore>> GetAllForIdeaAsync(Guid ideaId);

    /// <summary>
    /// Returns all score records for the given collection of idea IDs in a single query.
    /// Used by list views to bulk-fetch aggregates without N+1 queries.
    /// </summary>
    Task<List<IdeaScore>> GetBulkForIdeasAsync(IEnumerable<Guid> ideaIds);

    /// <summary>
    /// Inserts or updates the admin's score record for the given idea.
    /// If a record for (IdeaId, AdminId) already exists, all dimension values and
    /// <see cref="IdeaScore.LastUpdatedDate"/> are updated in-place.
    /// </summary>
    Task UpsertAsync(IdeaScore score);

    /// <summary>
    /// Deletes the score record for (<paramref name="ideaId"/>, <paramref name="adminId"/>).
    /// No-op when no matching record exists.
    /// </summary>
    Task DeleteAsync(Guid ideaId, Guid adminId);
}
