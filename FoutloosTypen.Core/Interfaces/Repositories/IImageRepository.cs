using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IImageRepository
    {
        Task<string> GenerateLessonSummaryImageAsync(string lessonName, string progressText);
        Task ShareLessonSummaryImageAsync(string lessonName, string progressText);
        Task<string> SaveLessonSummaryImageAsync(string lessonName, string progressText);
        Task<string?> SaveWithPickerAsync(string lessonName, string progressText);
    }
}
