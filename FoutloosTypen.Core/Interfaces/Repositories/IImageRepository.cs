using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IImageRepository
    {
        Task<byte[]> GenerateLessonSummaryBytesAsync(string lessonName, string progressText);
        Task<string> SaveLessonSummaryImageAsync(string lessonName, string progressText);
        Task<string?> SaveWithPickerAsync(string lessonName, string progressText);
        // Sharing is a UI concern; repository does not perform platform share operations
    }
}
