using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.ApplicationModel;
using FoutloosTypen.Core.Interfaces.Repositories;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class XAuthApiRepository : IXAuthApiRepository
    {
        private const string RequestTokenEndpoint = "https://api.twitter.com/oauth/request_token";
        private const string AuthorizeEndpoint = "https://api.twitter.com/oauth/authorize";
        private const string AccessTokenEndpoint = "https://api.twitter.com/oauth/access_token";

        public string PrepareRedirectUri(string callbackUrl)
        {
            if (!OperatingSystem.IsWindows())
                return callbackUrl;

            if (Uri.TryCreate(callbackUrl, UriKind.Absolute, out var parsed) && 
                (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps))
                return callbackUrl;

            var listenerForPort = new TcpListener(IPAddress.Loopback, 0);
            listenerForPort.Start();
            var port = ((IPEndPoint)listenerForPort.LocalEndpoint).Port;
            listenerForPort.Stop();

            return $"http://127.0.0.1:{port}/";
        }

        public async Task<string?> AuthenticateWithBrowserAsync(string authUrl, string effectiveCallbackUrl)
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    var prefix = effectiveCallbackUrl.EndsWith("/") ? effectiveCallbackUrl : effectiveCallbackUrl + "/";
                    using var listener = new System.Net.HttpListener();
                    listener.Prefixes.Add(prefix);
                    listener.Start();

                    await Launcher.OpenAsync(new Uri(authUrl));

                    var context = await listener.GetContextAsync();
                    var query = context.Request.Url?.Query ?? string.Empty;
                    var parsed = System.Web.HttpUtility.ParseQueryString(query);
                    var verifier = parsed.Get("oauth_verifier");

                    var responseString = "<html><body><h3>Je kunt het venster sluiten.</h3></body></html>";
                    var buffer = Encoding.UTF8.GetBytes(responseString);
                    context.Response.ContentLength64 = buffer.Length;
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    context.Response.OutputStream.Close();
                    listener.Stop();

                    return string.IsNullOrEmpty(verifier) ? null : verifier;
                }
                else
                {
                    var result = await WebAuthenticator.AuthenticateAsync(new Uri(authUrl), new Uri(effectiveCallbackUrl));
                    if (result?.Properties != null && result.Properties.TryGetValue("oauth_verifier", out var verifier))
                        return verifier;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public async Task<(string? requestToken, string? requestTokenSecret)> GetRequestTokenAsync(
            string consumerKey,
            string consumerSecret,
            string callbackUrl)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var nonce = Guid.NewGuid().ToString("N");

            var parameters = new SortedDictionary<string, string>
            {
                { "oauth_callback", callbackUrl },
                { "oauth_consumer_key", consumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timestamp },
                { "oauth_version", "1.0" }
            };

            var signature = GenerateSignature("POST", RequestTokenEndpoint, parameters, consumerSecret, null);
            parameters.Add("oauth_signature", signature);

            var authHeader = BuildAuthorizationHeader(parameters);

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, RequestTokenEndpoint);
            request.Headers.Add("Authorization", authHeader);

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return (null, null);

            var responseParams = ParseQueryString(content);
            var requestToken = responseParams.ContainsKey("oauth_token") ? responseParams["oauth_token"] : null;
            var requestTokenSecret = responseParams.ContainsKey("oauth_token_secret") ? responseParams["oauth_token_secret"] : null;

            return (requestToken, requestTokenSecret);
        }

        public async Task<(string? accessToken, string? accessTokenSecret)> ExchangeRequestTokenAsync(
            string consumerKey,
            string consumerSecret,
            string requestToken,
            string requestTokenSecret,
            string oauthVerifier)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var nonce = Guid.NewGuid().ToString("N");

            var parameters = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timestamp },
                { "oauth_token", requestToken },
                { "oauth_verifier", oauthVerifier },
                { "oauth_version", "1.0" }
            };

            var signature = GenerateSignature("POST", AccessTokenEndpoint, parameters, consumerSecret, requestTokenSecret);
            parameters.Add("oauth_signature", signature);

            var authHeader = BuildAuthorizationHeader(parameters);

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, AccessTokenEndpoint);
            request.Headers.Add("Authorization", authHeader);

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return (null, null);

            var responseParams = ParseQueryString(content);
            var accessToken = responseParams.ContainsKey("oauth_token") ? responseParams["oauth_token"] : null;
            var accessTokenSecret = responseParams.ContainsKey("oauth_token_secret") ? responseParams["oauth_token_secret"] : null;

            return (accessToken, accessTokenSecret);
        }

        public async Task<(string? userId, string? username)?> VerifyCredentialsAsync(
            string consumerKey,
            string consumerSecret,
            string accessToken,
            string accessTokenSecret)
        {
            const string verifyUrl = "https://api.twitter.com/1.1/account/verify_credentials.json";
            
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var nonce = Guid.NewGuid().ToString("N");

            var parameters = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timestamp },
                { "oauth_token", accessToken },
                { "oauth_version", "1.0" }
            };

            var signature = GenerateSignature("GET", verifyUrl, parameters, consumerSecret, accessTokenSecret);
            parameters.Add("oauth_signature", signature);

            var authHeader = BuildAuthorizationHeader(parameters);

            try
            {
                using var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, verifyUrl);
                request.Headers.Add("Authorization", authHeader);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                
                // Parse JSON manually (simple parsing zonder extra dependencies)
                var userId = ExtractJsonValue(content, "id_str");
                var username = ExtractJsonValue(content, "screen_name");

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
                    return null;

                return (userId, username);
            }
            catch
            {
                return null;
            }
        }

        private string GenerateSignature(string method, string url, SortedDictionary<string, string> parameters, string consumerSecret, string? tokenSecret)
        {
            var paramString = string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            var signatureBase = $"{method}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(paramString)}";
            var signingKey = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(tokenSecret ?? "")}";

            using var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
            var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(signatureBase));
            return Convert.ToBase64String(hash);
        }

        private string BuildAuthorizationHeader(SortedDictionary<string, string> parameters)
        {
            var headerParams = string.Join(", ", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}=\"{Uri.EscapeDataString(p.Value)}\""));
            return $"OAuth {headerParams}";
        }

        private Dictionary<string, string> ParseQueryString(string query)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(query)) return result;

            var pairs = query.Split('&');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    result[Uri.UnescapeDataString(parts[0])] = Uri.UnescapeDataString(parts[1]);
                }
            }
            return result;
        }

        private string? ExtractJsonValue(string json, string key)
        {
            var searchKey = $"\"{key}\":";
            var startIndex = json.IndexOf(searchKey, StringComparison.Ordinal);
            if (startIndex == -1)
                return null;

            startIndex += searchKey.Length;
            
            // Skip whitespace and quotes
            while (startIndex < json.Length && (json[startIndex] == ' ' || json[startIndex] == '"'))
                startIndex++;

            if (startIndex >= json.Length)
                return null;

            var endIndex = startIndex;
            while (endIndex < json.Length && json[endIndex] != '"' && json[endIndex] != ',' && json[endIndex] != '}')
                endIndex++;

            if (endIndex <= startIndex)
                return null;

            return json.Substring(startIndex, endIndex - startIndex);
        }
    }
}
