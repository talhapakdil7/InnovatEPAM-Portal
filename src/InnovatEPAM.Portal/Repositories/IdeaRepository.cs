using InnovatEPAM.Portal.Data;
using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
            .Include(i => i.Scores)
            .Include(i => i.AuditLogs).ThenInclude(a => a.ChangedByAdmin)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<List<Idea>> GetBySubmitterAsync(Guid submitterId) =>
        await _db.Ideas
            .Include(i => i.Submitter)
            .Include(i => i.IdeaAttachments)
            .Include(i => i.Scores)
            .Where(i => i.SubmitterId == submitterId)
            .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();

    public async Task<List<Idea>> GetAllAsync() =>
        await _db.Ideas
            .Include(i => i.Submitter)
            .Include(i => i.UpdatedByAdmin)
            .Include(i => i.IdeaAttachments)
            .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();

    public async Task<List<Idea>> GetByStatusAsync(IdeaStatus status) =>
        await _db.Ideas
            .Include(i => i.Submitter)
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();

    private IQueryable<Idea> AdminListFilteredCore(string? statusFilter, string? categoryFilter)
    {
        var q = _db.Ideas
            .AsNoTracking()
            .Where(i => i.Status != IdeaStatus.Draft);

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<IdeaStatus>(statusFilter, out var status))
            q = q.Where(i => i.Status == status);

        if (!string.IsNullOrWhiteSpace(categoryFilter) && CategoryDefinitions.All.ContainsKey(categoryFilter))
            q = q.Where(i => i.Category == categoryFilter);

        return q;
    }

    public async Task<(List<Idea> Items, int TotalCount)> GetAdminIdeasFilteredPagedAsync(
        string? statusFilter, string? categoryFilter, int skip, int take)
    {
        var core = AdminListFilteredCore(statusFilter, categoryFilter);
        var total = await core.CountAsync();
        var items = await core
            .Include(i => i.Submitter)
            .Include(i => i.UpdatedByAdmin)
            .Include(i => i.IdeaAttachments)
            .OrderByDescending(i => i.CreatedDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return (items, total);
    }

    public async Task<Dictionary<IdeaStatus, int>> CountNonDraftByStatusAsync()
    {
        var rows = await _db.Ideas.AsNoTracking()
            .Where(i => i.Status != IdeaStatus.Draft)
            .GroupBy(i => i.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();

        return rows.ToDictionary(x => x.Key, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetAdminStatusCountsAsync(
        string? statusFilter, string? categoryFilter)
    {
        var q = AdminListFilteredCore(statusFilter, categoryFilter);
        var rows = await q
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return rows.ToDictionary(x => x.Status.ToString(), x => x.Count);
    }

    public async Task<Dictionary<string, int>> CountByCategoryNonDraftAsync()
    {
        var rows = await _db.Ideas.AsNoTracking()
            .Where(i => i.Status != IdeaStatus.Draft)
            .GroupBy(i => i.Category)
            .Select(g => new { Cat = g.Key, Count = g.Count() })
            .ToListAsync();

        return rows.ToDictionary(
            x => x.Cat ?? string.Empty,
            x => x.Count);
    }

    public async Task<List<Idea>> GetRecentNonDraftIdeasAsync(int take) =>
        await _db.Ideas
            .AsNoTracking()
            .Include(i => i.Submitter)
            .Include(i => i.UpdatedByAdmin)
            .Include(i => i.IdeaAttachments)
            .Where(i => i.Status != IdeaStatus.Draft)
            .OrderByDescending(i => i.LastModifiedDate)
            .Take(take)
            .ToListAsync();

    public async Task<List<Idea>> GetScorableIdeasWithScoresAsync() =>
        await _db.Ideas
            .AsNoTracking()
            .Include(i => i.Submitter)
            .Include(i => i.IdeaAttachments)
            .Include(i => i.Scores)
            .Where(i => i.Status == IdeaStatus.Submitted || i.Status == IdeaStatus.UnderReview)
            .ToListAsync();

    public async Task<List<Guid>> GetNonDraftIdeaIdsAsync() =>
        await _db.Ideas.AsNoTracking()
            .Where(i => i.Status != IdeaStatus.Draft)
            .Select(i => i.Id)
            .ToListAsync();

    public async Task<int> CountUnderReviewWithNoScoresAsync() =>
        await _db.Ideas.AsNoTracking()
            .CountAsync(i => i.Status == IdeaStatus.UnderReview && !i.Scores.Any());

    public async Task AddAsync(Idea idea)
    {
        _db.Ideas.Add(idea);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Idea idea)
    {
        try
        {
            // Callers typically load via GetByIdAsync, so the entity is already tracked.
            // Only call Update() if the entity is detached to avoid issues with change tracking.
            if (_db.Entry(idea).State == EntityState.Detached)
                _db.Ideas.Update(idea);

            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another process modified or deleted the idea/attachments between load and save.
            // Reload the current database state and rethrow with helpful context.
            var entry = _db.Entry(idea);
            entry.Reload();
            
            // After reload, check if the idea still exists
            var currentValues = await _db.Ideas.AsNoTracking().FirstOrDefaultAsync(i => i.Id == idea.Id);
            if (currentValues == null)
            {
                throw new InvalidOperationException(
                    $"The idea (ID: {idea.Id}) was deleted by another user or process. " +
                    "Please refresh the page and try again.");
            }

            throw new InvalidOperationException(
                $"The idea (ID: {idea.Id}) was modified by another user or process. " +
                "Please refresh the page to see the latest changes and try again.");
        }
    }
}
