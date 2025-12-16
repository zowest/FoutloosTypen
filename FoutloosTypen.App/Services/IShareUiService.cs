using System.Threading.Tasks;

namespace FoutloosTypen.Services
{
    public interface IShareUiService
    {
        Task<bool> ShowImagePreviewAsync(string imagePath, string text);
        Task OpenBrowserAsync(string url);
        Task ShareFileAsync(string title, string filePath);
    }
}
