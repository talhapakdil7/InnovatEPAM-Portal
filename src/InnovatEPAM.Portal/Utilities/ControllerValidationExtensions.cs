using Microsoft.AspNetCore.Mvc;

namespace InnovatEPAM.Portal.Utilities;

/// <summary>After PRG, copies validation messages into TempData.</summary>
public static class ControllerValidationExtensions
{
    public const string ValidationErrorsTempDataKey = "ValidationErrors";

    public static void AddModelErrorsToTempData(this Controller controller)
    {
        var messages = controller.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? null : e.ErrorMessage)
            .Where(m => m != null)
            .Cast<string>()
            .ToList();

        if (messages.Count > 0)
            controller.TempData[ValidationErrorsTempDataKey] = string.Join("\n", messages.Distinct());
    }
}
