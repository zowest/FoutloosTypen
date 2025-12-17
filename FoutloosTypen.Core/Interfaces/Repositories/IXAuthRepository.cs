using FoutloosTypen.Core.Models;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IXAuthRepository
    {
        XAuthSettings GetSettings();
        
        Task<string?> GetStoredAccessTokenAsync();
        Task<string?> GetStoredRefreshTokenAsync();
        Task<string?> GetAuthenticatedHandleAsync();
        Task<string?> GetStoredOAuth1TokenAsync();
        Task<string?> GetStoredOAuth1SecretAsync();
        Task<string?> GetPkceVerifierAsync();
        
        Task SaveAccessTokenAsync(string token);
        Task SaveRefreshTokenAsync(string token);
        Task SaveScopeAsync(string scope);
        Task SaveClientInfoAsync(string clientId, string redirectUri);
        Task SavePkceVerifierAsync(string verifier);
        
        Task ClearAllTokensAsync();
    }
}
