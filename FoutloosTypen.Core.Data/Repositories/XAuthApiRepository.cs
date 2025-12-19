using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.ApplicationModel;
using FoutloosTypen.Core.Interfaces.Repositories;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class XAuthApiRepository : IXAuthApiRepository
    {
        private const string AuthorizationEndpoint = "https://x.com/i/oauth2/authorize";
        private const string TokenEndpoint = "https://api.twitter.com/2/oauth2/token";

        public string PrepareRedirectUri(string redirectUri)
        {
            if (!OperatingSystem.IsWindows())
                return redirectUri;

            if (Uri.TryCreate(redirectUri, UriKind.Absolute, out var parsed) && 
                (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps))
                return redirectUri;

            var listenerForPort = new TcpListener(IPAddress.Loopback, 0);
            listenerForPort.Start();
            var port = ((IPEndPoint)listenerForPort.LocalEndpoint).Port;
            listenerForPort.Stop();

            return $"http://127.0.0.1:{port}/";
        }

        public string BuildAuthUrl(string clientId, string redirectUri, string[] scopes, string challenge, bool forceConsent)
        {
            var required = new HashSet<string>(StringComparer.Ordinal)
            { "tweet.write", "users.read", "offline.access" };
            foreach (var s in scopes) required.Add(s);
            var scopeValue = string.Join(' ', required);

            var promptParam = forceConsent ? "&prompt=consent" : string.Empty;

            return $"{AuthorizationEndpoint}?response_type=code&client_id={Uri.EscapeDataString(clientId)}" +
                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scopeValue)}" +
                   $"&state={Guid.NewGuid():N}&code_challenge={challenge}&code_challenge_method=S256" +
                   promptParam;
        }

        public async Task<string?> AuthenticateWithBrowserAsync(string authUrl, string effectiveRedirectUri)
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    var prefix = effectiveRedirectUri.EndsWith("/") ? effectiveRedirectUri : effectiveRedirectUri + "/";
                    using var listener = new System.Net.HttpListener();
                    listener.Prefixes.Add(prefix);
                    listener.Start();

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
            catch
            {
                return null;
            }

            return null;
        }

        public async Task<(string? accessToken, string? refreshToken, string? scope)> ExchangeCodeForTokenAsync(
            string clientId,
            string code,
            string redirectUri,
            string codeVerifier)
        {
            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type","authorization_code"),
                new KeyValuePair<string,string>("code", code),
                new KeyValuePair<string,string>("client_id", clientId),
                new KeyValuePair<string,string>("redirect_uri", redirectUri),
                new KeyValuePair<string,string>("code_verifier", codeVerifier),
            });

            var response = await client.PostAsync(TokenEndpoint, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return (null, null, null);

            var doc = JsonDocument.Parse(json);
            var access = doc.RootElement.TryGetProperty("access_token", out var at) ? at.GetString() : null;
            var refresh = doc.RootElement.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            var scope = doc.RootElement.TryGetProperty("scope", out var sc) ? sc.GetString() : null;

            return (access, refresh, scope);
        }

        public async Task<(string? accessToken, string? refreshToken, string? scope)> RefreshAccessTokenAsync(
            string clientId,
            string refreshToken,
            string redirectUri)
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
                return (null, null, null);

            var doc = JsonDocument.Parse(json);
            var access = doc.RootElement.TryGetProperty("access_token", out var at) ? at.GetString() : null;
            var refresh = doc.RootElement.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            var scope = doc.RootElement.TryGetProperty("scope", out var sc) ? sc.GetString() : null;

            return (access, refresh, scope);
        }
    }
}
