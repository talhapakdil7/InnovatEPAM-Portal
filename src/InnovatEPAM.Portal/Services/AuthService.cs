using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace InnovatEPAM.Portal.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(
        string email, string password, string firstName, string lastName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            CreatedDate = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, "Submitter");
        _logger.LogInformation("New user registered: {Email}", email);
        return (true, Enumerable.Empty<string>());
    }

    public async Task<(bool Success, bool IsLockedOut)> LoginAsync(string email, string password, bool rememberMe)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
            _logger.LogInformation("User logged in: {Email}", email);
        else
            _logger.LogWarning("Failed login attempt for: {Email}", email);

        return (result.Succeeded, result.IsLockedOut);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
