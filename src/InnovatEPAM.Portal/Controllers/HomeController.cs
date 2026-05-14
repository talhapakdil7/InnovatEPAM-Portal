using Microsoft.AspNetCore.Mvc;

namespace InnovatEPAM.Portal.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Ideas");

        return RedirectToAction("Login", "Auth");
    }

    public IActionResult Error() => View();

    public IActionResult AccessDenied() => View();
}
