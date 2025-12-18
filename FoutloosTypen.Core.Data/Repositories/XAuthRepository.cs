using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class XAuthRepository : IXAuthRepository
    {
        private readonly XAuthSettings _settings;

        public XAuthRepository()
        {
            var (env, source) = LoadEnvWithSource();

            string clientId = ReadValue(env, "CLIENT_ID", Environment.GetEnvironmentVariable("CLIENT_ID"));
            string redirectUri = ReadValue(env, "REDIRECT_URI", Environment.GetEnvironmentVariable("REDIRECT_URI"));
            string consumerKey = ReadValue(env, "CONSUMER_KEY", Environment.GetEnvironmentVariable("CONSUMER_KEY"));
            string consumerSecret = ReadValue(env, "CONSUMER_SECRET", Environment.GetEnvironmentVariable("CONSUMER_SECRET"));
            string oauthToken = ReadValue(env, "OAUTH_TOKEN", Environment.GetEnvironmentVariable("OAUTH_TOKEN"));
            string oauthTokenSecret = ReadValue(env, "OAUTH_TOKEN_SECRET", Environment.GetEnvironmentVariable("OAUTH_TOKEN_SECRET"));
            string scopesEnv = ReadValue(env, "SCOPES", Environment.GetEnvironmentVariable("SCOPES"));

            string[] scopesParsed = Array.Empty<string>();
            if (!string.IsNullOrWhiteSpace(scopesEnv))
            {
                scopesParsed = scopesEnv
                    .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
            }

            var settings = new XAuthSettings
            {
                ClientId = (clientId ?? string.Empty).Trim(),
                RedirectUri = (redirectUri ?? string.Empty).Trim(),
                Scopes = scopesParsed,
                ConsumerKey = consumerKey ?? string.Empty,
                ConsumerSecret = consumerSecret ?? string.Empty,
                OAuthToken = oauthToken ?? string.Empty,
                OAuthTokenSecret = oauthTokenSecret ?? string.Empty
            };

            Debug.WriteLine($"XAuthRepository: config source: {source}");
            Debug.WriteLine($"XAuthRepository: clientId set: {!string.IsNullOrWhiteSpace(settings.ClientId)}, redirectUri set: {!string.IsNullOrWhiteSpace(settings.RedirectUri)}, scopes count: {settings.Scopes?.Length ?? 0}");

            _settings = settings;
        }

        private static string ReadValue(IDictionary<string, string> env, string key, string? fallback)
            => (env.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v)) ? v : (fallback ?? string.Empty);

        private static (IDictionary<string, string> values, string source) LoadEnvWithSource()
        {
            // 1) Try MAUI app package (app project assets)
            var fromPackage = LoadEnvFromAppPackage(out var packageName);
            if (fromPackage.Count > 0)
                return (fromPackage, $"app package '{packageName}'");

            // 2) Try embedded resource (this assembly)
            var fromEmbedded = LoadEnvFromEmbeddedResource(out var embeddedName);
            if (fromEmbedded.Count > 0)
                return (fromEmbedded, $"embedded resource '{embeddedName}'");

            // 3) Nothing found
            return (new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), "environment variables (XApi.env not found)");
        }

        private static IDictionary<string, string> LoadEnvFromAppPackage(out string nameUsed)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            nameUsed = string.Empty;

            var candidates = new[]
            {
                "Resources/Raw/XApi.env",
                "Resources/XApi.env",
                "XApi.env"
            };

            foreach (var name in candidates)
            {
                try
                {
                    using var stream = FileSystem.OpenAppPackageFileAsync(name).GetAwaiter().GetResult();
                    using var reader = new StreamReader(stream);
                    ParseEnv(reader.ReadToEnd(), dict);
                    if (dict.Count > 0)
                    {
                        nameUsed = name;
                        return dict;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"XAuthRepository: app package probe '{name}' failed: {ex.Message}");
                }
            }

            return dict;
        }

        private static IDictionary<string, string> LoadEnvFromEmbeddedResource(out string nameUsed)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            nameUsed = string.Empty;

            try
            {
                var asm = typeof(XAuthRepository).Assembly;
                var resourceName = asm.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("XApi.env", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(resourceName))
                {
                    using var stream = asm.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        ParseEnv(reader.ReadToEnd(), dict);
                        nameUsed = resourceName;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"XAuthRepository: embedded resource probe failed: {ex.Message}");
            }

            return dict;
        }

        private static void ParseEnv(string content, IDictionary<string, string> dict)
        {
            using var sr = new StringReader(content);
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;

                var idx = line.IndexOf('=', StringComparison.Ordinal);
                if (idx <= 0) continue;

                var key = line[..idx].Trim();
                var value = line[(idx + 1)..].Trim();

                if (value.Length >= 2 && value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                dict[key] = value;
            }
        }

        public XAuthSettings GetSettings() => _settings;

        public Task<string?> GetStoredAccessTokenAsync() => SecureStorage.GetAsync("x_access_token");
        public Task<string?> GetStoredRefreshTokenAsync() => SecureStorage.GetAsync("x_refresh_token");
        public Task<string?> GetAuthenticatedHandleAsync() => SecureStorage.GetAsync("x_handle");
        public Task<string?> GetStoredOAuth1TokenAsync() => SecureStorage.GetAsync("x_oauth_token");
        public Task<string?> GetStoredOAuth1SecretAsync() => SecureStorage.GetAsync("x_oauth_token_secret");
        public Task<string?> GetPkceVerifierAsync() => SecureStorage.GetAsync("x_pkce_verifier");

        public async Task SaveAccessTokenAsync(string token) => await SecureStorage.SetAsync("x_access_token", token);
        public async Task SaveRefreshTokenAsync(string token) => await SecureStorage.SetAsync("x_refresh_token", token);
        public async Task SaveScopeAsync(string scope) => await SecureStorage.SetAsync("x_scope", scope);

        public async Task SaveClientInfoAsync(string clientId, string redirectUri)
        {
            await SecureStorage.SetAsync("x_client_id", clientId);
            await SecureStorage.SetAsync("x_redirect_uri", redirectUri);
        }

        public async Task SavePkceVerifierAsync(string verifier) => await SecureStorage.SetAsync("x_pkce_verifier", verifier);

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