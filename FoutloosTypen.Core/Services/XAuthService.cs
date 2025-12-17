using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.ApplicationModel;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Interfaces.Repositories;

namespace FoutloosTypen.Core.Services
{
    public class XAuthService : IXAuthService
    {
        private readonly IXAuthRepository _xAuthRepository;
        private const string AuthorizationEndpoint = "https://x.com/i/oauth2/authorize";
        private const string TokenEndpoint = "https://api.twitter.com/2/oauth2/token";

        public XAuthService(IXAuthRepository xAuthRepository) 
            => _xAuthRepository = xAuthRepository;

        public async Task<string?> AuthenticateAsync(string clientId, string redirectUri, string[] scopes, bool forceConsent = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri))
                    return null;

                var (verifier, challenge) = CreatePkcePair();
                await _xAuthRepository.SavePkceVerifierAsync(verifier);

                var required = new HashSet<string>(StringComparer.Ordinal)
                { "tweet.write", "users.read", "offline.access" };
                foreach (var s in scopes) required.Add(s);
                var scopeValue = string.Join(' ', required);

                var promptParam = forceConsent ? "&prompt=consent" : string.Empty;

                string effectiveRedirectUri = redirectUri;
                if (OperatingSystem.IsWindows())
                {
                    if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var parsed) || 
                        (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps))
                    {
                        // allocate an available ephemeral port
                        var listenerForPort = new TcpListener(IPAddress.Loopback, 0);
                        listenerForPort.Start();
                        var port = ((IPEndPoint)listenerForPort.LocalEndpoint).Port;
                        listenerForPort.Stop();

                        effectiveRedirectUri = $"http://127.0.0.1:{port}/";
                    }
                }

                var authUrl =
                    $"{AuthorizationEndpoint}?response_type=code&client_id={Uri.EscapeDataString(clientId)}" +
                    $"&redirect_uri={Uri.EscapeDataString(effectiveRedirectUri)}&scope={Uri.EscapeDataString(scopeValue)}" +
                    $"&state={Guid.NewGuid():N}&code_challenge={challenge}&code_challenge_method=S256" +
                    promptParam;

                if (OperatingSystem.IsWindows())
                {
                    var prefix = effectiveRedirectUri.EndsWith("/") ? effectiveRedirectUri : effectiveRedirectUri + "/";
                    using var listener = new System.Net.HttpListener();
                    listener.Prefixes.Add(prefix);
                    listener.Start();

                    // Open system browser to the auth URL
                    await Launcher.OpenAsync(new Uri(authUrl));

                    var context = await listener.GetContextAsync();
                    var query = context.Request.Url?.Query ?? string.Empty;
                    var parsed = System.Web.HttpUtility.ParseQueryString(query);
                    var code = parsed.Get("code");

                    var responseString = "<html><body><h3>Je kunt het venster sluiten.</h3></body></html>";
                    var buffer = Encoding.UTF8.GetBytes(responseString);
                    context.Response.ContentLength64 = buffer.Length;
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    context.Response.OutputStream.Close();
                    listener.Stop();

                    return string.IsNullOrEmpty(code) ? null : code;
                }
                else
                {
                    var result = await WebAuthenticator.AuthenticateAsync(new Uri(authUrl), new Uri(effectiveRedirectUri));
                    if (result?.Properties != null && result.Properties.TryGetValue("code", out var code))
                        return code;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"X OAuth authenticate error: {ex.Message}");
            }

            return null;
        }

        public async Task<string?> ExchangeCodeForTokenAsync(string clientId, string code, string redirectUri)
        {
            try
            {
                using var client = new HttpClient();
                var verifier = await _xAuthRepository.GetPkceVerifierAsync() ?? string.Empty;

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("grant_type","authorization_code"),
                    new KeyValuePair<string,string>("code", code),
                    new KeyValuePair<string,string>("client_id", clientId),
                    new KeyValuePair<string,string>("redirect_uri", redirectUri),
                    new KeyValuePair<string,string>("code_verifier", verifier),
                });

                var response = await client.PostAsync(TokenEndpoint, content);
                var json = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                    return null;

                var doc = JsonDocument.Parse(json);
                var access = doc.RootElement.TryGetProperty("access_token", out var at) ? at.GetString() : null;
                var refresh = doc.RootElement.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
                var scope = doc.RootElement.TryGetProperty("scope", out var sc) ? sc.GetString() : null;

                if (!string.IsNullOrEmpty(access))
                    await _xAuthRepository.SaveAccessTokenAsync(access);
                if (!string.IsNullOrEmpty(refresh))
                    await _xAuthRepository.SaveRefreshTokenAsync(refresh);
                if (!string.IsNullOrEmpty(scope))
                    await _xAuthRepository.SaveScopeAsync(scope);

                await _xAuthRepository.SaveClientInfoAsync(clientId, redirectUri);

                return access;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Token exchange error: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> RefreshAccessTokenAsync(string clientId, string refreshToken, string redirectUri)
        {
            try
            {
                using var client = new HttpClient();
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("grant_type","refresh_token"),
                    new KeyValuePair<string,string>("refresh_token", refreshToken),
                    new KeyValuePair<string,string>("client_id", clientId),
                    new KeyValuePair<string,string>("redirect_uri", redirectUri)
                });

                var response = await client.PostAsync(TokenEndpoint, content);
                var json = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                    return null;

                var doc = JsonDocument.Parse(json);
                var access = doc.RootElement.TryGetProperty("access_token", out var at) ? at.GetString() : null;
                var refresh = doc.RootElement.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
                var scope = doc.RootElement.TryGetProperty("scope", out var sc) ? sc.GetString() : null;

                if (!string.IsNullOrEmpty(access))
                    await _xAuthRepository.SaveAccessTokenAsync(access);
                if (!string.IsNullOrEmpty(refresh))
                    await _xAuthRepository.SaveRefreshTokenAsync(refresh);
                if (!string.IsNullOrEmpty(scope))
                    await _xAuthRepository.SaveScopeAsync(scope);

                return access;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Refresh token error: {ex.Message}");
                return null;
            }
        }

        public Task SignOutAsync() => _xAuthRepository.ClearAllTokensAsync();

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
