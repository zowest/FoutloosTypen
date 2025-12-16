using CommunityToolkit.Maui;
using FoutloosTypen.Core.Data.Helpers;
using FoutloosTypen.Core.Data.Repositories;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Services;
using FoutloosTypen.Services;
using FoutloosTypen.ViewModels;
using FoutloosTypen.Views;
using Grocery.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using System.Diagnostics;
using System.IO;
using Microsoft.Maui.Storage;
using System.Text.Json;
using System.Reflection;

#if WINDOWS
using Windows.System;
#endif

namespace FoutloosTypen
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
#if DEBUG
            DebugDatabaseReset.Reset();
#endif

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Repositories
            builder.Services.AddSingleton<ILessonRepository, LessonRepository>();
            builder.Services.AddSingleton<ICourseRepository, CourseRepository>();
            builder.Services.AddSingleton<IAssignmentRepository, AssignmentRepository>();
            builder.Services.AddSingleton<IPracticeMaterialRepository, PracticeMaterialRepository>();
            builder.Services.AddSingleton<IStudentRepository, StudentRepository>();

            // Services
            builder.Services.AddSingleton<ILessonService, LessonService>();
            builder.Services.AddSingleton<ICourseService, CourseService>();
            builder.Services.AddSingleton<IAssignmentService, AssignmentService>();
            builder.Services.AddSingleton<IPracticeMaterialService, PracticeMaterialService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IStudentService, StudentService>();
            builder.Services.AddSingleton<ITimerService, TimerService>();

            // OAuth for X
            builder.Services.AddSingleton<IXAuthService, XAuthService>();
            // Media upload (OAuth1) service
            builder.Services.AddSingleton<IXMediaUploadService, MediaUploadService>();

            // Load configuration from appsettings*.json + environment variables (no hardcoded secrets)
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
            // Use AppDomain.CurrentDomain.BaseDirectory so files copied to output (appsettings*.json) are found on all platforms
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            var configuration = configBuilder.Build();

            // Try to get X settings from configuration; if missing, try to load appsettings files from app package
            string clientId = configuration["X:ClientId"] ?? string.Empty;
            string redirectUri = configuration["X:RedirectUri"] ?? string.Empty;
            string[] scopes = configuration.GetSection("X:Scopes").Get<string[]>() ?? new[] { "tweet.write", "users.read", "media.write", "offline.access" };
            string consumerKey = configuration["X:ConsumerKey"] ?? string.Empty;
            string consumerSecret = configuration["X:ConsumerSecret"] ?? string.Empty;
            string oauthToken = configuration["X:OAuthToken"] ?? string.Empty;
            string oauthTokenSecret = configuration["X:OAuthTokenSecret"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri))
            {
                try
                {
                    Task.Run(async () =>
                    {
                        foreach (var filename in new[] { $"appsettings.{environment}.json", "appsettings.json" })
                        {
                            try
                            {
                                using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
                                using var reader = new StreamReader(stream);
                                var json = await reader.ReadToEndAsync();
                                using var doc = JsonDocument.Parse(json);
                                if (doc.RootElement.TryGetProperty("X", out var xEl))
                                {
                                    if (string.IsNullOrWhiteSpace(clientId) && xEl.TryGetProperty("ClientId", out var cEl))
                                        clientId = cEl.GetString() ?? clientId;
                                    if (string.IsNullOrWhiteSpace(redirectUri) && xEl.TryGetProperty("RedirectUri", out var rEl))
                                        redirectUri = rEl.GetString() ?? redirectUri;
                                    if (xEl.TryGetProperty("Scopes", out var sEl) && sEl.ValueKind == JsonValueKind.Array)
                                    {
                                        var list = new List<string>();
                                        foreach (var item in sEl.EnumerateArray()) if (item.ValueKind == JsonValueKind.String) list.Add(item.GetString() ?? string.Empty);
                                        if (list.Any()) scopes = list.ToArray();
                                    }
                                    if (xEl.TryGetProperty("ConsumerKey", out var ck)) consumerKey = ck.GetString() ?? consumerKey;
                                    if (xEl.TryGetProperty("ConsumerSecret", out var cs)) consumerSecret = cs.GetString() ?? consumerSecret;
                                    if (xEl.TryGetProperty("OAuthToken", out var ot)) oauthToken = ot.GetString() ?? oauthToken;
                                    if (xEl.TryGetProperty("OAuthTokenSecret", out var ots)) oauthTokenSecret = ots.GetString() ?? oauthTokenSecret;

                                    // stop after first successful file read
                                    return;
                                }
                            }
                            catch (FileNotFoundException)
                            {
                                // try next
                            }
                            catch
                            {
                                // ignore and try next
                            }
                        }

                        // If still missing, try reading embedded resources from Core.Data assembly
                        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri))
                        {
                            try
                            {
                                var coreDataAssembly = typeof(FoutloosTypen.Core.Data.Helpers.ConnectionHelper).Assembly;
                                var names = coreDataAssembly.GetManifestResourceNames();
#if DEBUG
                                Debug.WriteLine("Core.Data embedded resources: " + string.Join(",", names));
#endif
                                var resourceName = names.FirstOrDefault(n => n.EndsWith($"appsettings.{environment}.json", StringComparison.OrdinalIgnoreCase))
                                    ?? names.FirstOrDefault(n => n.EndsWith("appsettings.json", StringComparison.OrdinalIgnoreCase));

                                if (!string.IsNullOrEmpty(resourceName))
                                {
                                    using var resStream = coreDataAssembly.GetManifestResourceStream(resourceName)!;
                                    using var reader = new StreamReader(resStream);
                                    var json = await reader.ReadToEndAsync();
                                    using var doc = JsonDocument.Parse(json);
                                    if (doc.RootElement.TryGetProperty("X", out var xEl))
                                    {
                                        if (string.IsNullOrWhiteSpace(clientId) && xEl.TryGetProperty("ClientId", out var cEl))
                                            clientId = cEl.GetString() ?? clientId;
                                        if (string.IsNullOrWhiteSpace(redirectUri) && xEl.TryGetProperty("RedirectUri", out var rEl))
                                            redirectUri = rEl.GetString() ?? redirectUri;
                                        if (xEl.TryGetProperty("Scopes", out var sEl) && sEl.ValueKind == JsonValueKind.Array)
                                        {
                                            var list = new List<string>();
                                            foreach (var item in sEl.EnumerateArray()) if (item.ValueKind == JsonValueKind.String) list.Add(item.GetString() ?? string.Empty);
                                            if (list.Any()) scopes = list.ToArray();
                                        }
                                        if (xEl.TryGetProperty("ConsumerKey", out var ck)) consumerKey = ck.GetString() ?? consumerKey;
                                        if (xEl.TryGetProperty("ConsumerSecret", out var cs)) consumerSecret = cs.GetString() ?? consumerSecret;
                                        if (xEl.TryGetProperty("OAuthToken", out var ot)) oauthToken = ot.GetString() ?? oauthToken;
                                        if (xEl.TryGetProperty("OAuthTokenSecret", out var ots)) oauthTokenSecret = ots.GetString() ?? oauthTokenSecret;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
#if DEBUG
                                Debug.WriteLine($"Failed to load X settings from Core.Data resources: {ex.Message}");
#endif
                            }
                        }

                    }).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine($"Failed to load X settings from app package files: {ex.Message}");
#endif
                }
            }

            var xSettings = new XAuthSettings
            {
                ClientId = clientId,
                RedirectUri = redirectUri,
                Scopes = scopes,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                OAuthToken = oauthToken,
                OAuthTokenSecret = oauthTokenSecret
            };

            // Register settings instance
            builder.Services.AddSingleton(xSettings);

