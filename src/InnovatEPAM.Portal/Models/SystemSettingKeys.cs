namespace InnovatEPAM.Portal.Models;

/// <summary>
/// Well-known key constants for the <see cref="SystemSetting"/> table.
/// Use these constants instead of raw strings to prevent typos.
/// </summary>
public static class SystemSettingKeys
{
    /// <summary>
    /// When the stored value is <c>"true"</c> (case-insensitive), administrators
    /// cannot see submitter identity in idea review views.
    /// </summary>
    public const string BlindReviewEnabled = "BlindReviewEnabled";
}
