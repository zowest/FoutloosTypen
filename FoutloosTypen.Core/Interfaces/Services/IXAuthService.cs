using FoutloosTypen.Core.Models;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Services;

public interface IXAuthService
{
    Task<string?> AuthenticateAsync(string clientId, string redirectUri, string[] scopes, bool forceConsent = false);
    Task<string?> ExchangeCodeForTokenAsync(string clientId, string code, string redirectUri);
    Task<string?> RefreshAccessTokenAsync(string clientId, string refreshToken, string redirectUri);
    Task<string> UploadMediaV11Async(string filePath, string contentType, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret);
    Task CreateTweetAsync(string text, string? mediaId = null);
    Task<string?> GetStoredAccessTokenAsync();
    Task<string?> GetStoredRefreshTokenAsync();
    Task<string?> GetAuthenticatedHandleAsync();
    Task SignOutAsync();
    Task<string?> GetStoredOAuth1TokenAsync();
    Task<string?> GetStoredOAuth1SecretAsync();

    // Synchronous wrappers (kept for compatibility)
    string? Authenticate(string clientId, string redirectUri, string[] scopes, bool forceConsent = false);
    string? ExchangeCodeForToken(string clientId, string code, string redirectUri);
    string? RefreshAccessToken(string clientId, string refreshToken, string redirectUri);
    string UploadMediaV11(string filePath, string contentType, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret);
    void CreateTweet(string text, string? mediaId = null);
    string? GetStoredAccessToken();
    string? GetStoredRefreshToken();
    string? GetAuthenticatedHandle();
    void SignOut();
    string? GetStoredOAuth1Token();
    string? GetStoredOAuth1Secret();
}
