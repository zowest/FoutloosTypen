using FoutloosTypen.Core.Models;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IXAuthRepository
    {
        XAuthSettings GetSettings();
        
        Task<string?> GetAuthenticatedHandleAsync();
        
        Task ClearAllTokensAsync();

        // Per-user OAuth1 support
        Task SaveOAuth1TokensAsync(int ownerUserId, string accessToken, string accessSecret);
        Task<(string AccessToken, string AccessSecret)?> GetOAuth1TokensAsync(int ownerUserId);
        Task DeleteOAuth1TokensAsync(int ownerUserId);

        // X user info management
        Task SaveXUserInfoAsync(int ownerUserId, string xUserId, string xUsername);
        Task<(string XUserId, string XUsername)?> GetXUserInfoAsync(int ownerUserId);
        Task DeleteXUserInfoAsync(int ownerUserId);
    }
}