#if DEBUG
            Debug.WriteLine("XAuthSettings loaded. ConsumerKey set: " + !string.IsNullOrEmpty(xSettings.ConsumerKey));
            Debug.WriteLine("XAuthSettings loaded. ClientId set: " + !string.IsNullOrEmpty(xSettings.ClientId));
            Debug.WriteLine("XAuthSettings loaded. RedirectUri set: " + !string.IsNullOrEmpty(xSettings.RedirectUri));
#endif

            // Image sharing and social sharing
            builder.Services.AddSingleton<IShareImageService, ShareImageService>();
            builder.Services.AddSingleton<ISocialShareService>(sp =>
                new SocialShareService(
                    sp.GetRequiredService<IShareImageService>(),
                    sp.GetRequiredService<IXAuthService>(),
                    sp.GetRequiredService<XAuthSettings>(),
                    sp.GetRequiredService<IXMediaUploadService>()
                ));

            // ViewModels
            builder.Services.AddTransient<LessonViewModel>();
            builder.Services.AddTransient<CoursesViewModel>();
            builder.Services.AddTransient<LearnpathViewModel>();
            builder.Services.AddTransient<LessonView>();
            builder.Services.AddTransient<AssignmentViewModel>();
            builder.Services.AddTransient<AssignmentView>();
            builder.Services.AddSingleton<GlobalViewModel>();
            builder.Services.AddTransient<LoginView>().AddTransient<LoginViewModel>();
#if WINDOWS
            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(windowsLifecycleBuilder =>
                {
                    windowsLifecycleBuilder.OnWindowCreated(window =>
                    {
                        window.ExtendsContentIntoTitleBar = false;
                        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                        var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
                        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
                        switch (appWindow.Presenter)
                        {
                            case Microsoft.UI.Windowing.OverlappedPresenter overlappedPresenter:
                                overlappedPresenter.SetBorderAndTitleBar(false, false);
                                overlappedPresenter.Maximize();
                                break;
                        }

                        window.Content.KeyDown += async (sender, args) =>
                        {
                            if (args.Key == VirtualKey.Escape)
                            {
                                if (Microsoft.Maui.Controls.Application.Current?.MainPage != null)
                                {
                                    var result = await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                                        "Close BolType",
                                        "Are you sure you want to close BolType?",
                                        "Yes",
                                        "No");

                                    if (result)
                                    {
                                        Microsoft.Maui.Controls.Application.Current?.Quit();
                                    }
                                }
                            }
                        };
                    });
                });
            });
#endif
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}