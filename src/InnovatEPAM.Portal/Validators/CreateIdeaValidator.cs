using FluentValidation;
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
    }
}
