namespace InnovatEPAM.Portal.Models;

/// <summary>
/// Persists a named system-wide configuration value as a key-value pair.
/// Each row represents one distinct setting. Blind review mode uses
/// <see cref="SystemSettingKeys.BlindReviewEnabled"/> as the key.
/// </summary>
public class SystemSetting
{
    /// <summary>Setting identifier (e.g., "BlindReviewEnabled"). Acts as the primary key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>String-encoded value (e.g., "true" / "false").</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>UTC timestamp of the last change to this setting.</summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The admin user who last changed this setting.
    /// Null for seed/default rows that have never been manually updated.
    /// </summary>
    public Guid? LastModifiedByAdminId { get; set; }

    /// <summary>Navigation property to the admin who last modified this setting.</summary>
    public ApplicationUser? LastModifiedByAdmin { get; set; }
}
