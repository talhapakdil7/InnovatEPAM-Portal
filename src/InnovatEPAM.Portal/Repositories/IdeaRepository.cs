using InnovatEPAM.Portal.Data;
using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InnovatEPAM.Portal.Repositories;

public class IdeaRepository : IIdeaRepository
{
    private readonly ApplicationDbContext _db;

    public IdeaRepository(ApplicationDbContext db) => _db = db;

    public async Task<Idea?> GetByIdAsync(Guid id) =>
        await _db.Ideas
            .Include(i => i.Submitter)
            .Include(i => i.UpdatedByAdmin)
            .Include(i => i.IdeaAttachments)
            .Include(i => i.AuditLogs).ThenInclude(a => a.ChangedByAdmin)
            .Include(i => i.StageTransitions).ThenInclude(t => t.TransitionedByAdmin)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<List<Idea>> GetBySubmitterAsync(Guid submitterId) =>
        await _db.Ideas
            .Include(i => i.Submitter)
            .Where(i => i.SubmitterId == submitterId)
            .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();

    public async Task<List<Idea>> GetAllAsync() =>
        await _db.Ideas
            .Include(i => i.Submitter)
            .Include(i => i.UpdatedByAdmin)
            .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();

    public async Task<List<Idea>> GetByStatusAsync(IdeaStatus status) =>
        await _db.Ideas
            .Include(i => i.Submitter)
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();

    public async Task AddAsync(Idea idea)
    {
        _db.Ideas.Add(idea);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Idea idea)
    {
        _db.Ideas.Update(idea);
        await _db.SaveChangesAsync();
    }
}
