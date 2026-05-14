namespace InnovatEPAM.Portal.Models;

/// <summary>
/// Represents the four sequential stages in the multi-stage innovation idea review pipeline.
/// Integer values start at 1 so that a NULL database column unambiguously means "no stage assigned".
/// </summary>
public enum ReviewStage
{
    /// <summary>First stage: initial triage and eligibility check.</summary>
    InitialScreening = 1,

    /// <summary>Second stage: detailed technical feasibility review.</summary>
    TechnicalReview = 2,

    /// <summary>Third stage: evaluation of business ROI and strategic alignment.</summary>
    BusinessImpactAssessment = 3,

    /// <summary>Fourth stage: final approval or rejection decision.</summary>
    FinalDecision = 4
}
