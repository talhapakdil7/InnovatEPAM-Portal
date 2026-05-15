using FluentValidation;
using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Validators;

/// <summary>
/// Validator for <see cref="EditDraftViewModel"/> used exclusively on the Submit Draft path.
/// Draft save and update operations bypass this validator (no required-field enforcement).
/// Rules mirror <see cref="CreateIdeaValidator"/> to ensure consistent submission requirements.
/// </summary>
public class EditDraftValidator : AbstractValidator<EditDraftViewModel>
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png"
    };

    public EditDraftValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Please select a category.")
            .Must(c => c == null || CategoryDefinitions.All.ContainsKey(c))
            .WithMessage("The selected category is not valid.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must be at most 2000 characters.");

        When(x => x.Attachment != null, () =>
        {
            RuleFor(x => x.Attachment!.Length)
                .LessThanOrEqualTo(10 * 1024 * 1024)
                .WithMessage("Attachments must be at most 10 MB.");

            RuleFor(x => x.Attachment!.FileName)
                .Must(name => AllowedExtensions.Contains(Path.GetExtension(name)))
                .WithMessage("Allowed types: PDF, DOC, DOCX, XLS, XLSX, JPG, PNG.");
        });

        When(x => x.Category == CategoryDefinitions.TechnicalImprovement, () =>
        {
            var techFields = CategoryDefinitions.All[CategoryDefinitions.TechnicalImprovement].Fields;

            RuleFor(x => x.TechArea)
                .NotEmpty().WithMessage("Technology area is required.")
                .Must(v => v == null || techFields.First(f => f.Key == "TechArea").Options.Contains(v))
                .WithMessage("The selected technology area is not valid.");

            RuleFor(x => x.TechEffort)
                .NotEmpty().WithMessage("Estimated implementation effort is required.")
                .Must(v => v == null || techFields.First(f => f.Key == "TechEffort").Options.Contains(v))
                .WithMessage("The selected effort level is not valid.");

            RuleFor(x => x.TechBenefit)
                .NotEmpty().WithMessage("Expected technical benefit is required.")
                .MaximumLength(500).WithMessage("Expected technical benefit must be at most 500 characters.");
        });

        When(x => x.Category == CategoryDefinitions.ProcessImprovement, () =>
        {
            RuleFor(x => x.ProcDepartment)
                .NotEmpty().WithMessage("Affected unit or team is required.")
                .MaximumLength(100).WithMessage("Unit name must be at most 100 characters.");

            RuleFor(x => x.ProcPainPoint)
                .NotEmpty().WithMessage("Current process pain point is required.")
                .MaximumLength(500).WithMessage("Pain point description must be at most 500 characters.");

            RuleFor(x => x.ProcSavings)
                .MaximumLength(200).WithMessage("Estimated savings must be at most 200 characters.");
        });

        When(x => x.Category == CategoryDefinitions.ClientSolution, () =>
        {
            RuleFor(x => x.ClientSegment)
                .NotEmpty().WithMessage("Target customer segment is required.")
                .MaximumLength(200).WithMessage("Segment must be at most 200 characters.");

            RuleFor(x => x.ClientProblem)
                .NotEmpty().WithMessage("Customer problem addressed is required.")
                .MaximumLength(500).WithMessage("Problem description must be at most 500 characters.");

            RuleFor(x => x.ClientImpact)
                .NotEmpty().WithMessage("Expected business impact is required.")
                .MaximumLength(300).WithMessage("Business impact must be at most 300 characters.");
        });
    }
}
