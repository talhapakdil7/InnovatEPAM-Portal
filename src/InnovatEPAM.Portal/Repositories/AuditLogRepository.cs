using InnovatEPAM.Portal.Data;
using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InnovatEPAM.Portal.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _db;

    public AuditLogRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<AuditLog>> GetByIdeaAsync(Guid ideaId) =>
        await _db.AuditLogs
            .Include(a => a.ChangedByAdmin)
            .Where(a => a.IdeaId == ideaId)
            .OrderByDescending(a => a.ChangedDate)
            .ToListAsync();

    public async Task AddAsync(AuditLog log)
    {
        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
