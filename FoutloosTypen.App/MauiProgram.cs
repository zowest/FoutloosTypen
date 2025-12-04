using FoutloosTypen.Core.Data.Helpers;
using FoutloosTypen.Core.Data.Repositories;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Services;
using FoutloosTypen.ViewModels;
using FoutloosTypen.Views;
using Grocery.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using System.Diagnostics;

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