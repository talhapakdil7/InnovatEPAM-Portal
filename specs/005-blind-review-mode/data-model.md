# Data Model: Blind Review Mode

**Feature**: `specs/005-blind-review-mode`
**Date**: 2026-05-14

---

## 1. New Entity: SystemSetting

A generic key-value settings store. Blind review mode is stored as a single row with `Key = "BlindReviewEnabled"`.

```csharp
namespace InnovatEPAM.Portal.Models;

/// <summary>
/// Persists a named system-wide configuration value.
/// Each row is a distinct setting key. Blind review mode uses Key = "BlindReviewEnabled".
/// </summary>
public class SystemSetting
{
    /// <summary>Setting identifier (e.g., "BlindReviewEnabled").</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>String-encoded value (e.g., "true" / "false").</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>UTC timestamp of the last change.</summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Admin who last changed this setting. Null for seed data.</summary>
    public Guid? LastModifiedByAdminId { get; set; }

    /// <summary>Navigation property to the admin who last modified this setting.</summary>
    public ApplicationUser? LastModifiedByAdmin { get; set; }
}
```

### EF Core Configuration (in ApplicationDbContext)

```csharp
builder.Entity<SystemSetting>(entity =>
{
    entity.ToTable("SystemSettings");
    entity.HasKey(s => s.Key);
    entity.Property(s => s.Key).HasMaxLength(100).IsRequired();
    entity.Property(s => s.Value).HasMaxLength(500).IsRequired();

    entity.HasOne(s => s.LastModifiedByAdmin)
        .WithMany()
        .HasForeignKey(s => s.LastModifiedByAdminId)
        .IsRequired(false)
        .OnDelete(DeleteBehavior.SetNull);
});
```

### Seed Data

```csharp
// In SeedRoles / separate seeder: ensure the key exists with default false
builder.Entity<SystemSetting>().HasData(
    new SystemSetting { Key = "BlindReviewEnabled", Value = "false" }
);
```

### EF Migration

`dotnet ef migrations add AddSystemSettings`

---

## 2. New ViewModel: BlindReviewSettingsViewModel

```csharp
namespace InnovatEPAM.Portal.ViewModels;

/// <summary>
/// View model for the admin settings page for the blind review mode toggle.
/// </summary>
public class BlindReviewSettingsViewModel
{
    /// <summary>Current on/off state of blind review mode.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>UTC timestamp of when the setting was last changed. Null for seed defaults.</summary>
    public DateTime? LastModifiedDate { get; set; }

    /// <summary>Full name of the admin who last changed the setting. Null for seed defaults.</summary>
    public string? LastModifiedByAdminName { get; set; }
}
```

---

## 3. New Interface: IBlindReviewService

```csharp
namespace InnovatEPAM.Portal.Services.Interfaces;

/// <summary>
/// Manages the blind review mode setting and applies identity masking
/// to idea DTOs before they are rendered in admin views.
/// </summary>
public interface IBlindReviewService
{
    /// <summary>Returns true when blind review mode is currently enabled.</summary>
    Task<bool> IsEnabledAsync();

    /// <summary>
    /// Enables or disables blind review mode and persists the change.
    /// </summary>
    /// <param name="enabled">Target state.</param>
    /// <param name="adminId">The admin performing the change.</param>
    Task SetEnabledAsync(bool enabled, Guid adminId);

    /// <summary>
    /// Masks submitter identity fields in a detail DTO when blind review is active
    /// and the idea has not yet reached a concluded status (Accepted/Rejected).
    /// </summary>
    /// <param name="dto">The detail DTO to mask in-place.</param>
    /// <param name="isBlindReviewEnabled">Current global blind review flag.</param>
    void ApplyMasking(IdeaDetailDTO dto, bool isBlindReviewEnabled);

    /// <summary>
    /// Masks submitter identity fields across a list of list-item DTOs.
    /// </summary>
    /// <param name="dtos">DTOs to mask in-place.</param>
    /// <param name="isBlindReviewEnabled">Current global blind review flag.</param>
    void ApplyMasking(IEnumerable<IdeaListItemDTO> dtos, bool isBlindReviewEnabled);

    /// <summary>
    /// Returns true when the idea's status means evaluation is concluded
    /// and submitter identity should be revealed even in blind review mode.
    /// </summary>
    bool ShouldRevealIdentity(string ideaStatus);
}
```

---

## 4. Updated ViewModels: IsBlindReviewActive flag

The following existing ViewModels gain a new read-only display property so views can show a contextual banner.

**AdminIdeaListViewModel** (in `IdeaViewModels.cs`):
```csharp
/// <summary>True when blind review mode is globally active; drives the info banner.</summary>
public bool IsBlindReviewActive { get; set; }
```

**AdminIdeaDetailViewModel** (in `IdeaViewModels.cs`):
```csharp
/// <summary>True when blind review mode is globally active; drives the info banner.</summary>
public bool IsBlindReviewActive { get; set; }
```

---

## 5. New ISystemSettingRepository

```csharp
namespace InnovatEPAM.Portal.Repositories.Interfaces;

/// <summary>
/// Read/write access to the SystemSettings table.
/// </summary>
public interface ISystemSettingRepository
{
    /// <summary>Returns the setting value for <paramref name="key"/>, or null if not found.</summary>
    Task<SystemSetting?> GetByKeyAsync(string key);

    /// <summary>Persists a new or updated setting.</summary>
    Task UpsertAsync(SystemSetting setting);
}
```

---

## 6. Constants

```csharp
namespace InnovatEPAM.Portal.Models;

/// <summary>Well-known keys for the SystemSettings table.</summary>
public static class SystemSettingKeys
{
    public const string BlindReviewEnabled = "BlindReviewEnabled";
}
```

---

## 7. No Changes to Existing Entities

- `Idea`, `StageTransition`, `AuditLog`, `IdeaAttachment`, `ApplicationUser`, `IdeaDetailDTO`, `IdeaListItemDTO` — **unchanged** structurally.
- `SubmitterName` fields are masked in-place by the service; no new columns, no new navigation properties on existing entities.
