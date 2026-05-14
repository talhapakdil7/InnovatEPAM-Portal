namespace InnovatEPAM.Portal.ViewModels;

/// <summary>
/// View model for the admin settings page that controls the blind review mode toggle.
/// </summary>
public class BlindReviewSettingsViewModel
{
    /// <summary>Current on/off state of blind review mode.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>UTC timestamp of when the setting was last changed. Null for the initial seed row.</summary>
    public DateTime? LastModifiedDate { get; set; }

    /// <summary>Full name of the admin who last changed the setting. Null for the initial seed row.</summary>
    public string? LastModifiedByAdminName { get; set; }
}
