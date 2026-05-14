using InnovatEPAM.Portal.Models;

namespace InnovatEPAM.Portal.Repositories.Interfaces;

public interface IAuditLogRepository
{
    Task<List<AuditLog>> GetByIdeaAsync(Guid ideaId);
    Task AddAsync(AuditLog log);
}
