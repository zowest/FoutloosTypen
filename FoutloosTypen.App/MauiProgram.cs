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

            // Services
            builder.Services.AddSingleton<ILessonService, LessonService>();
            builder.Services.AddSingleton<ICourseService, CourseService>();
            builder.Services.AddSingleton<IAssignmentService, AssignmentService>();
            builder.Services.AddSingleton<IPracticeMaterialService, PracticeMaterialService>();

            // ViewModels
            builder.Services.AddTransient<LessonViewModel>();
            builder.Services.AddTransient<CoursesViewModel>();
            builder.Services.AddTransient<LearnpathViewModel>();
            builder.Services.AddTransient<LessonView>();
            builder.Services.AddTransient<AssignmentViewModel>();
            builder.Services.AddTransient<AssignmentView>();
#if WINDOWS
            builder.ConfigureLifecycleEvents(events =>
            {
                // Make sure to add "using Microsoft.Maui.LifecycleEvents;" in the top of the file 
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
