using SkiaSharp;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Helpers
{
    public static class ShareImageGenerator
    {
        // Color scheme
        private static readonly SKColor PrimaryYellow = SKColor.Parse("#FFD700");
        private static readonly SKColor LightYellow = SKColor.Parse("#FFF8DC");
        private static readonly SKColor DarkGray = SKColor.Parse("#333333");
        private static readonly SKColor MediumGray = SKColor.Parse("#666666");
        private static readonly SKColor CardWhite = SKColors.White;
        private static readonly SKColor AccentBlue = SKColor.Parse("#4A90E2");

        public static string BuildTweetText(string lessonName, string progressText)
            => $"{lessonName} - {progressText} #BolType";

        public static SKBitmap Generate(string lessonName, string progressText, System.IO.Stream? logoStream = null)
        {
            var bitmap = new SKBitmap(1080, 1080);
            using var canvas = new SKCanvas(bitmap);

            // Transparent background for PNG
            canvas.Clear(SKColors.Transparent);

            // Yellow header wave/blob
            DrawYellowHeader(canvas, bitmap.Width);

            float margin = 60f;
            float currentY = margin + 40;

            // Logo and title
            currentY = DrawBranding(canvas, margin, currentY, logoStream);

            // Main content card
            currentY = DrawProgressCard(canvas, bitmap.Width, margin, currentY, lessonName, progressText);

            // Footer
            DrawFooter(canvas, bitmap.Width, bitmap.Height, margin);

            return bitmap;
        }

        public static SKBitmap GenerateWithResults(string lessonName, Result result, System.IO.Stream? logoStream = null)
        {
            var bitmap = new SKBitmap(1080, 1080);
            using var canvas = new SKCanvas(bitmap);

            // Transparent background for PNG
            canvas.Clear(SKColors.Transparent);

            // Yellow header wave blobs
            DrawYellowHeader(canvas, bitmap.Width);

            float margin = 60f;
            float currentY = margin + 40;

            // Logo and title
            currentY = DrawBranding(canvas, margin, currentY, logoStream);

            // Main content card with results
            currentY = DrawResultsCard(canvas, bitmap.Width, margin, currentY, lessonName, result);

            // Footer
            DrawFooter(canvas, bitmap.Width, bitmap.Height, margin);

            return bitmap;
        }

        private static void DrawYellowHeader(SKCanvas canvas, int width)
        {
            using var path = new SKPath();
            
            // Create a smooth wave/blob shape at the top
            path.MoveTo(0, 0);
            path.LineTo(width, 0);
            path.LineTo(width, 280);
            
            // Smooth curve at bottom of yellow section
            path.CubicTo(
                width * 0.75f, 320,
                width * 0.25f, 240,
                0, 280
            );
            path.Close();

            using var paint = new SKPaint
            {
                Color = PrimaryYellow,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawPath(path, paint);
        }

        private static float DrawBranding(SKCanvas canvas, float margin, float currentY, System.IO.Stream? logoStream)
        {
            // Logo in circle
            float logoSize = 100;
            float logoX = margin;
            float logoY = currentY;
            
            // Circle background for logo
            using var circlePaint = new SKPaint
            {
                Color = CardWhite,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawCircle(logoX + logoSize/2, logoY + logoSize/2, logoSize/2, circlePaint);

            SKBitmap? logoImage = null;
            if (logoStream != null)
            {
                try
                {
                    logoImage = SKBitmap.Decode(logoStream);
                }
                catch { }
            }

            if (logoImage != null)
            {
                using (logoImage)
                {
                    // Draw logo inside circle (80% of circle size for better visibility)
                    float innerSize = logoSize * 0.8f;
                    float offset = (logoSize - innerSize) / 2;
                    var destRect = new SKRect(
                        logoX + offset, 
                        logoY + offset, 
                        logoX + offset + innerSize, 
                        logoY + offset + innerSize);
                    canvas.DrawBitmap(logoImage, destRect, new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High });
                }
            }
            else
            {
                // Fallback "FT" text in circle
                using var logoTextPaint = new SKPaint
                {
                    Color = PrimaryYellow,
                    TextSize = 40,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
                    TextAlign = SKTextAlign.Center
                };
                canvas.DrawText("FT", logoX + logoSize/2, logoY + logoSize/2 + 14, logoTextPaint);
            }

            // Circle border
            using var circleBorderPaint = new SKPaint
            {
                Color = CardWhite,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3
            };
            canvas.DrawCircle(logoX + logoSize/2, logoY + logoSize/2, logoSize/2 - 1.5f, circleBorderPaint);

            // BolType title next to circle
            using var titlePaint = new SKPaint
            {
                Color = CardWhite,
                TextSize = 56,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText("BolType", logoX + logoSize + 20, logoY + logoSize/2 + 18, titlePaint);

            return currentY + logoSize + 40;
        }

        private static float DrawProgressCard(SKCanvas canvas, int width, float margin, float currentY, string lessonName, string progressText)
        {
            float cardWidth = width - (margin * 2);
            float cardHeight = 480;
            float cardX = margin;
            float cardY = currentY;

            // Card shadow
            using var shadowPaint = new SKPaint
            {
                Color = SKColor.Parse("#20000000"),
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 12)
            };
            canvas.DrawRoundRect(cardX + 8, cardY + 8, cardWidth, cardHeight, 20, 20, shadowPaint);

            // Card background
            using var cardPaint = new SKPaint
            {
                Color = CardWhite,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRoundRect(cardX, cardY, cardWidth, cardHeight, 20, 20, cardPaint);

            // Card border
            using var borderPaint = new SKPaint
            {
                Color = LightYellow,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3
            };
            canvas.DrawRoundRect(cardX, cardY, cardWidth, cardHeight, 20, 20, borderPaint);

            float contentY = cardY + 60;

            // "Les" label
            using var labelPaint = new SKPaint
            {
                Color = MediumGray,
                TextSize = 32,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal)
            };
            canvas.DrawText("Les", cardX + 40, contentY, labelPaint);
            contentY += 60;

            // Lesson name
            using var lessonNamePaint = new SKPaint
            {
                Color = DarkGray,
                TextSize = 48,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText(lessonName, cardX + 40, contentY, lessonNamePaint);
            contentY += 80;

            // Divider line
            using var dividerPaint = new SKPaint
            {
                Color = LightYellow,
                StrokeWidth = 2,
                IsAntialias = true
            };
            canvas.DrawLine(cardX + 40, contentY, cardX + cardWidth - 40, contentY, dividerPaint);
            contentY += 60;

            // Progress text
            using var progressPaint = new SKPaint
            {
                Color = AccentBlue,
                TextSize = 40,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText(progressText, cardX + 40, contentY, progressPaint);

            // Date badge (top-right corner of card)
            DrawDateBadge(canvas, cardX + cardWidth - 160, cardY + 30);

            return cardY + cardHeight + 40;
        }

        private static float DrawResultsCard(SKCanvas canvas, int width, float margin, float currentY, string lessonName, Result result)
        {
            float cardWidth = width - (margin * 2);
            float cardHeight = 600;
            float cardX = margin;
            float cardY = currentY;

            // Card shadow
            using var shadowPaint = new SKPaint
            {
                Color = SKColor.Parse("#20000000"),
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 12)
            };
            canvas.DrawRoundRect(cardX + 8, cardY + 8, cardWidth, cardHeight, 20, 20, shadowPaint);

            // Card background
            using var cardPaint = new SKPaint
            {
                Color = CardWhite,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRoundRect(cardX, cardY, cardWidth, cardHeight, 20, 20, cardPaint);

            // Card border
            using var borderPaint = new SKPaint
            {
                Color = LightYellow,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3
            };
            canvas.DrawRoundRect(cardX, cardY, cardWidth, cardHeight, 20, 20, borderPaint);

            float contentY = cardY + 60;

            // "Resultaat" label
            using var labelPaint = new SKPaint
            {
                Color = MediumGray,
                TextSize = 32,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal)
            };
            canvas.DrawText("Resultaat", cardX + 40, contentY, labelPaint);
            contentY += 60;

            // Lesson name
            using var lessonNamePaint = new SKPaint
            {
                Color = DarkGray,
                TextSize = 44,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText(lessonName, cardX + 40, contentY, lessonNamePaint);
            contentY += 70;

            // Score highlight
            DrawScoreBadge(canvas, cardX + cardWidth - 200, contentY - 40, result.Score);

            // Stats grid
            contentY = DrawStatsGrid(canvas, cardX, contentY, cardWidth, result);

            // Date badge
            DrawDateBadge(canvas, cardX + cardWidth - 160, cardY + 30);

            return cardY + cardHeight + 40;
        }

        private static float DrawStatsGrid(SKCanvas canvas, float cardX, float startY, float cardWidth, Result result)
        {
            float currentY = startY;
            float leftCol = cardX + 40;
            float rightCol = cardX + cardWidth / 2 + 20;
            float spacing = 70;

            var stats = new[]
            {
                ("Nauwkeurigheid", $"{result.AccuracyPercent:F1}%"),
                ("Woorden/min", $"{result.WordsPerMinute}"),
                ("Aanslagen/min", $"{result.StrokesPerMinute}"),
                ("Fouten", $"{result.TotalMistakes}"),
                ("Zinnen", $"{result.SentencesCompleted}")
            };

            using var labelPaint = new SKPaint
            {
                Color = MediumGray,
                TextSize = 32,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal)
            };

            using var valuePaint = new SKPaint
            {
                Color = AccentBlue,
                TextSize = 36,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            int index = 0;
            foreach (var (label, value) in stats)
            {
                float x = (index % 2 == 0) ? leftCol : rightCol;
                
                canvas.DrawText(label, x, currentY, labelPaint);
                canvas.DrawText(value, x, currentY + 35, valuePaint);

                if (index % 2 == 1)
                    currentY += spacing;
                
                index++;
            }

            return currentY + 40;
        }

        private static void DrawScoreBadge(SKCanvas canvas, float x, float y, int score)
        {
            float badgeSize = 140;

            // Badge background circle
            using var badgePaint = new SKPaint
            {
                Color = PrimaryYellow,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawCircle(x + badgeSize / 2, y + badgeSize / 2, badgeSize / 2, badgePaint);

            // Badge border
            using var badgeBorderPaint = new SKPaint
            {
                Color = CardWhite,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 4
            };
            canvas.DrawCircle(x + badgeSize / 2, y + badgeSize / 2, badgeSize / 2 - 2, badgeBorderPaint);

            // Score text
            using var scorePaint = new SKPaint
            {
                Color = CardWhite,
                TextSize = 48,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText(score.ToString(), x + badgeSize / 2, y + badgeSize / 2 + 16, scorePaint);
        }

        private static void DrawDateBadge(SKCanvas canvas, float x, float y)
        {
            var date = System.DateTime.Now.ToString("dd/MM/yyyy");
            
            // Badge background
            using var badgePaint = new SKPaint
            {
                Color = LightYellow,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRoundRect(x, y, 150, 50, 25, 25, badgePaint);

            // Date text
            using var datePaint = new SKPaint
            {
                Color = DarkGray,
                TextSize = 24,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal),
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText(date, x + 75, y + 33, datePaint);
        }

        private static void DrawFooter(SKCanvas canvas, int width, int height, float margin)
        {
            using var footerPaint = new SKPaint
            {
                Color = MediumGray,
                TextSize = 28,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal),
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText("Deel je voortgang! #BolType", width / 2, height - margin + 10, footerPaint);
        }
    }
}
