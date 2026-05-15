using FluentValidation;
using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Validators;

public class RegisterValidator : AbstractValidator<RegisterViewModel>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Enter a valid email address.");

        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.").MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.").MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(12).WithMessage("Password must be at least 12 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}
