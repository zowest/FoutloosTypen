using FoutloosTypen.Core.Data.Helpers;
using FoutloosTypen.Core.Data.Repositories;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Services;
using FoutloosTypen.ViewModels;
using FoutloosTypen.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using System.Diagnostics;
using CommunityToolkit.Maui;
using Microsoft.Maui.Devices;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.Json;
using Microsoft.Maui.Storage;

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
            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }).UseMauiCommunityToolkit();

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
            builder.Services.AddSingleton<IXAuthService, XAuthService>();
            builder.Services.AddSingleton<IXMediaUploadService,MediaUploadService>();

            builder.Services.AddSingleton<IShareUiService,ShareUiService>();
            builder.Services.AddSingleton<IShareImageService,ShareImageService>();
            builder.Services.AddSingleton<ISocialShareService,SocialShareService>();

            // Load configuration for XAuthSettings from appsettings.Development.json if available
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);

            // If not found in base dir, try relative path to Core.Data Resources (useful in dev)
            var cfg = configBuilder.Build();
            if (!cfg.GetSection("X").Exists())
            {
                var altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "FoutloosTypen.Core.Data", "Resources", "appsettings.Development.json");
                if (File.Exists(altPath))
                {
                    configBuilder = new ConfigurationBuilder().AddJsonFile(altPath, optional: true, reloadOnChange: false);
                    cfg = configBuilder.Build();
                }
            }

            var xSection = cfg.GetSection("X");
            var xSettings = xSection.Get<FoutloosTypen.Core.XAuthSettings>();

            // Fallback: try reading appsettings.Development.json from app package (useful on mobile platforms)
            if (xSettings == null)
            {
                try
                {
                    using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.Development.json").GetAwaiter().GetResult();
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("X", out var xElem))
                    {
                        xSettings = JsonSerializer.Deserialize<FoutloosTypen.Core.XAuthSettings>(xElem.GetRawText());
                    }
                }
                catch
                {
                    // ignore; we'll fall back to defaults
                }
            }

            xSettings ??= new FoutloosTypen.Core.XAuthSettings();
            builder.Services.AddSingleton<FoutloosTypen.Core.XAuthSettings>(_ => xSettings);

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
                                if (Application.Current?.MainPage != null)
                                {
                                    var result = await Application.Current.MainPage.DisplayAlert(
                                        "Close BolType",
                                        "Are you sure you want to close BolType?",
                                        "Yes",
                                        "No");

                                    if (result)
                                    {
                                        Application.Current?.Quit();
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