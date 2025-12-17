using SkiaSharp;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Helpers
{
    public static class ShareImageGenerator
    {
        public static string BuildTweetText(string lessonName, string progressText)
            => $"{lessonName} - {progressText} #FoutloosTypen";

        public static SKBitmap Generate(string lessonName, string progressText, System.IO.Stream? logoStream = null)
        {
            var bitmap = new SKBitmap(1080, 1080);
            using var canvas = new SKCanvas(bitmap);

            DrawBackground(canvas, bitmap);
            DrawBorder(canvas, bitmap);

            float margin = 60f;
            float currentY = margin;

            currentY = DrawHeader(canvas, bitmap, margin, currentY, logoStream);
            currentY = DrawLessonSection(canvas, bitmap, margin, currentY, lessonName, progressText);
            DrawFooter(canvas, bitmap, margin);

            return bitmap;
        }

        public static SKBitmap GenerateWithResults(string lessonName, Result result, System.IO.Stream? logoStream = null)
        {
            var bitmap = new SKBitmap(1080, 1080);
            using var canvas = new SKCanvas(bitmap);

            DrawBackground(canvas, bitmap);
            DrawBorder(canvas, bitmap);

            float margin = 60f;
            float currentY = margin;

            currentY = DrawHeader(canvas, bitmap, margin, currentY, logoStream);
            currentY = DrawResultSection(canvas, bitmap, margin, currentY, lessonName, result);
            DrawFooter(canvas, bitmap, margin);

            return bitmap;
        }

        private static void DrawBackground(SKCanvas canvas, SKBitmap bitmap)
        {
            using var gradientPaint = new SKPaint();
            gradientPaint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(0, bitmap.Height),
                new[] { SKColors.White, SKColor.Parse("#FFD700") },
                new[] { 0f, 1f },
                SKShaderTileMode.Clamp);
            canvas.DrawRect(0, 0, bitmap.Width, bitmap.Height, gradientPaint);
        }

        private static void DrawBorder(SKCanvas canvas, SKBitmap bitmap)
        {
            using var borderPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#4A90E2"),
                StrokeWidth = 8,
                IsAntialias = true
            };
            canvas.DrawRect(15, 15, bitmap.Width - 30, bitmap.Height - 30, borderPaint);
        }

        private static float DrawHeader(SKCanvas canvas, SKBitmap bitmap, float margin, float currentY, System.IO.Stream? logoStream)
        {
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
                    var destRect = new SKRect(margin, currentY, margin + 80, currentY + 80);
                    canvas.DrawBitmap(logoImage, destRect, new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High });
                }
            }
            else
            {
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

            canvas.DrawText("Boltype", margin + 100, currentY + 60, headerPaint);

            using var datePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 32,
                IsAntialias = true,
                TextAlign = SKTextAlign.Right
            };
            var datum = System.DateTime.Now.ToString("dd/MM/yyyy");
            canvas.DrawText(datum, bitmap.Width - margin, currentY + 60, datePaint);

            return currentY + 150;
        }

        private static float DrawLessonSection(SKCanvas canvas, SKBitmap bitmap, float margin, float currentY, string lessonName, string progressText)
        {
            using var titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 96,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText("Les", margin, currentY, titlePaint);
            currentY += 80;

            using var linePaint = new SKPaint
            {
                Color = SKColor.Parse("#4A90E2"),
                StrokeWidth = 3,
                IsAntialias = true
            };
            canvas.DrawLine(margin, currentY, bitmap.Width - margin, currentY, linePaint);
            currentY += 60;

            using var lessonPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 52,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText(lessonName, margin, currentY, lessonPaint);
            currentY += 80;

            using var progressPaint = new SKPaint
            {
                Color = SKColor.Parse("#333333"),
                TextSize = 44,
                IsAntialias = true
            };
            canvas.DrawText(progressText, margin, currentY, progressPaint);

            return currentY;
        }

        private static float DrawResultSection(SKCanvas canvas, SKBitmap bitmap, float margin, float currentY, string lessonName, Result result)
        {
            using var titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 96,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText("Resultaat", margin, currentY, titlePaint);
            currentY += 80;

            using var linePaint = new SKPaint
            {
                Color = SKColor.Parse("#4A90E2"),
                StrokeWidth = 3,
                IsAntialias = true
            };
            canvas.DrawLine(margin, currentY, bitmap.Width - margin, currentY, linePaint);
            currentY += 60;

            using var lessonPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 52,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText(lessonName, margin, currentY, lessonPaint);
            currentY += 80;

            using var statLabelPaint = new SKPaint
            {
                Color = SKColor.Parse("#333333"),
                TextSize = 40,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            using var statValuePaint = new SKPaint
            {
                Color = SKColor.Parse("#4A90E2"),
                TextSize = 40,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
                TextAlign = SKTextAlign.Right
            };

            canvas.DrawText("Score:", margin, currentY, statLabelPaint);
            canvas.DrawText($"{result.Score}", bitmap.Width - margin, currentY, statValuePaint);
            currentY += 60;

            canvas.DrawText("Nauwkeurigheid:", margin, currentY, statLabelPaint);
            canvas.DrawText($"{result.AccuracyPercent:F1}%", bitmap.Width - margin, currentY, statValuePaint);
            currentY += 60;

            canvas.DrawText("Woorden/min:", margin, currentY, statLabelPaint);
            canvas.DrawText($"{result.WordsPerMinute}", bitmap.Width - margin, currentY, statValuePaint);
            currentY += 60;

            canvas.DrawText("Aanslagen/min:", margin, currentY, statLabelPaint);
            canvas.DrawText($"{result.StrokesPerMinute}", bitmap.Width - margin, currentY, statValuePaint);
            currentY += 60;

            canvas.DrawText("Fouten:", margin, currentY, statLabelPaint);
            canvas.DrawText($"{result.TotalMistakes}", bitmap.Width - margin, currentY, statValuePaint);
            currentY += 60;

            canvas.DrawText("Zinnen voltooid:", margin, currentY, statLabelPaint);
            canvas.DrawText($"{result.SentencesCompleted}", bitmap.Width - margin, currentY, statValuePaint);

            return currentY;
        }

        private static void DrawFooter(SKCanvas canvas, SKBitmap bitmap, float margin)
        {
            using var footerPaint = new SKPaint
            {
                Color = SKColor.Parse("#666666"),
                TextSize = 28,
                IsAntialias = true
            };
            canvas.DrawText("Deel je voortgang! #FoutloosTypen", margin, bitmap.Height - margin + 10, footerPaint);
        }
    }
}
