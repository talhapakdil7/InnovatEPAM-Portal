using FluentValidation;
using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Validators;

/// <summary>
/// Validates the <see cref="RecordDecisionViewModel"/> submitted when an admin records
/// the final outcome at the Final Decision stage.
/// </summary>
public class RecordDecisionValidator : AbstractValidator<RecordDecisionViewModel>
{
    private static readonly string[] ValidOutcomes = ["Accepted", "Rejected"];

    public RecordDecisionValidator()
    {
        RuleFor(x => x.IdeaId)
            .NotEmpty().WithMessage("Idea ID is required.");

        RuleFor(x => x.Outcome)
            .NotEmpty().WithMessage("An outcome is required.")
            .Must(o => ValidOutcomes.Contains(o))
            .WithMessage("Outcome must be either 'Accepted' or 'Rejected'.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => x.Notes != null);
    }
}
