using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using FoutloosTypen.Core.Interfaces.Services;

namespace FoutloosTypen.Core.Services
{
    public class ShareImageService : IShareImageService
    {
        public string BuildTweetText(string lessonName, string progressText)
        {
            return $"{lessonName} - {progressText} #FoutloosTypen";
        }

        public SKBitmap Generate(string lessonName, string progressText)
        {
            var bitmap = new SKBitmap(1080, 1080);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);

            using var paintTitle = new SKPaint { Color = SKColors.Black, TextSize = 72, IsAntialias = true, Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold) };
            using var paintSub = new SKPaint { Color = SKColors.Gray, TextSize = 48, IsAntialias = true };
            using var linePaint = new SKPaint { Color = SKColors.LightGray, StrokeWidth = 4 };
            using var footerPaint = new SKPaint { Color = SKColors.DarkSlateGray, TextSize = 36, IsAntialias = true };

            float margin = 80f;
            canvas.DrawText("FoutloosTypen", margin, margin + 40, paintTitle);
            canvas.DrawText($"Les: {lessonName}", margin, margin + 140, paintSub);
            canvas.DrawText(progressText, margin, margin + 220, paintSub);
            canvas.DrawLine(margin, margin + 260, bitmap.Width - margin, margin + 260, linePaint);
            canvas.DrawText("Gegenereerd met SkiaSharp", margin, bitmap.Height - margin, footerPaint);

            return bitmap;
        }

        public async Task<string> SaveToCacheAsync(SKBitmap bitmap, string extension = "png")
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(extension.Equals("jpg", StringComparison.OrdinalIgnoreCase) ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png, 90);
            var dir = Path.GetTempPath();
            var filePath = Path.Combine(dir, $"les_resultaat_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{extension}");
            await File.WriteAllBytesAsync(filePath, data.ToArray());
            return filePath;
        }
    }
}
