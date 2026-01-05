using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Interfaces.Repositories;

namespace FoutloosTypen.Core.Services
{
    public class XAuthService : IXAuthService
    {
        private readonly IXAuthRepository _xAuthRepository;
        private readonly IXAuthApiRepository _xAuthApiRepository;

        public XAuthService(IXAuthRepository xAuthRepository, IXAuthApiRepository xAuthApiRepository)
        {
            _xAuthRepository = xAuthRepository;
            _xAuthApiRepository = xAuthApiRepository;
        }

        public async Task<string?> AuthenticateAsync(string consumerKey, string consumerSecret, string callbackUrl)
        {
            if (string.IsNullOrWhiteSpace(consumerKey) || string.IsNullOrWhiteSpace(consumerSecret))
                return null;

            var effectiveCallback = _xAuthApiRepository.PrepareRedirectUri(callbackUrl);

            var (requestToken, requestTokenSecret) = await _xAuthApiRepository.GetRequestTokenAsync(
                consumerKey, consumerSecret, effectiveCallback);

            if (string.IsNullOrEmpty(requestToken) || string.IsNullOrEmpty(requestTokenSecret))
            {
                Debug.WriteLine("Failed to get request token");
                return null;
            }

            Debug.WriteLine($"Got request token: {requestToken}");

            var authorizeUrl = $"https://api.twitter.com/oauth/authorize?oauth_token={Uri.EscapeDataString(requestToken)}";
            var oauthVerifier = await _xAuthApiRepository.AuthenticateWithBrowserAsync(authorizeUrl, effectiveCallback);

            if (string.IsNullOrEmpty(oauthVerifier))
            {
                Debug.WriteLine("User cancelled or no verifier received");
                return null;
            }

            Debug.WriteLine($"Got oauth_verifier: {oauthVerifier}");
            return $"{requestToken}|{requestTokenSecret}|{oauthVerifier}";
        }

        public Task SignOutAsync() => _xAuthRepository.ClearAllTokensAsync();

        // OAuth1 per-user token management
        public Task SaveUserAccessTokensAsync(int ownerUserId, string accessToken, string accessSecret)
            => _xAuthRepository.SaveOAuth1TokensAsync(ownerUserId, accessToken, accessSecret);

        public Task<(string AccessToken, string AccessSecret)?> GetUserAccessTokensAsync(int ownerUserId)
            => _xAuthRepository.GetOAuth1TokensAsync(ownerUserId);

        public Task DeleteUserAccessTokensAsync(int ownerUserId)
            => _xAuthRepository.DeleteOAuth1TokensAsync(ownerUserId);
    }
}
