using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using FoutloosTypen.Core.Interfaces.Repositories;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class ShareImageRepository : IShareImageRepository
    {
        public async Task<string> SaveImageToCacheAsync(SKBitmap bitmap, string extension = "png")
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(
                extension.Equals("jpg", StringComparison.OrdinalIgnoreCase) 
                    ? SKEncodedImageFormat.Jpeg 
                    : SKEncodedImageFormat.Png, 
                90);

            var dir = Path.GetTempPath();
            var filePath = Path.Combine(dir, $"les_resultaat_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{extension}");
            
            await File.WriteAllBytesAsync(filePath, data.ToArray());
            
            return filePath;
        }
    }
}
