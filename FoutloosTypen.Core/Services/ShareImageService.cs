using SkiaSharp;
using FoutloosTypen.Core.Interfaces.Services;

namespace FoutloosTypen.Core.Services
{
    public class ShareImageService : IShareImageService
    {
        public string BuildTweetText(string lessonName, string progressText)
            => $"{lessonName} - {progressText} #FoutloosTypen";

        public SKBitmap Generate(string lessonName, string progressText, System.IO.Stream? logoStream = null)
        {
            var bitmap = new SKBitmap(1080, 1080);
            using var canvas = new SKCanvas(bitmap);

            // Gradient achtergrond (wit naar geel)
            using var gradientPaint = new SKPaint();
            gradientPaint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(0, bitmap.Height),
                new[] { SKColors.White, SKColor.Parse("#FFD700") },
                new[] { 0f, 1f },
                SKShaderTileMode.Clamp);
            canvas.DrawRect(0, 0, bitmap.Width, bitmap.Height, gradientPaint);

            // Border
            using var borderPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#4A90E2"),
                StrokeWidth = 8,
                IsAntialias = true
            };
            canvas.DrawRect(15, 15, bitmap.Width - 30, bitmap.Height - 30, borderPaint);

            float margin = 60f;
            float currentY = margin;

            // Header sectie met logo en "Boltype"
            using var headerPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 64,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            // Load and draw the actual logo if stream is provided
            SKBitmap? logoImage = null;
            if (logoStream != null)
            {
                try
                {
                    logoImage = SKBitmap.Decode(logoStream);
                }
                catch
                {
                    // Will use fallback
                }
            }

            if (logoImage != null)
            {
                using (logoImage)
                {
                    // Draw logo (80x80)
                    var destRect = new SKRect(margin, currentY, margin + 80, currentY + 80);
                    canvas.DrawBitmap(logoImage, destRect, new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High });
                }
            }
            else
            {
                // Fallback to placeholder if logo can't be loaded
                using var logoBackPaint = new SKPaint { Color = SKColor.Parse("#FFD700"), IsAntialias = true };
                canvas.DrawRoundRect(margin, currentY, 80, 80, 8, 8, logoBackPaint);
                
                using var logoTextPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 36,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
                    TextAlign = SKTextAlign.Center
                };
                canvas.DrawText("FT", margin + 40, currentY + 52, logoTextPaint);
            }

            // "Boltype" tekst naast logo
            canvas.DrawText("Boltype", margin + 100, currentY + 60, headerPaint);

            // Datum rechts uitgelijnd
            using var datePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 32,
                IsAntialias = true,
                TextAlign = SKTextAlign.Right
            };
            var datum = System.DateTime.Now.ToString("dd/MM/yyyy");
            canvas.DrawText(datum, bitmap.Width - margin, currentY + 60, datePaint);

            currentY += 150;

            // "Les" titel
            using var titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 96,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText("Les", margin, currentY, titlePaint);

            currentY += 80;

            // Lijn onder "Les"
            using var linePaint = new SKPaint
            {
                Color = SKColor.Parse("#4A90E2"),
                StrokeWidth = 3,
                IsAntialias = true
            };
            canvas.DrawLine(margin, currentY, bitmap.Width - margin, currentY, linePaint);

            currentY += 60;

            // Les naam
            using var lessonPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 52,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText(lessonName, margin, currentY, lessonPaint);

            currentY += 80;

            // Progress text
            using var progressPaint = new SKPaint
            {
                Color = SKColor.Parse("#333333"),
                TextSize = 44,
                IsAntialias = true
            };
            canvas.DrawText(progressText, margin, currentY, progressPaint);

            // Footer (onderaan)
            using var footerPaint = new SKPaint
            {
                Color = SKColor.Parse("#666666"),
                TextSize = 28,
                IsAntialias = true
            };
            canvas.DrawText("Deel je voortgang! #FoutloosTypen", margin, bitmap.Height - margin + 10, footerPaint);

            return bitmap;
        }
    }
}
