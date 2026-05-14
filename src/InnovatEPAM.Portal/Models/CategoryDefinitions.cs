namespace InnovatEPAM.Portal.Models;

/// <summary>
/// Defines a single field within an innovation category form section.
/// </summary>
public class CategoryFieldDefinition
{
    /// <summary>ViewModel property name used for model binding (e.g. "TechArea").</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Human-readable label shown in the UI (e.g. "Technology Area").</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>HTML input type: "select", "text", or "textarea".</summary>
    public string InputType { get; init; } = "text";

    /// <summary>Selectable options for "select" input type. Empty for text/textarea.</summary>
    public IReadOnlyList<string> Options { get; init; } = Array.Empty<string>();

    /// <summary>Whether the field must be filled before form submission.</summary>
    public bool IsRequired { get; init; }

    /// <summary>Maximum character length enforced by validator and HTML maxlength attribute. 0 = no limit.</summary>
    public int MaxLength { get; init; }

    /// <summary>Contextual hint text displayed below the field to guide the submitter.</summary>
    public string GuidanceHint { get; init; } = string.Empty;
}

/// <summary>
/// Defines an innovation category, its display name, and the ordered list of category-specific fields.
/// </summary>
public class CategoryDefinition
{
    /// <summary>Stable string key stored in the database (e.g. "TechnicalImprovement").</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Human-readable name shown in the UI (e.g. "Technical Improvement").</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Ordered list of fields shown when this category is selected.</summary>
    public IReadOnlyList<CategoryFieldDefinition> Fields { get; init; } = Array.Empty<CategoryFieldDefinition>();
}

/// <summary>
/// Static registry of all supported innovation categories and their field definitions.
/// This is the single source of truth for category metadata used by validators, services, and views.
/// </summary>
public static class CategoryDefinitions
{
    /// <summary>Key constant for the Technical Improvement category.</summary>
    public const string TechnicalImprovement = "TechnicalImprovement";

    /// <summary>Key constant for the Process Improvement category.</summary>
    public const string ProcessImprovement = "ProcessImprovement";

    /// <summary>Key constant for the Client Solution category.</summary>
    public const string ClientSolution = "ClientSolution";

    /// <summary>
    /// All supported categories, keyed by their stable string key.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, CategoryDefinition> All =
        new Dictionary<string, CategoryDefinition>
        {
            [TechnicalImprovement] = new CategoryDefinition
            {
                Key = TechnicalImprovement,
                DisplayName = "Technical Improvement",
                Fields = new[]
                {
                    new CategoryFieldDefinition
                    {
                        Key = "TechArea",
                        Label = "Technology Area",
                        InputType = "select",
                        Options = new[] { "Backend", "Frontend", "Infrastructure", "Security", "Data/Analytics", "Other" },
                        IsRequired = true,
                        GuidanceHint = "Select the primary technology domain your idea addresses."
                    },
                    new CategoryFieldDefinition
                    {
                        Key = "TechEffort",
                        Label = "Estimated Implementation Effort",
                        InputType = "select",
                        Options = new[] { "Small — days", "Medium — weeks", "Large — months" },
                        IsRequired = true,
                        GuidanceHint = "Estimate the engineering effort needed to implement this idea."
                    },
                    new CategoryFieldDefinition
                    {
                        Key = "TechBenefit",
                        Label = "Expected Technical Benefit",
                        InputType = "textarea",
                        IsRequired = true,
                        MaxLength = 500,
                        GuidanceHint = "Describe the measurable technical improvement: performance gain, reliability, maintainability, or security."
                    }
                }
            },

            [ProcessImprovement] = new CategoryDefinition
            {
                Key = ProcessImprovement,
                DisplayName = "Process Improvement",
                Fields = new[]
                {
                    new CategoryFieldDefinition
                    {
                        Key = "ProcDepartment",
                        Label = "Affected Department or Team",
                        InputType = "text",
                        IsRequired = true,
                        MaxLength = 100,
                        GuidanceHint = "Name the team or department that would benefit most from this improvement."
                    },
                    new CategoryFieldDefinition
                    {
                        Key = "ProcPainPoint",
                        Label = "Current Process Pain Point",
                        InputType = "textarea",
                        IsRequired = true,
                        MaxLength = 500,
                        GuidanceHint = "Describe the specific inefficiency, bottleneck, or friction point this idea addresses."
                    },
                    new CategoryFieldDefinition
                    {
                        Key = "ProcSavings",
                        Label = "Estimated Savings",
                        InputType = "text",
                        IsRequired = false,
                        MaxLength = 200,
                        GuidanceHint = "Optional. Estimate time or cost savings (e.g. \"2 hours/week per team member\")."
                    }
                }
            },

            [ClientSolution] = new CategoryDefinition
            {
                Key = ClientSolution,
                DisplayName = "Client Solution",
                Fields = new[]
                {
                    new CategoryFieldDefinition
                    {
                        Key = "ClientSegment",
                        Label = "Target Client Segment",
                        InputType = "text",
                        IsRequired = true,
                        MaxLength = 200,
                        GuidanceHint = "Name the client type or segment this solution is designed for."
                    },
                    new CategoryFieldDefinition
                    {
                        Key = "ClientProblem",
                        Label = "Client Problem Being Solved",
                        InputType = "textarea",
                        IsRequired = true,
                        MaxLength = 500,
                        GuidanceHint = "Describe the client's unmet need or pain point that this idea addresses."
                    },
                    new CategoryFieldDefinition
                    {
                        Key = "ClientImpact",
                        Label = "Expected Business Impact",
                        InputType = "text",
                        IsRequired = true,
                        MaxLength = 300,
                        GuidanceHint = "Describe the measurable business outcome for the client (revenue, retention, satisfaction)."
                    }
                }
            }
        };
}
