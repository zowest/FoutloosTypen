using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Services;

public interface IXAuthService
{
    Task<string?> AuthenticateAsync(string clientId, string redirectUri, string[] scopes, bool forceConsent = false);
    Task<string?> ExchangeCodeForTokenAsync(string clientId, string code, string redirectUri);
    Task<string?> RefreshAccessTokenAsync(string clientId, string refreshToken, string redirectUri);
    Task SignOutAsync();
}
