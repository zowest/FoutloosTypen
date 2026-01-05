using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IXAuthApiRepository
    {
        string PrepareRedirectUri(string callbackUrl);
        
        Task<string?> AuthenticateWithBrowserAsync(string authUrl, string effectiveCallbackUrl);

        Task<(string? requestToken, string? requestTokenSecret)> GetRequestTokenAsync(
            string consumerKey,
            string consumerSecret,
            string callbackUrl);

        Task<(string? accessToken, string? accessTokenSecret)> ExchangeRequestTokenAsync(
            string consumerKey,
            string consumerSecret,
            string requestToken,
            string requestTokenSecret,
            string oauthVerifier);

        Task<(string? userId, string? username)?> VerifyCredentialsAsync(
            string consumerKey,
            string consumerSecret,
            string accessToken,
            string accessTokenSecret);
    }
}
