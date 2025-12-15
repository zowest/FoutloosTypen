using System.Collections.Generic;
using SkiaSharp;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Storage;

namespace FoutloosTypen.Services
{
    public interface IShareImageService
    {
        Task ShareLessonSummaryImageAsync(string lessonName, string progressText);
        Task<string> SaveLessonSummaryImageAsync(string lessonName, string progressText);
        Task ShareToTwitterAsync(string lessonName, string progressText);
        Task<string?> SaveWithPickerAsync(string lessonName, string progressText);
    }

    public class ShareImageService : IShareImageService
    {
        private readonly IFileSaver _fileSaver;

        public ShareImageService()
        {
            _fileSaver = FileSaver.Default;
        }

        public async Task ShareLessonSummaryImageAsync(string lessonName, string progressText)
        {
            var filePath = await GenerateImageAsync(lessonName, progressText);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Deel les samenvatting",
                File = new ShareFile(filePath)
            });
        }

        public async Task<string> SaveLessonSummaryImageAsync(string lessonName, string progressText)
        {
            var filePath = await GenerateImageAsync(lessonName, progressText);

            // Copy from cache to app data directory as a persistent save
            var downloadsDir = FileSystem.AppDataDirectory;
            var targetPath = Path.Combine(downloadsDir, Path.GetFileName(filePath));
            File.Copy(filePath, targetPath, overwrite: true);
            return targetPath;
        }

        public async Task ShareToTwitterAsync(string lessonName, string progressText)
        {
            var imagePath = await GenerateImageAsync(lessonName, progressText);

            var status = $"{lessonName} - {progressText} #FoutloosTypen #TypingPractice";
            await Clipboard.SetTextAsync(status);

            var request = new ShareFileRequest
            {
                Title = "Deel op X",
                File = new ShareFile(imagePath)
            };

            await Share.RequestAsync(request);
        }

        public async Task<string?> SaveWithPickerAsync(string lessonName, string progressText)
        {
            var filePath = await GenerateImageAsync(lessonName, progressText);

            await using var fileStream = File.OpenRead(filePath);
            var result = await _fileSaver.SaveAsync(
                Path.GetFileName(filePath),
                fileStream,
                CancellationToken.None);

            return result.IsSuccessful ? result.FilePath : null;
        }

        private async Task<string> GenerateImageAsync(string lessonName, string progressText)
        {
            using var bitmap = new SKBitmap(1080, 1080);
            using var canvas = new SKCanvas(bitmap);

            canvas.Clear(SKColors.White);

            using var paintTitle = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 72,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            using var paintSub = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 48,
                IsAntialias = true
            };

            var margin = 80f;
            canvas.DrawText("FoutloosTypen", margin, margin + 40, paintTitle);
            canvas.DrawText($"Les: {lessonName}", margin, margin + 140, paintSub);
            canvas.DrawText(progressText, margin, margin + 220, paintSub);

            using var linePaint = new SKPaint { Color = SKColors.LightGray, StrokeWidth = 4 };
            canvas.DrawLine(margin, margin + 260, bitmap.Width - margin, margin + 260, linePaint);

            using var footerPaint = new SKPaint { Color = SKColors.DarkSlateGray, TextSize = 36, IsAntialias = true };
            canvas.DrawText("Gegenereerd met SkiaSharp", margin, bitmap.Height - margin, footerPaint);

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 90);

            var fileName = $"les_resultaat_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            using (var stream = File.OpenWrite(filePath))
            {
                data.SaveTo(stream);
            }

            return filePath;
        }
    }
}
