using FluentValidation;
using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Validators;

/// <summary>
/// Validates the <see cref="AdvanceStageViewModel"/> submitted when an admin advances
/// an idea to the next review stage.
/// </summary>
public class AdvanceStageValidator : AbstractValidator<AdvanceStageViewModel>
{
    public AdvanceStageValidator()
    {
        RuleFor(x => x.IdeaId)
            .NotEmpty().WithMessage("Idea ID is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => x.Notes != null);
    }
}
