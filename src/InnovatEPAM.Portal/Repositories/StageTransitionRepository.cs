using InnovatEPAM.Portal.Data;
using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InnovatEPAM.Portal.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IStageTransitionRepository"/>.
/// All writes use <see cref="ApplicationDbContext.SaveChangesAsync()"/> immediately
/// to keep transitions atomically persistent.
/// </summary>
public class StageTransitionRepository : IStageTransitionRepository
{
    private readonly ApplicationDbContext _db;

    public StageTransitionRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task AddAsync(StageTransition transition)
    {
        await _db.StageTransitions.AddAsync(transition);
        await _db.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<List<StageTransition>> GetByIdeaIdAsync(Guid ideaId)
    {
        return await _db.StageTransitions
            .Where(t => t.IdeaId == ideaId)
            .Include(t => t.TransitionedByAdmin)
            .OrderBy(t => t.TransitionDate)
            .ToListAsync();
    }
}
