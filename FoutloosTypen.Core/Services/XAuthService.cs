using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;  

namespace FoutloosTypen.Core.Services
{

    public class XAuthService : IXAuthService
    {
        private const string AuthorizationEndpoint = "https://x.com/i/oauth2/authorize";
        private const string TokenEndpoint = "https://api.twitter.com/2/oauth2/token";
        private const string TweetEndpoint = "https://api.x.com/2/tweets";
        private const string MediaUploadV11 = "https://upload.twitter.com/1.1/media/upload.json";

        public async Task<string?> AuthenticateAsync(string clientId, string redirectUri, string[] scopes, bool forceConsent = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri))
                {
                    Debug.WriteLine($"X OAuth authenticate error: Missing clientId or redirectUri. clientId set: {!string.IsNullOrWhiteSpace(clientId)}, redirectUri set: {!string.IsNullOrWhiteSpace(redirectUri)}");
                    return null;
                }

                var (verifier, challenge) = CreatePkcePair();
                await SecureStorage.SetAsync("x_pkce_verifier", verifier);

                var required = new HashSet<string>(StringComparer.Ordinal)
                { "tweet.write", "users.read", "offline.access" };
                foreach (var s in scopes) required.Add(s);
                var scopeValue = string.Join(' ', required);

                var promptParam = forceConsent ? "&prompt=consent" : string.Empty;

                // Determine an effective redirect URI usable by HttpListener on Windows.
                // If the provided redirectUri is not an absolute http(s) URI, open a random loopback port and use that.
                string effectiveRedirectUri = redirectUri;
                if (OperatingSystem.IsWindows())
                {
                    if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var parsed) || (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps))
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

                // Use runtime platform detection so Windows runtime gets the custom loopback listener
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

        public async Task<string> UploadMediaV11Async(string filePath, string contentType, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
#if DEBUG
            try
            {
                var maskedToken = string.IsNullOrEmpty(accessToken) ? "(empty)" : (accessToken.Length > 8 ? accessToken.Substring(0, 4) + "..." + accessToken.Substring(accessToken.Length - 4) : accessToken);
                var maskedSecret = string.IsNullOrEmpty(accessTokenSecret) ? "(empty)" : (accessTokenSecret.Length > 8 ? accessTokenSecret.Substring(0, 4) + "..." + accessTokenSecret.Substring(accessTokenSecret.Length - 4) : accessTokenSecret);
                Debug.WriteLine($"UploadMediaV11Async using OAuth1 token={maskedToken} secret={maskedSecret}");
            }
            catch
            {
                // ignore logging errors
            }
#endif

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var totalBytes = fileBytes.Length;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

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

        public async Task CreateTweetAsync(string text, string? mediaId = null)
        {
            var accessToken = await SecureStorage.GetAsync("x_access_token");
            if (string.IsNullOrEmpty(accessToken))
                throw new InvalidOperationException("Missing X access token");

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            object payload = mediaId is not null
                ? new { text = text, media = new { media_ids = new[] { mediaId } } }
                : new { text = text };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var resp = await http.PostAsync(TweetEndpoint, content);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Tweet create failed: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}");
        }

        public async Task CreateTweetWithOAuth1Async(string text, string? mediaId, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            if (string.IsNullOrWhiteSpace(consumerKey) || string.IsNullOrWhiteSpace(consumerSecret) ||
                string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(accessTokenSecret))
            {
                throw new ArgumentException("OAuth1 credentials (consumer key/secret and oauth token/secret) must be provided to post with user context.");
            }

            using var http = new HttpClient();

            object payload = mediaId is not null
                ? new { text = text, media = new { media_ids = new[] { mediaId } } }
                : new { text = text };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var header = "OAuth " + BuildOAuth1Header(TweetEndpoint, HttpMethod.Post, Enumerable.Empty<KeyValuePair<string, string>>(), consumerKey, consumerSecret, accessToken, accessTokenSecret);
            var request = new HttpRequestMessage(HttpMethod.Post, TweetEndpoint)
            {
                Content = content
            };
            request.Headers.TryAddWithoutValidation("Authorization", header);
            request.Headers.TryAddWithoutValidation("user-agent", "v2CreateTweetCSharp");
            request.Headers.TryAddWithoutValidation("accept", "application/json");
            request.Headers.TryAddWithoutValidation("content-type", "application/json");

            var resp = await http.SendAsync(request);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Tweet create failed: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}");
        }

        public Task<string?> GetStoredAccessTokenAsync() => SecureStorage.GetAsync("x_access_token");
        public Task<string?> GetStoredRefreshTokenAsync() => SecureStorage.GetAsync("x_refresh_token");
        public Task<string?> GetAuthenticatedHandleAsync() => SecureStorage.GetAsync("x_handle");

        public async Task SignOutAsync()
        {
            SecureStorage.Remove("x_access_token");
            SecureStorage.Remove("x_refresh_token");
            SecureStorage.Remove("x_pkce_verifier");
            SecureStorage.Remove("x_handle");
            SecureStorage.Remove("x_oauth_token");
            SecureStorage.Remove("x_oauth_token_secret");
        }

        public Task<string?> GetStoredOAuth1TokenAsync() => SecureStorage.GetAsync("x_oauth_token");
        public Task<string?> GetStoredOAuth1SecretAsync() => SecureStorage.GetAsync("x_oauth_token_secret");

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

        public string? Authenticate(string clientId, string redirectUri, string[] scopes, bool forceConsent = false)
        {
            return AuthenticateAsync(clientId, redirectUri, scopes, forceConsent).GetAwaiter().GetResult();
        }

        public string? ExchangeCodeForToken(string clientId, string code, string redirectUri)
        {
            return ExchangeCodeForTokenAsync(clientId, code, redirectUri).GetAwaiter().GetResult();
        }

        public string? RefreshAccessToken(string clientId, string refreshToken, string redirectUri)
        {
            return RefreshAccessTokenAsync(clientId, refreshToken, redirectUri).GetAwaiter().GetResult();
        }

        public string UploadMediaV11(string filePath, string contentType, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            return UploadMediaV11Async(filePath, contentType, consumerKey, consumerSecret, accessToken, accessTokenSecret).GetAwaiter().GetResult();
        }

        public void CreateTweet(string text, string? mediaId = null)
        {
            CreateTweetAsync(text, mediaId).GetAwaiter().GetResult();
        }

        public string? GetStoredAccessToken()
        {
            return GetStoredAccessTokenAsync().GetAwaiter().GetResult();
        }

        public string? GetStoredRefreshToken()
        {
            return GetStoredRefreshTokenAsync().GetAwaiter().GetResult();
        }

        public string? GetAuthenticatedHandle()
        {
            return GetAuthenticatedHandleAsync().GetAwaiter().GetResult();
        }

        public void SignOut()
        {
            SignOutAsync().GetAwaiter().GetResult();
        }

        public string? GetStoredOAuth1Token()
        {
            return GetStoredOAuth1TokenAsync().GetAwaiter().GetResult();
        }

        public string? GetStoredOAuth1Secret()
        {
            return GetStoredOAuth1SecretAsync().GetAwaiter().GetResult();
        }
    }
}
