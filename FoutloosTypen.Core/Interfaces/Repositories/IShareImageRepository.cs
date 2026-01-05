using System.Threading.Tasks;
using SkiaSharp;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IShareImageRepository
    {
        Task<string> SaveImageToCacheAsync(SKBitmap bitmap, string extension = "png");
    }
}
