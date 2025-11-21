using Microsoft.Extensions.Logging;
using FoutloosTypen.ViewModels;
using FoutloosTypen.Views;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Services;
using Microsoft.Maui;


namespace FoutloosTypen
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register your services and viewmodels for dependency injection
            builder.Services.AddTransient<LessonViewModel>();
            builder.Services.AddTransient<CourseViewModel>();
            builder.Services.AddSingleton<ILessonService, LessonService>();
            builder.Services.AddSingleton<ICourseService, CourseService>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
