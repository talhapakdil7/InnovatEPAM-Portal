using FluentValidation;
using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Validators;

/// <summary>
/// FluentValidation validator for <see cref="SubmitScoreViewModel"/>.
/// Enforces the 1–5 range on each provided dimension and requires at least one dimension to be scored.
/// </summary>
public class SubmitScoreValidator : AbstractValidator<SubmitScoreViewModel>
{
    public SubmitScoreValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Innovation.HasValue
                    || x.TechnicalFeasibility.HasValue
                    || x.BusinessImpact.HasValue
                    || x.ImplementationValue.HasValue)
            .WithName("Score")
            .WithMessage("Score at least one dimension to save.");

        When(x => x.Innovation.HasValue, () =>
            RuleFor(x => x.Innovation!.Value)
                .InclusiveBetween(1, 5)
                .WithMessage("Innovation score must be between 1 and 5."));

        When(x => x.TechnicalFeasibility.HasValue, () =>
            RuleFor(x => x.TechnicalFeasibility!.Value)
                .InclusiveBetween(1, 5)
                .WithMessage("Technical feasibility score must be between 1 and 5."));

        When(x => x.BusinessImpact.HasValue, () =>
            RuleFor(x => x.BusinessImpact!.Value)
                .InclusiveBetween(1, 5)
                .WithMessage("Business impact score must be between 1 and 5."));

        When(x => x.ImplementationValue.HasValue, () =>
            RuleFor(x => x.ImplementationValue!.Value)
                .InclusiveBetween(1, 5)
                .WithMessage("Implementation value score must be between 1 and 5."));
    }
}
