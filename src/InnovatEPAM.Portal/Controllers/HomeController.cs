using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InnovatEPAM.Portal.Models;
using System.Diagnostics;

namespace InnovatEPAM.Portal.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction(nameof(AdminController.Dashboard), "Admin");
            return RedirectToAction(nameof(IdeasController.Index), "Ideas");
        }
        return RedirectToAction("Login", "Auth");
    }

    public IActionResult Privacy() => View();

    [AllowAnonymous]
    public IActionResult Error()
    {
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ErrorMessage = TempData["ErrorMessage"] as string,
            IsConcurrencyError = TempData["IsConcurrencyError"] is true
        };
        return View(model);
    }

    public IActionResult AccessDenied() => View();
}
