using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Services.Interfaces;
using InnovatEPAM.Portal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InnovatEPAM.Portal.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(IAuthService authService, UserManager<ApplicationUser> userManager)
    {
        _authService = authService;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated != true) return View();

        if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
        return RedirectToAction("Index", "Ideas");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var (success, errors) = await _authService.RegisterAsync(vm.Email, vm.Password, vm.FirstName, vm.LastName);
        if (!success)
        {
            foreach (var error in errors) ModelState.AddModelError(string.Empty, error);
            return View(vm);
        }

        TempData["Success"] = "Account created. You can sign in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
            return RedirectToAction("Index", "Ideas");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(vm);

        var (success, lockedOut) = await _authService.LoginAsync(vm.Email, vm.Password, vm.RememberMe);
        if (!success)
        {
            ModelState.AddModelError(string.Empty,
                lockedOut
                    ? "Too many failed attempts. Your account is temporarily locked; try again later."
                    : "Invalid email or password.");
            return View(vm);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        var user = await _userManager.FindByEmailAsync(vm.Email);
        if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            return RedirectToAction("Dashboard", "Admin");

        return RedirectToAction("Index", "Ideas");
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction(nameof(Login));
    }
}
