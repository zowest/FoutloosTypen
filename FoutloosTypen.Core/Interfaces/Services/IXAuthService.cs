using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Services;

public interface IXAuthService
{
    Task<string?> AuthenticateAsync(string consumerKey, string consumerSecret, string callbackUrl);
    Task SignOutAsync();

    Task SaveUserAccessTokensAsync(int ownerUserId, string accessToken, string accessSecret);
    Task<(string AccessToken, string AccessSecret)?> GetUserAccessTokensAsync(int ownerUserId);
    Task DeleteUserAccessTokensAsync(int ownerUserId);
}
