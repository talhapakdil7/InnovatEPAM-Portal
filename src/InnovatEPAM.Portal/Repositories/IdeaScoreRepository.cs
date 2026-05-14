using InnovatEPAM.Portal.Data;
using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InnovatEPAM.Portal.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IIdeaScoreRepository"/>.
/// </summary>
public class IdeaScoreRepository : IIdeaScoreRepository
{
    private readonly ApplicationDbContext _db;

    public IdeaScoreRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<IdeaScore?> GetAsync(Guid ideaId, Guid adminId)
    {
        return await _db.IdeaScores
            .Include(s => s.Admin)
            .FirstOrDefaultAsync(s => s.IdeaId == ideaId && s.AdminId == adminId);
    }

    /// <inheritdoc/>
    public async Task<List<IdeaScore>> GetAllForIdeaAsync(Guid ideaId)
    {
        return await _db.IdeaScores
            .Include(s => s.Admin)
            .Where(s => s.IdeaId == ideaId)
            .OrderBy(s => s.SubmittedDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<IdeaScore>> GetBulkForIdeasAsync(IEnumerable<Guid> ideaIds)
    {
        var ids = ideaIds.ToList();
        return await _db.IdeaScores
            .Where(s => ids.Contains(s.IdeaId))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task UpsertAsync(IdeaScore score)
    {
        var existing = await _db.IdeaScores
            .FirstOrDefaultAsync(s => s.IdeaId == score.IdeaId && s.AdminId == score.AdminId);

        if (existing == null)
        {
            await _db.IdeaScores.AddAsync(score);
        }
        else
        {
            existing.Innovation = score.Innovation;
            existing.TechnicalFeasibility = score.TechnicalFeasibility;
            existing.BusinessImpact = score.BusinessImpact;
            existing.ImplementationValue = score.ImplementationValue;
            existing.LastUpdatedDate = DateTime.UtcNow;
            _db.IdeaScores.Update(existing);
        }

        await _db.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid ideaId, Guid adminId)
    {
        var existing = await _db.IdeaScores
            .FirstOrDefaultAsync(s => s.IdeaId == ideaId && s.AdminId == adminId);

        if (existing != null)
        {
            _db.IdeaScores.Remove(existing);
            await _db.SaveChangesAsync();
        }
    }
}
