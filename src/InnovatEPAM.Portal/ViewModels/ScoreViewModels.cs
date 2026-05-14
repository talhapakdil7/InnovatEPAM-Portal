using System.ComponentModel.DataAnnotations;

namespace InnovatEPAM.Portal.ViewModels;

/// <summary>
/// Form model for an admin submitting or updating their evaluation score for an idea.
/// All four dimension scores are optional (partial scoring is permitted per FR-003).
/// At least one dimension must be non-null — enforced by <c>SubmitScoreValidator</c>.
/// </summary>
public class SubmitScoreViewModel
{
    /// <summary>The idea being scored. Hidden field in the scoring form.</summary>
    public Guid IdeaId { get; set; }

    /// <summary>Innovation dimension score (1–5). Null when the admin chooses not to score this dimension.</summary>
    [Range(1, 5, ErrorMessage = "Innovation score must be between 1 and 5.")]
    public int? Innovation { get; set; }

    /// <summary>Technical Feasibility dimension score (1–5).</summary>
    [Range(1, 5, ErrorMessage = "Technical Feasibility score must be between 1 and 5.")]
    public int? TechnicalFeasibility { get; set; }

    /// <summary>Business Impact dimension score (1–5).</summary>
    [Range(1, 5, ErrorMessage = "Business Impact score must be between 1 and 5.")]
    public int? BusinessImpact { get; set; }

    /// <summary>Implementation Value dimension score (1–5).</summary>
    [Range(1, 5, ErrorMessage = "Implementation Value score must be between 1 and 5.")]
    public int? ImplementationValue { get; set; }
}
