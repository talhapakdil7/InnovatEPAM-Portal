using InnovatEPAM.Portal.Models;

namespace InnovatEPAM.Portal.Repositories.Interfaces;

/// <summary>
/// Append-only repository for <see cref="StageTransition"/> rows.
/// Reads are order-aware (ascending TransitionDate).
/// </summary>
public interface IStageTransitionRepository
{
    /// <summary>Adds a new transition record and persists it.</summary>
    Task AddAsync(StageTransition transition);

    /// <summary>Returns all transitions for <paramref name="ideaId"/>, oldest first.</summary>
    Task<List<StageTransition>> GetByIdeaIdAsync(Guid ideaId);
}
