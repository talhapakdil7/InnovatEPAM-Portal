namespace InnovatEPAM.Portal.Models;

/// <summary>
/// Records one admin's evaluation scores for a single idea across the four fixed dimensions.
/// Composite primary key: (IdeaId, AdminId) — one record per admin per idea.
/// Partial scoring is supported: any dimension may be null when the admin skips it.
/// </summary>
public class IdeaScore
{
    /// <summary>The idea being scored. Part of the composite primary key.</summary>
    public Guid IdeaId { get; set; }

    /// <summary>Navigation property to the scored idea.</summary>
    public Idea Idea { get; set; } = null!;

    /// <summary>The admin who submitted this score. Part of the composite primary key.</summary>
    public Guid AdminId { get; set; }

    /// <summary>Navigation property to the admin who scored.</summary>
    public ApplicationUser Admin { get; set; } = null!;

    /// <summary>Score for the Innovation dimension (1–5). Null when this dimension was not scored.</summary>
    public int? Innovation { get; set; }

    /// <summary>Score for the Technical Feasibility dimension (1–5). Null when this dimension was not scored.</summary>
    public int? TechnicalFeasibility { get; set; }

    /// <summary>Score for the Business Impact dimension (1–5). Null when this dimension was not scored.</summary>
    public int? BusinessImpact { get; set; }

    /// <summary>Score for the Implementation Value dimension (1–5). Null when this dimension was not scored.</summary>
    public int? ImplementationValue { get; set; }

    /// <summary>UTC timestamp when this score record was first submitted.</summary>
    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the most recent update to any dimension on this record.</summary>
    public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
}
