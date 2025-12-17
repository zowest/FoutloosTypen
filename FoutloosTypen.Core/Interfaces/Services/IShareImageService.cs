using SkiaSharp;
using System.IO;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IShareImageService
    {
        string BuildTweetText(string lessonName, string progressText);
        SKBitmap Generate(string lessonName, string progressText, Stream? logoStream = null);
    }
}
