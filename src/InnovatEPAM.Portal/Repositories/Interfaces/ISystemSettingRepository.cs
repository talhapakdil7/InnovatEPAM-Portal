using InnovatEPAM.Portal.Models;

namespace InnovatEPAM.Portal.Repositories.Interfaces;

/// <summary>
/// Read/write access to the <c>SystemSettings</c> table.
/// Supports upsert semantics so callers never need to distinguish insert from update.
/// </summary>
public interface ISystemSettingRepository
{
    /// <summary>
    /// Returns the <see cref="SystemSetting"/> row for <paramref name="key"/>,
    /// or <c>null</c> when no matching row exists.
    /// </summary>
    Task<SystemSetting?> GetByKeyAsync(string key);

    /// <summary>
    /// Inserts or updates the <see cref="SystemSetting"/> row.
    /// If a row with the same <see cref="SystemSetting.Key"/> already exists, it is updated in-place.
    /// </summary>
    Task UpsertAsync(SystemSetting setting);
}
