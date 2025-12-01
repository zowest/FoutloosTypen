using Microsoft.Extensions.Logging;
using FoutloosTypen.ViewModels;
using FoutloosTypen.Views;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Services;
using FoutloosTypen.Core.Interfaces.Repositories; 
using FoutloosTypen.Core.Data.Repositories;
using System.Diagnostics;

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
            builder.Services.AddTransient<LessonView>();
            builder.Services.AddTransient<AssignmentViewModel>();
            builder.Services.AddTransient<AssignmentView>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
