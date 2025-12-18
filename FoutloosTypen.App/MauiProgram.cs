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
            builder.Services.AddSingleton<ITtsService, TtsService>();
            builder.Services.AddSingleton<ISharePostRepository, SharePostRepository>();
            builder.Services.AddSingleton<IMediaUploadRepository, MediaUploadRepository>();
            builder.Services.AddSingleton<IXAuthRepository, XAuthRepository>();
            builder.Services.AddSingleton<IXAuthApiRepository, XAuthApiRepository>();
            builder.Services.AddSingleton<IImageRepository, ImageRepository>();
            builder.Services.AddSingleton<IShareImageRepository, ShareImageRepository>();

            // Domain Services
            builder.Services.AddSingleton<ILessonService, LessonService>();
            builder.Services.AddSingleton<ICourseService, CourseService>();
            builder.Services.AddSingleton<IAssignmentService, AssignmentService>();
            builder.Services.AddSingleton<IPracticeMaterialService, PracticeMaterialService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IStudentService, StudentService>();
            builder.Services.AddSingleton<ITimerService, TimerService>();
            builder.Services.AddSingleton<IXAuthService, XAuthService>();
            builder.Services.AddSingleton<IShareImageService, ShareImageService>();
            builder.Services.AddSingleton<ITypingComparisonService, TypingComparisonService>();
          
            builder.Services.AddSingleton<IResultService, ResultService>();
            builder.Services.AddSingleton<IAudioAssignmentService, AudioAssignmentService>();

            // Use XAuthRepository to provide XAuthSettings in DI
            builder.Services.AddSingleton<FoutloosTypen.Core.Models.XAuthSettings>(provider => provider.GetRequiredService<IXAuthRepository>().GetSettings());

            // ViewModels & Views
            builder.Services.AddTransient<LessonViewModel>();
            builder.Services.AddTransient<CoursesViewModel>();
            builder.Services.AddTransient<LearnpathViewModel>();
            builder.Services.AddTransient<LessonView>();
            builder.Services.AddSingleton<ShareViewModel>();
            builder.Services.AddTransient<AssignmentViewModel>();
            builder.Services.AddTransient<AssignmentView>();
            builder.Services.AddSingleton<GlobalViewModel>();
            builder.Services.AddTransient<LoginView>().AddTransient<LoginViewModel>();
            builder.Services.AddTransient<ProfileView>().AddTransient<ProfileViewModel>();
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

            var app = builder.Build();

            // Debug: log loaded XAuthSettings to confirm values at startup
            try
            {
                var settings = app.Services.GetRequiredService<FoutloosTypen.Core.Models.XAuthSettings>();
                Debug.WriteLine($"Startup: XAuthSettings.ClientId set: {!string.IsNullOrEmpty(settings.ClientId)}");
                Debug.WriteLine($"Startup: XAuthSettings.RedirectUri set: {!string.IsNullOrEmpty(settings.RedirectUri)}");
                Debug.WriteLine($"Startup: XAuthSettings.Scopes count: {settings.Scopes?.Length ?? 0}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Startup: failed to read XAuthSettings from DI: {ex.Message}");
            }

            return app;
        }
    }
}