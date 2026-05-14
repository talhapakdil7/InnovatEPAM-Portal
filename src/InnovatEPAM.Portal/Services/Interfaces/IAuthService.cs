using InnovatEPAM.Portal.Models;

namespace InnovatEPAM.Portal.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<bool> LoginAsync(string email, string password, bool rememberMe);
    Task LogoutAsync();
}
