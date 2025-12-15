using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using System.Net;

namespace FoutloosTypen.Services
{
    public interface IXAuthService
    {
        Task<string?> AuthenticateAsync(string clientId, string redirectUri, string[] scopes);
        Task<string?> ExchangeCodeForTokenAsync(string clientId, string code, string redirectUri);
        Task<string?> RefreshAccessTokenAsync(string clientId, string refreshToken, string redirectUri);
        Task<string> UploadMediaAsync(string filePath, string contentType);
        Task CreateTweetAsync(string text, string? mediaId = null);
        Task<string?> GetStoredAccessTokenAsync();
        Task<string?> GetStoredRefreshTokenAsync();
        Task<string?> GetAuthenticatedHandleAsync();
    }

    public class XAuthService : IXAuthService
    {
        private const string AuthorizationEndpoint = "https://x.com/i/oauth2/authorize";
        private const string TokenEndpoint = "https://api.twitter.com/2/oauth2/token";
        private const string TweetEndpoint = "https://api.twitter.com/2/tweets";

        // v2 media upload endpoint
        private const string MediaUploadV2 = "https://api.twitter.com/2/media/upload";

        public async Task<string?> AuthenticateAsync(string clientId, string redirectUri, string[] scopes)
        {
            try
            {
                var (verifier, challenge) = CreatePkcePair();
                await SecureStorage.SetAsync("x_pkce_verifier", verifier);

                // Ensure media.write is present for v2 upload
                var required = new HashSet<string>(StringComparer.Ordinal)
                { "tweet.write", "users.read", "media.write", "offline.access" };
                foreach (var s in scopes) required.Add(s);
                var scopeValue = string.Join(' ', required);

                var authUrl =
                    $"{AuthorizationEndpoint}?response_type=code&client_id={Uri.EscapeDataString(clientId)}" +
                    $"&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scopeValue)}" +
                    $"&state={Guid.NewGuid():N}&code_challenge={challenge}&code_challenge_method=S256";

#if WINDOWS
                if (!redirectUri.StartsWith("http://127.0.0.1:"))
                {
                    Debug.WriteLine("X OAuth: On Windows, use a loopback redirect URI (e.g., http://127.0.0.1:5000/callback) registered with X.");
                    return null;
                }

                var uri = new Uri(redirectUri);
                var prefix = $"http://{uri.Host}:{uri.Port}/{uri.AbsolutePath.TrimStart('/')}";
                using var listener = new HttpListener();
                listener.Prefixes.Add(prefix.EndsWith("/") ? prefix : prefix + "/");
                listener.Start();

                await Launcher.Default.OpenAsync(new Uri(authUrl));

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
#else
                var result = await WebAuthenticator.AuthenticateAsync(new Uri(authUrl), new Uri(redirectUri));
                if (result?.Properties != null && result.Properties.TryGetValue("code", out var code))
                {
                    return code;
                }
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"X OAuth authenticate error: {ex.Message}");
            }

            return null;
        }

        public async Task<string?> ExchangeCodeForTokenAsync(string clientId, string code, string redirectUri)
        {
            try
            {
                using var client = new HttpClient();
                var verifier = await SecureStorage.GetAsync("x_pkce_verifier") ?? string.Empty;

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
                {
                    System.Diagnostics.Debug.WriteLine($"Token exchange failed: {(int)response.StatusCode} {response.ReasonPhrase} - {json}");
                    return null;
                }

                var doc = JsonDocument.Parse(json);
                var access = doc.RootElement.TryGetProperty("access_token", out var at) ? at.GetString() : null;
                var refresh = doc.RootElement.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;

                if (!string.IsNullOrEmpty(access))
                {
                    await SecureStorage.SetAsync("x_access_token", access);
                }

                if (!string.IsNullOrEmpty(refresh))
                {
                    await SecureStorage.SetAsync("x_refresh_token", refresh);
                }

                return access;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Token exchange HTTP error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Token exchange error: {ex.Message}");
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
                {
                    System.Diagnostics.Debug.WriteLine($"Refresh token failed: {(int)response.StatusCode} {response.ReasonPhrase} - {json}");
                    return null;
                }

                var doc = JsonDocument.Parse(json);
                var access = doc.RootElement.TryGetProperty("access_token", out var at) ? at.GetString() : null;
                var refresh = doc.RootElement.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;

                if (!string.IsNullOrEmpty(access))
                {
                    await SecureStorage.SetAsync("x_access_token", access);
                }

                if (!string.IsNullOrEmpty(refresh))
                {
                    await SecureStorage.SetAsync("x_refresh_token", refresh);
                }

                return access;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Refresh token HTTP error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Refresh token error: {ex.Message}");
                return null;
            }
        }

        public async Task<string> UploadMediaAsync(string filePath, string contentType)
        {
            var accessToken = await SecureStorage.GetAsync("x_access_token");
            if (string.IsNullOrEmpty(accessToken))
                throw new InvalidOperationException("Missing X access token");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptimizeHQXApp");

            var bytes = await File.ReadAllBytesAsync(filePath);
            var form = new MultipartFormDataContent
            {
                { new ByteArrayContent(bytes), "file", Path.GetFileName(filePath) },
                { new StringContent(contentType), "media_type" }
            };

            try
            {
                var resp = await client.PostAsync(MediaUploadV2, form);
                var json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    var status = (int)resp.StatusCode;
                    var reason = resp.ReasonPhrase;
                    var headers = string.Join("; ", resp.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"));
                    Debug.WriteLine($"Media upload failed: {status} {reason}");
                    Debug.WriteLine($"Response headers: {headers}");
                    Debug.WriteLine($"Response body: {json}");
                    throw new HttpRequestException($"Media upload failed: {status} {reason}");
                }

                using var doc = JsonDocument.Parse(json);
                var mediaId = doc.RootElement.TryGetProperty("media_id", out var mid) ? mid.GetString() : null;
                if (string.IsNullOrEmpty(mediaId))
                    throw new HttpRequestException("Media upload response missing media_id");

                return mediaId!;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Media upload HTTP error: {ex.Message}");
                throw;
            }
        }

        public async Task CreateTweetAsync(string text, string? mediaId = null)
        {
            var accessToken = await SecureStorage.GetAsync("x_access_token");
            if (string.IsNullOrEmpty(accessToken))
                throw new InvalidOperationException("Missing X access token");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptimizeHQXApp");

            var payload = new
            {
                text = text,
                media = mediaId is not null ? new { media_ids = new[] { mediaId } } : null
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            try
            {
                var resp = await client.PostAsync(TweetEndpoint, content);
                var responseJson = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                {
                    var status = (int)resp.StatusCode;
                    var reason = resp.ReasonPhrase;
                    Debug.WriteLine($"Tweet failed: {status} {reason} - {responseJson}");
                    throw new HttpRequestException($"Tweet create failed: {status} {reason}");
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Tweet HTTP error: {ex.Message}");
                throw;
            }
        }

        public Task<string?> GetStoredAccessTokenAsync() => SecureStorage.GetAsync("x_access_token");
        public Task<string?> GetStoredRefreshTokenAsync() => SecureStorage.GetAsync("x_refresh_token");

        public Task<string?> GetAuthenticatedHandleAsync()
        {
            return SecureStorage.GetAsync("x_handle");
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
        {
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        private static string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/png"
            };
        }
    }
}
