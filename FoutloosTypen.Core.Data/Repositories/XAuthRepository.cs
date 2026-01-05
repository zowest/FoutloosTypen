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

            string consumerKey = ReadValue(env, "CONSUMER_KEY", Environment.GetEnvironmentVariable("CONSUMER_KEY"));
            string consumerSecret = ReadValue(env, "CONSUMER_SECRET", Environment.GetEnvironmentVariable("CONSUMER_SECRET"));
            string callbackUrl = ReadValue(env, "CALLBACK_URL", Environment.GetEnvironmentVariable("CALLBACK_URL"));

            var settings = new XAuthSettings
            {
                ConsumerKey = consumerKey ?? string.Empty,
                ConsumerSecret = consumerSecret ?? string.Empty,
                CallbackUrl = callbackUrl ?? string.Empty
            };

            Debug.WriteLine($"XAuthRepository: config source: {source}");
            Debug.WriteLine($"XAuthRepository: consumerKey set: {!string.IsNullOrWhiteSpace(settings.ConsumerKey)}, consumerSecret set: {!string.IsNullOrWhiteSpace(settings.ConsumerSecret)}");

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

        public Task<string?> GetAuthenticatedHandleAsync() => SecureStorage.GetAsync("x_handle");

        public async Task ClearAllTokensAsync()
        {
            try
            {
                SecureStorage.Remove("x_handle");
                
                // Clear device-level OAuth tokens
                SecureStorage.Remove("x_oauth_token_0");
                SecureStorage.Remove("x_oauth_token_secret_0");
                SecureStorage.Remove("x_user_id_0");
                SecureStorage.Remove("x_username_0");
                
                Debug.WriteLine("[XAuthRepository] Cleared all tokens");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[XAuthRepository] FAILED to clear tokens: {ex.GetType().Name}: {ex.Message}");
            }
            
            await Task.CompletedTask;
        }

        public async Task SaveOAuth1TokensAsync(int ownerUserId, string accessToken, string accessSecret)
        {
            try
            {
                await SecureStorage.SetAsync($"x_oauth_token_{ownerUserId}", accessToken);
                await SecureStorage.SetAsync($"x_oauth_token_secret_{ownerUserId}", accessSecret);
                Debug.WriteLine($"[XAuthRepository] Saved OAuth1 tokens for ownerUserId={ownerUserId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[XAuthRepository] FAILED to save OAuth1 tokens: {ex.GetType().Name}: {ex.Message}");
                throw new InvalidOperationException("Kon OAuth tokens niet opslaan in SecureStorage. Controleer apparaat beveiliging.", ex);
            }
        }

        public async Task<(string AccessToken, string AccessSecret)?> GetOAuth1TokensAsync(int ownerUserId)
        {
            try
            {
                var token = await SecureStorage.GetAsync($"x_oauth_token_{ownerUserId}");
                var secret = await SecureStorage.GetAsync($"x_oauth_token_secret_{ownerUserId}");
                
                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(secret))
                {
                    Debug.WriteLine($"[XAuthRepository] No OAuth1 tokens found for ownerUserId={ownerUserId}");
                    return null;
                }
                
                Debug.WriteLine($"[XAuthRepository] Retrieved OAuth1 tokens for ownerUserId={ownerUserId}");
                return (token, secret);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[XAuthRepository] FAILED to retrieve OAuth1 tokens: {ex.GetType().Name}: {ex.Message}");
                return null; // Behandel als "geen tokens" in plaats van crash
            }
        }

        public Task DeleteOAuth1TokensAsync(int ownerUserId)
        {
            try
            {
                SecureStorage.Remove($"x_oauth_token_{ownerUserId}");
                SecureStorage.Remove($"x_oauth_token_secret_{ownerUserId}");
                Debug.WriteLine($"[XAuthRepository] Deleted OAuth1 tokens for ownerUserId={ownerUserId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[XAuthRepository] FAILED to delete OAuth1 tokens: {ex.GetType().Name}: {ex.Message}");
            }
            
            return Task.CompletedTask;
        }

        public async Task SaveXUserInfoAsync(int ownerUserId, string xUserId, string xUsername)
        {
            try
            {
                await SecureStorage.SetAsync($"x_user_id_{ownerUserId}", xUserId);
                await SecureStorage.SetAsync($"x_username_{ownerUserId}", xUsername);
                Debug.WriteLine($"[XAuthRepository] Saved X user info for ownerUserId={ownerUserId}: X user={xUserId}, username={xUsername}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[XAuthRepository] FAILED to save X user info: {ex.GetType().Name}: {ex.Message}");
                throw new InvalidOperationException("Kon X gebruikersinformatie niet opslaan in SecureStorage.", ex);
            }
        }

        public async Task<(string XUserId, string XUsername)?> GetXUserInfoAsync(int ownerUserId)
        {
            try
            {
                var xUserId = await SecureStorage.GetAsync($"x_user_id_{ownerUserId}");
                var xUsername = await SecureStorage.GetAsync($"x_username_{ownerUserId}");
                
                if (string.IsNullOrWhiteSpace(xUserId) || string.IsNullOrWhiteSpace(xUsername))
                {
                    Debug.WriteLine($"[XAuthRepository] No X user info found for ownerUserId={ownerUserId}");
                    return null;
                }
                
                Debug.WriteLine($"[XAuthRepository] Retrieved X user info for ownerUserId={ownerUserId}: X user={xUserId}, username={xUsername}");
                return (xUserId, xUsername);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[XAuthRepository] FAILED to retrieve X user info: {ex.GetType().Name}: {ex.Message}");
                return null;
            }
        }

        public Task DeleteXUserInfoAsync(int ownerUserId)
        {
            try
            {
                SecureStorage.Remove($"x_user_id_{ownerUserId}");
                SecureStorage.Remove($"x_username_{ownerUserId}");
                Debug.WriteLine($"[XAuthRepository] Deleted X user info for ownerUserId={ownerUserId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[XAuthRepository] FAILED to delete X user info: {ex.GetType().Name}: {ex.Message}");
            }
            
            return Task.CompletedTask;
        }
    }
}