using FluentValidation;
using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Validators;

public class LoginValidator : AbstractValidator<LoginViewModel>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Enter a valid email address.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}
