using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<string?> AuthenticateAsync(string clientId, string redirectUri, string[] scopes, bool forceConsent = false)
        {
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri))
                return null;

            var (verifier, challenge) = CreatePkcePair();
            await _xAuthRepository.SavePkceVerifierAsync(verifier);

            var effectiveRedirectUri = _xAuthApiRepository.PrepareRedirectUri(redirectUri);
            var authUrl = _xAuthApiRepository.BuildAuthUrl(clientId, effectiveRedirectUri, scopes, challenge, forceConsent);

            return await _xAuthApiRepository.AuthenticateWithBrowserAsync(authUrl, effectiveRedirectUri);
        }

        public async Task<string?> ExchangeCodeForTokenAsync(string clientId, string code, string redirectUri)
        {
            var verifier = await _xAuthRepository.GetPkceVerifierAsync() ?? string.Empty;
            var (access, refresh, scope) = await _xAuthApiRepository.ExchangeCodeForTokenAsync(
                clientId, code, redirectUri, verifier);

            if (string.IsNullOrEmpty(access))
                return null;

            await SaveTokensAsync(access, refresh, scope);
            await _xAuthRepository.SaveClientInfoAsync(clientId, redirectUri);

            return access;
        }

        public async Task<string?> RefreshAccessTokenAsync(string clientId, string refreshToken, string redirectUri)
        {
            var (access, refresh, scope) = await _xAuthApiRepository.RefreshAccessTokenAsync(
                clientId, refreshToken, redirectUri);

            if (string.IsNullOrEmpty(access))
                return null;

            await SaveTokensAsync(access, refresh, scope);
            return access;
        }

        public Task SignOutAsync() => _xAuthRepository.ClearAllTokensAsync();

        private async Task SaveTokensAsync(string? access, string? refresh, string? scope)
        {
            if (!string.IsNullOrEmpty(access))
                await _xAuthRepository.SaveAccessTokenAsync(access);
            if (!string.IsNullOrEmpty(refresh))
                await _xAuthRepository.SaveRefreshTokenAsync(refresh);
            if (!string.IsNullOrEmpty(scope))
                await _xAuthRepository.SaveScopeAsync(scope);
        }

        private static (string verifier, string challenge) CreatePkcePair()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            var verifier = Base64UrlEncode(bytes);
            using var sha = SHA256.Create();
            var challengeBytes = sha.ComputeHash(Encoding.ASCII.GetBytes(verifier));
            var challenge = Base64UrlEncode(challengeBytes);
            return (verifier, challenge);
        }

        private static string Base64UrlEncode(byte[] bytes)
            => Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
