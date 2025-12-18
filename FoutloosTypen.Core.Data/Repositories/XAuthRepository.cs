using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Maui.Storage;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class XAuthRepository : IXAuthRepository
    {
        private readonly XAuthSettings _settings;

        public XAuthRepository()
        {
            // Haal settings uit environment variables
            var settings = new XAuthSettings
            {
                ClientId = (Environment.GetEnvironmentVariable("CLIENT_ID") ?? string.Empty).Trim(),
                RedirectUri = (Environment.GetEnvironmentVariable("REDIRECT_URI") ?? string.Empty).Trim(),
                ConsumerKey = Environment.GetEnvironmentVariable("CONSUMER_KEY"),
                ConsumerSecret = Environment.GetEnvironmentVariable("CONSUMER_SECRET"),
                OAuthToken = Environment.GetEnvironmentVariable("OAUTH_TOKEN"),
                OAuthTokenSecret = Environment.GetEnvironmentVariable("OAUTH_TOKEN_SECRET")
            };

            Debug.WriteLine($"XAuthRepository: clientId set: {!string.IsNullOrEmpty(settings.ClientId)}, redirectUri set: {!string.IsNullOrEmpty(settings.RedirectUri)}");

            _settings = settings;
        }

        public XAuthSettings GetSettings() => _settings;

        public Task<string?> GetStoredAccessTokenAsync()
            => SecureStorage.GetAsync("x_access_token");

        public Task<string?> GetStoredRefreshTokenAsync()
            => SecureStorage.GetAsync("x_refresh_token");

        public Task<string?> GetAuthenticatedHandleAsync()
            => SecureStorage.GetAsync("x_handle");

        public Task<string?> GetStoredOAuth1TokenAsync()
            => SecureStorage.GetAsync("x_oauth_token");

        public Task<string?> GetStoredOAuth1SecretAsync()
            => SecureStorage.GetAsync("x_oauth_token_secret");

        public Task<string?> GetPkceVerifierAsync()
            => SecureStorage.GetAsync("x_pkce_verifier");

        public async Task SaveAccessTokenAsync(string token)
            => await SecureStorage.SetAsync("x_access_token", token);

        public async Task SaveRefreshTokenAsync(string token)
            => await SecureStorage.SetAsync("x_refresh_token", token);

        public async Task SaveScopeAsync(string scope)
            => await SecureStorage.SetAsync("x_scope", scope);

        public async Task SaveClientInfoAsync(string clientId, string redirectUri)
        {
            await SecureStorage.SetAsync("x_client_id", clientId);
            await SecureStorage.SetAsync("x_redirect_uri", redirectUri);
        }

        public async Task SavePkceVerifierAsync(string verifier)
            => await SecureStorage.SetAsync("x_pkce_verifier", verifier);

        public async Task ClearAllTokensAsync()
        {
            SecureStorage.Remove("x_access_token");
            SecureStorage.Remove("x_refresh_token");
            SecureStorage.Remove("x_pkce_verifier");
            SecureStorage.Remove("x_handle");
            SecureStorage.Remove("x_oauth_token");
            SecureStorage.Remove("x_oauth_token_secret");
            SecureStorage.Remove("x_client_id");
            SecureStorage.Remove("x_redirect_uri");
            SecureStorage.Remove("x_scope");
        }
    }
}