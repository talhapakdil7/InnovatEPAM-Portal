using FluentValidation;
using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Validators;

/// <summary>
/// Validates the <see cref="RevertStageViewModel"/> submitted when an admin reverts
/// an idea to the previous review stage.
/// </summary>
public class RevertStageValidator : AbstractValidator<RevertStageViewModel>
{
    public RevertStageValidator()
    {
        RuleFor(x => x.IdeaId)
            .NotEmpty().WithMessage("Idea ID is required.");

        RuleFor(x => x.RevertReason)
            .NotEmpty().WithMessage("A revert reason is required.")
            .MaximumLength(500).WithMessage("Revert reason must not exceed 500 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => x.Notes != null);
    }
}
