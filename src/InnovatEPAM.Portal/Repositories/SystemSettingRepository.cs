using InnovatEPAM.Portal.Data;
using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InnovatEPAM.Portal.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ISystemSettingRepository"/>.
/// </summary>
public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly ApplicationDbContext _db;

    public SystemSettingRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<SystemSetting?> GetByKeyAsync(string key)
    {
        return await _db.SystemSettings
            .Include(s => s.LastModifiedByAdmin)
            .FirstOrDefaultAsync(s => s.Key == key);
    }

    /// <inheritdoc/>
    public async Task UpsertAsync(SystemSetting setting)
    {
        var existing = await _db.SystemSettings.FindAsync(setting.Key);
        if (existing == null)
        {
            await _db.SystemSettings.AddAsync(setting);
        }
        else
        {
            existing.Value = setting.Value;
            existing.LastModifiedDate = setting.LastModifiedDate;
            existing.LastModifiedByAdminId = setting.LastModifiedByAdminId;
            _db.SystemSettings.Update(existing);
        }
        await _db.SaveChangesAsync();
    }
}
