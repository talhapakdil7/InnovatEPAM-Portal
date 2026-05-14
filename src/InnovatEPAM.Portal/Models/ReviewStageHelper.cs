namespace InnovatEPAM.Portal.Models;

/// <summary>
/// Utility methods for navigating and displaying <see cref="ReviewStage"/> values.
/// All members are stateless and thread-safe.
/// </summary>
public static class ReviewStageHelper
{
    /// <summary>All review stages in sequential order (ascending integer value).</summary>
    public static readonly IReadOnlyList<ReviewStage> Stages =
        Enum.GetValues<ReviewStage>().OrderBy(s => (int)s).ToList();

    /// <summary>
    /// Returns the stage that follows <paramref name="current"/> in the pipeline,
    /// or <c>null</c> when <paramref name="current"/> is the final stage.
    /// </summary>
    public static ReviewStage? NextStage(ReviewStage current)
    {
        var next = (int)current + 1;
        return Enum.IsDefined(typeof(ReviewStage), next)
            ? (ReviewStage)next
            : (ReviewStage?)null;
    }

    /// <summary>Returns <c>true</c> when <paramref name="stage"/> is the first stage in the pipeline.</summary>
    public static bool IsFirstStage(ReviewStage stage) => stage == ReviewStage.InitialScreening;

    /// <summary>Returns <c>true</c> when <paramref name="stage"/> is the last stage in the pipeline.</summary>
    public static bool IsLastStage(ReviewStage stage) => stage == ReviewStage.FinalDecision;

    /// <summary>Returns the human-readable display name for a review stage.</summary>
    public static string DisplayName(ReviewStage stage) => stage switch
    {
        ReviewStage.InitialScreening         => "Initial Screening",
        ReviewStage.TechnicalReview          => "Technical Review",
        ReviewStage.BusinessImpactAssessment => "Business Impact Assessment",
        ReviewStage.FinalDecision            => "Final Decision",
        _                                    => stage.ToString()
    };

    /// <summary>
    /// Returns the human-readable display name for a nullable review stage.
    /// Returns "Pending Review" when <paramref name="stage"/> is <c>null</c>.
    /// </summary>
    public static string DisplayName(ReviewStage? stage) =>
        stage.HasValue ? DisplayName(stage.Value) : "Pending Review";
}
