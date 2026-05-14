using FluentValidation;
using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Validators;

public class CreateIdeaValidator : AbstractValidator<CreateIdeaViewModel>
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png"
    };

    public CreateIdeaValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Please select a category.")
            .Must(c => c == null || CategoryDefinitions.All.ContainsKey(c))
            .WithMessage("Selected category is not valid.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must be at most 2000 characters.");

        When(x => x.Attachment != null, () =>
        {
            RuleFor(x => x.Attachment!.Length)
                .LessThanOrEqualTo(10 * 1024 * 1024)
                .WithMessage("Attachment must be 10 MB or less.");

            RuleFor(x => x.Attachment!.FileName)
                .Must(name => AllowedExtensions.Contains(Path.GetExtension(name)))
                .WithMessage("Allowed file types: PDF, DOC, DOCX, XLS, XLSX, JPG, PNG.");
        });

        // Technical Improvement conditional rules
        When(x => x.Category == CategoryDefinitions.TechnicalImprovement, () =>
        {
            var techFields = CategoryDefinitions.All[CategoryDefinitions.TechnicalImprovement].Fields;

            RuleFor(x => x.TechArea)
                .NotEmpty().WithMessage("Technology Area is required.")
                .Must(v => v == null || techFields.First(f => f.Key == "TechArea").Options.Contains(v))
                .WithMessage("Selected Technology Area is not valid.");

            RuleFor(x => x.TechEffort)
                .NotEmpty().WithMessage("Estimated Implementation Effort is required.")
                .Must(v => v == null || techFields.First(f => f.Key == "TechEffort").Options.Contains(v))
                .WithMessage("Selected Effort level is not valid.");

            RuleFor(x => x.TechBenefit)
                .NotEmpty().WithMessage("Expected Technical Benefit is required.")
                .MaximumLength(500).WithMessage("Expected Technical Benefit must be at most 500 characters.");
        });

        // Process Improvement conditional rules
        When(x => x.Category == CategoryDefinitions.ProcessImprovement, () =>
        {
            RuleFor(x => x.ProcDepartment)
                .NotEmpty().WithMessage("Affected Department or Team is required.")
                .MaximumLength(100).WithMessage("Department must be at most 100 characters.");

            RuleFor(x => x.ProcPainPoint)
                .NotEmpty().WithMessage("Current Process Pain Point is required.")
                .MaximumLength(500).WithMessage("Pain Point must be at most 500 characters.");

            RuleFor(x => x.ProcSavings)
                .MaximumLength(200).WithMessage("Estimated Savings must be at most 200 characters.");
        });

        // Client Solution conditional rules
        When(x => x.Category == CategoryDefinitions.ClientSolution, () =>
        {
            RuleFor(x => x.ClientSegment)
                .NotEmpty().WithMessage("Target Client Segment is required.")
                .MaximumLength(200).WithMessage("Client Segment must be at most 200 characters.");

            RuleFor(x => x.ClientProblem)
                .NotEmpty().WithMessage("Client Problem Being Solved is required.")
                .MaximumLength(500).WithMessage("Client Problem must be at most 500 characters.");

            RuleFor(x => x.ClientImpact)
                .NotEmpty().WithMessage("Expected Business Impact is required.")
                .MaximumLength(300).WithMessage("Business Impact must be at most 300 characters.");
        });
    }
}
