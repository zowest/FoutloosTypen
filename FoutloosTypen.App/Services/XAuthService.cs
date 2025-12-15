using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Storage;

namespace FoutloosTypen.Services
{
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
    }

    public class XAuthService : IXAuthService
    {
        private const string AuthorizationEndpoint = "https://x.com/i/oauth2/authorize";
        private const string TokenEndpoint = "https://api.twitter.com/2/oauth2/token";
        private const string TweetEndpoint = "https://api.x.com/2/tweets";
        private const string MediaUploadV11 = "https://upload.twitter.com/1.1/media/upload.json";

        #region OAuth2 Auth

        public async Task<string?> AuthenticateAsync(string clientId, string redirectUri, string[] scopes, bool forceConsent = false)
        {
            try
            {
                var (verifier, challenge) = CreatePkcePair();
                await SecureStorage.SetAsync("x_pkce_verifier", verifier);

                var required = new HashSet<string>(StringComparer.Ordinal)
                { "tweet.write", "users.read", "offline.access" };
                foreach (var s in scopes) required.Add(s);
                var scopeValue = string.Join(' ', required);

                var promptParam = forceConsent ? "&prompt=consent" : string.Empty;
                var authUrl =
                    $"{AuthorizationEndpoint}?response_type=code&client_id={Uri.EscapeDataString(clientId)}" +
                    $"&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scopeValue)}" +
                    $"&state={Guid.NewGuid():N}&code_challenge={challenge}&code_challenge_method=S256" +
                    promptParam;

#if WINDOWS
                // Loopback listener for Windows
                var uri = new Uri(redirectUri);
                var prefix = $"http://{uri.Host}:{uri.Port}/{uri.AbsolutePath.TrimStart('/')}";
                using var listener = new System.Net.HttpListener();
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
                    return code;
#endif
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
                    Debug.WriteLine($"Token exchange failed: {(int)response.StatusCode} {response.ReasonPhrase} - {json}");
                    return null;
                }

                var doc = JsonDocument.Parse(json);
                var access = doc.RootElement.TryGetProperty("access_token", out var at) ? at.GetString() : null;
                var refresh = doc.RootElement.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
                var scope = doc.RootElement.TryGetProperty("scope", out var sc) ? sc.GetString() : null;

                if (!string.IsNullOrEmpty(access))
                    await SecureStorage.SetAsync("x_access_token", access);
                if (!string.IsNullOrEmpty(refresh))
                    await SecureStorage.SetAsync("x_refresh_token", refresh);
                if (!string.IsNullOrEmpty(scope))
                    await SecureStorage.SetAsync("x_scope", scope);

                await SecureStorage.SetAsync("x_client_id", clientId);
                await SecureStorage.SetAsync("x_redirect_uri", redirectUri);

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
                {
                    Debug.WriteLine($"Refresh token failed: {(int)response.StatusCode} {response.ReasonPhrase} - {json}");
                    return null;
                }

                var doc = JsonDocument.Parse(json);
                var access = doc.RootElement.TryGetProperty("access_token", out var at) ? at.GetString() : null;
                var refresh = doc.RootElement.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
                var scope = doc.RootElement.TryGetProperty("scope", out var sc) ? sc.GetString() : null;

                if (!string.IsNullOrEmpty(access))
                    await SecureStorage.SetAsync("x_access_token", access);
                if (!string.IsNullOrEmpty(refresh))
                    await SecureStorage.SetAsync("x_refresh_token", refresh);
                if (!string.IsNullOrEmpty(scope))
                    await SecureStorage.SetAsync("x_scope", scope);

                return access;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Refresh token error: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Media Upload v1.1

        public async Task<string> UploadMediaV11Async(string filePath, string contentType, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var totalBytes = fileBytes.Length;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

            // INIT
            var initParams = new List<KeyValuePair<string, string>>
            {
                new("command", "INIT"),
                new("total_bytes", totalBytes.ToString()),
                new("media_type", contentType)
            };
            var initRequest = new HttpRequestMessage(HttpMethod.Post, MediaUploadV11)
            {
                Content = new FormUrlEncodedContent(initParams)
            };
            initRequest.Headers.Authorization = new AuthenticationHeaderValue("OAuth", BuildOAuth1Header(MediaUploadV11, HttpMethod.Post, initParams, consumerKey, consumerSecret, accessToken, accessTokenSecret));
            var initResp = await client.SendAsync(initRequest);
            var initJson = await initResp.Content.ReadAsStringAsync();
            if (!initResp.IsSuccessStatusCode)
                throw new HttpRequestException($"v1.1 INIT failed: {(int)initResp.StatusCode} {initResp.ReasonPhrase} - {initJson}");

            using var initDoc = JsonDocument.Parse(initJson);
            var mediaId = initDoc.RootElement.GetProperty("media_id_string").GetString();

            // APPEND (one chunk, base64)
            var base64 = Convert.ToBase64String(fileBytes);
            var appendParams = new List<KeyValuePair<string, string>>
            {
                new("command", "APPEND"),
                new("media_id", mediaId!),
                new("segment_index", "0"),
                new("media_data", base64)
            };
            var appendRequest = new HttpRequestMessage(HttpMethod.Post, MediaUploadV11)
            {
                Content = new FormUrlEncodedContent(appendParams)
            };
            appendRequest.Headers.Authorization = new AuthenticationHeaderValue("OAuth", BuildOAuth1Header(MediaUploadV11, HttpMethod.Post, appendParams, consumerKey, consumerSecret, accessToken, accessTokenSecret));
            var appendResp = await client.SendAsync(appendRequest);
            if (!appendResp.IsSuccessStatusCode)
                throw new HttpRequestException($"v1.1 APPEND failed: {(int)appendResp.StatusCode} {appendResp.ReasonPhrase}");

            // FINALIZE
            var finalizeParams = new List<KeyValuePair<string, string>>
            {
                new("command", "FINALIZE"),
                new("media_id", mediaId!)
            };
            var finalizeRequest = new HttpRequestMessage(HttpMethod.Post, MediaUploadV11)
            {
                Content = new FormUrlEncodedContent(finalizeParams)
            };
            finalizeRequest.Headers.Authorization = new AuthenticationHeaderValue("OAuth", BuildOAuth1Header(MediaUploadV11, HttpMethod.Post, finalizeParams, consumerKey, consumerSecret, accessToken, accessTokenSecret));
            var finalizeResp = await client.SendAsync(finalizeRequest);
            var finalizeJson = await finalizeResp.Content.ReadAsStringAsync();
            if (!finalizeResp.IsSuccessStatusCode)
                throw new HttpRequestException($"v1.1 FINALIZE failed: {(int)finalizeResp.StatusCode} {finalizeResp.ReasonPhrase} - {finalizeJson}");

            return mediaId!;
        }

        private static string BuildOAuth1Header(string url, HttpMethod method, IEnumerable<KeyValuePair<string, string>> requestParams, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            var oauthParams = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_nonce", Guid.NewGuid().ToString("N") },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
                { "oauth_token", accessToken },
                { "oauth_version", "1.0" }
            };

            foreach (var kv in requestParams)
                oauthParams[kv.Key] = kv.Value;

            var uri = new Uri(url);
            var normalizedUrl = uri.GetLeftPart(UriPartial.Path);

            var paramString = string.Join('&', oauthParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var signatureBaseString = $"{method.Method.ToUpperInvariant()}&{Uri.EscapeDataString(normalizedUrl)}&{Uri.EscapeDataString(paramString)}";
            var signingKey = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(accessTokenSecret)}";

            using var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
            var signature = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString)));

            var headerParams = new Dictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_nonce", oauthParams["oauth_nonce"] },
                { "oauth_signature", signature },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", oauthParams["oauth_timestamp"] },
                { "oauth_token", accessToken },
                { "oauth_version", "1.0" }
            };

            return string.Join(", ", headerParams.Select(kv => $"{kv.Key}=\"{Uri.EscapeDataString(kv.Value)}\""));
        }

        #endregion

        #region Tweet Create

        public async Task CreateTweetAsync(string text, string? mediaId = null)
        {
            var accessToken = await SecureStorage.GetAsync("x_access_token");
            if (string.IsNullOrEmpty(accessToken))
                throw new InvalidOperationException("Missing X access token");

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            object payload;
            if (mediaId is not null)
            {
                payload = new { text = text, media = new { media_ids = new[] { mediaId } } };
            }
            else
            {
                payload = new { text = text };
            }

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var resp = await http.PostAsync(TweetEndpoint, content);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Tweet create failed: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}");
        }

        #endregion

        #region Storage

        public Task<string?> GetStoredAccessTokenAsync() => SecureStorage.GetAsync("x_access_token");
        public Task<string?> GetStoredRefreshTokenAsync() => SecureStorage.GetAsync("x_refresh_token");
        public Task<string?> GetAuthenticatedHandleAsync() => SecureStorage.GetAsync("x_handle");

        public async Task SignOutAsync()
        {
            SecureStorage.Remove("x_access_token");
            SecureStorage.Remove("x_refresh_token");
            SecureStorage.Remove("x_pkce_verifier");
            SecureStorage.Remove("x_handle");
        }

        #endregion

        #region PKCE Helper

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
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        #endregion
    }
}
