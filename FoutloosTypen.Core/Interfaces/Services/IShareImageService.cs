using SkiaSharp;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IShareImageService
    {
        string BuildTweetText(string lessonName, string progressText);
        SKBitmap Generate(string lessonName, string progressText);
        Task<string> SaveToCacheAsync(SKBitmap bitmap, string extension = "png");
    }
}
