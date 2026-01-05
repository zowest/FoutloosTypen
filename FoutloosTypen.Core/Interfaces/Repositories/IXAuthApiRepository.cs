using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IXAuthApiRepository
    {
        string PrepareRedirectUri(string redirectUri);
        
        string BuildAuthUrl(string clientId, string redirectUri, string[] scopes, string challenge, bool forceConsent);
        
        Task<string?> AuthenticateWithBrowserAsync(
            string authUrl, 
            string effectiveRedirectUri);

        Task<(string? accessToken, string? refreshToken, string? scope)> ExchangeCodeForTokenAsync(
            string clientId, 
            string code, 
            string redirectUri, 
            string codeVerifier);

        Task<(string? accessToken, string? refreshToken, string? scope)> RefreshAccessTokenAsync(
            string clientId, 
            string refreshToken, 
            string redirectUri);
    }
}
