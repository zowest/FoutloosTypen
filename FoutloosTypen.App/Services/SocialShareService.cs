using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace FoutloosTypen.Services
{
    public interface ISocialShareService
    {
        Task ShareToTwitterAsync(string lessonName, string progressText);
        Task ShareGenericAsync(string lessonName, string progressText);
        Task<string?> SaveWithPickerAsync(string lessonName, string progressText);
    }

    public class SocialShareService : ISocialShareService
    {
        private readonly IShareImageService _shareImageService;
        private readonly IFileSaver _fileSaver;

        public SocialShareService(IShareImageService shareImageService)
        {
            _shareImageService = shareImageService;
            _fileSaver = FileSaver.Default;
        }

        public async Task ShareToTwitterAsync(string lessonName, string progressText)
        {
            // Use system share; user chooses X/Twitter if available
            await _shareImageService.ShareToTwitterAsync(lessonName, progressText);
        }

        public async Task ShareGenericAsync(string lessonName, string progressText)
        {
            await _shareImageService.ShareLessonSummaryImageAsync(lessonName, progressText);
        }

        public async Task<string?> SaveWithPickerAsync(string lessonName, string progressText)
        {
            // Generate image via ShareImageService, then save with FileSaver
            var tempPath = await _shareImageService.SaveLessonSummaryImageAsync(lessonName, progressText);
            await using var fileStream = File.OpenRead(tempPath);

            var result = await _fileSaver.SaveAsync(
                Path.GetFileName(tempPath),
                fileStream,
                CancellationToken.None);

            return result.IsSuccessful ? result.FilePath : null;
        }
    }
}
