using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using System.IO;
using System.Text.Json;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class XAuthRepository : IXAuthRepository
    {
        private readonly XAuthSettings _settings;

        public XAuthRepository()
        {
            XAuthSettings? settings = null;

            try
            {
                var openTask = FileSystem.OpenAppPackageFileAsync("appsettings.Development.json");
                if (openTask.IsCompletedSuccessfully)
                {
                    using var stream = openTask.GetAwaiter().GetResult();
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("X", out var xElem))
                    {
                        settings = JsonSerializer.Deserialize<XAuthSettings>(xElem.GetRawText());
                        Debug.WriteLine("XAuthRepository: loaded settings from app package");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"XAuthRepository: package read failed: {ex.Message}");
            }

            if (settings == null)
            {
                try
                {
                    var candidate = Path.Combine(AppContext.BaseDirectory, "appsettings.Development.json");
                    if (File.Exists(candidate))
                    {
                        var json = File.ReadAllText(candidate);
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("X", out var xElem))
                        {
                            settings = JsonSerializer.Deserialize<XAuthSettings>(xElem.GetRawText());
                            Debug.WriteLine($"XAuthRepository: loaded settings from {candidate}");
                        }
                    }
                    if (settings == null)
                    {
                        var outputResourceCandidate = Path.Combine(AppContext.BaseDirectory, "Resources", "appsettings.Development.json");
                        if (File.Exists(outputResourceCandidate))
                        {
                            var json = File.ReadAllText(outputResourceCandidate);
                            using var doc = JsonDocument.Parse(json);
                            if (doc.RootElement.TryGetProperty("X", out var xElem))
                            {
                                settings = JsonSerializer.Deserialize<XAuthSettings>(xElem.GetRawText());
                                Debug.WriteLine($"XAuthRepository: loaded settings from {outputResourceCandidate}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"XAuthRepository: base dir read failed: {ex.Message}");
                }
            }

            if (settings == null)
            {
                try
                {
                    var alt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "FoutloosTypen.Core.Data", "Resources", "appsettings.Development.json");
                    alt = Path.GetFullPath(alt);
                    if (File.Exists(alt))
                    {
                        var json = File.ReadAllText(alt);
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("X", out var xElem))
                        {
                            settings = JsonSerializer.Deserialize<XAuthSettings>(xElem.GetRawText());
                            Debug.WriteLine($"XAuthRepository: loaded settings from dev path {alt}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"XAuthRepository: fallback read failed: {ex.Message}");
                }
            }

            if (settings == null)
            {
                try
                {
                    var asm = Assembly.GetExecutingAssembly();
                    var names = asm.GetManifestResourceNames();
                    foreach (var name in names)
                    {
                        if (name.EndsWith("appsettings.Development.json", StringComparison.OrdinalIgnoreCase))
                        {
                            using var stream = asm.GetManifestResourceStream(name);
                            if (stream != null)
                            {
                                using var reader = new StreamReader(stream);
                                var json = reader.ReadToEnd();
                                using var doc = JsonDocument.Parse(json);
                                if (doc.RootElement.TryGetProperty("X", out var xElem))
                                {
                                    settings = JsonSerializer.Deserialize<XAuthSettings>(xElem.GetRawText());
                                    Debug.WriteLine($"XAuthRepository: loaded settings from embedded resource {name}");
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"XAuthRepository: embedded resource read failed: {ex.Message}");
                }
            }

            if (settings == null)
            {
                Debug.WriteLine("XAuthRepository: no settings found, using defaults");
                settings = new XAuthSettings();
            }

            settings.ClientId = (settings.ClientId ?? string.Empty).Trim();
            settings.RedirectUri = (settings.RedirectUri ?? string.Empty).Trim();

            Debug.WriteLine($"XAuthRepository: clientId set: {!string.IsNullOrEmpty(settings.ClientId)}, redirectUri set: {!string.IsNullOrEmpty(settings.RedirectUri)}");

            _settings = settings;
        }

        public XAuthSettings GetSettings() => _settings;

        public Task<string?> GetStoredAccessTokenAsync() 
            => SecureStorage.GetAsync("x_access_token");

        public Task<string?> GetStoredRefreshTokenAsync() 
            => SecureStorage.GetAsync("x_refresh_token");

        public Task<string?> GetAuthenticatedHandleAsync() 
            => SecureStorage.GetAsync("x_handle");

        public Task<string?> GetStoredOAuth1TokenAsync() 
            => SecureStorage.GetAsync("x_oauth_token");

        public Task<string?> GetStoredOAuth1SecretAsync() 
            => SecureStorage.GetAsync("x_oauth_token_secret");

        public Task<string?> GetPkceVerifierAsync() 
            => SecureStorage.GetAsync("x_pkce_verifier");

        public async Task SaveAccessTokenAsync(string token)
            => await SecureStorage.SetAsync("x_access_token", token);

        public async Task SaveRefreshTokenAsync(string token)
            => await SecureStorage.SetAsync("x_refresh_token", token);

        public async Task SaveScopeAsync(string scope)
            => await SecureStorage.SetAsync("x_scope", scope);

        public async Task SaveClientInfoAsync(string clientId, string redirectUri)
        {
            await SecureStorage.SetAsync("x_client_id", clientId);
            await SecureStorage.SetAsync("x_redirect_uri", redirectUri);
        }

        public async Task SavePkceVerifierAsync(string verifier)
            => await SecureStorage.SetAsync("x_pkce_verifier", verifier);

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
