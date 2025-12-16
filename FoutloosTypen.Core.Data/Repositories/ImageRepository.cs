using FoutloosTypen.Core.Interfaces.Repositories;
using System.Threading.Tasks;
using System;
using System.IO;
using SkiaSharp;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class ImageRepository : IImageRepository
    {
        public ImageRepository()
        {
        }

        // Repository now only produces plain white image bytes. UI is responsible for layout and rendering text.
        public Task<byte[]> GenerateLessonSummaryBytesAsync(string lessonName, string progressText)
        {
            using var bitmap = new SKBitmap(1080, 1080);
            using var canvas = new SKCanvas(bitmap);

            canvas.Clear(SKColors.White);

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 90);

            using var ms = new MemoryStream();
            data.SaveTo(ms);
            return Task.FromResult(ms.ToArray());
        }

        public async Task<string> SaveLessonSummaryImageAsync(string lessonName, string progressText)
        {
            var bytes = await GenerateLessonSummaryBytesAsync(lessonName, progressText);
            var fileName = $"les_resultaat_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
            var filePath = Path.Combine(System.IO.Path.GetTempPath(), fileName);
            await File.WriteAllBytesAsync(filePath, bytes);
            return filePath;
        }

        public Task<string?> SaveWithPickerAsync(string lessonName, string progressText)
        {
            // Picker UI is a platform concern; repository returns saved path via SaveLessonSummaryImageAsync if needed.
            return Task.FromResult<string?>(null);
        }
    }
}
