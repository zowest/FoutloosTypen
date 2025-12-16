using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using CommunityToolkit.Maui.Views;
using FoutloosTypen.Views;

namespace FoutloosTypen.Services
{
    public class ShareUiService : IShareUiService
    {
        public async Task<bool> ShowImagePreviewAsync(string imagePath, string text)
        {
            var mainPage = Microsoft.Maui.Controls.Application.Current?.MainPage;
            if (mainPage == null) return false;

            var popup = new ImagePreviewPopup(imagePath, text);
            var result = await mainPage.ShowPopupAsync(popup);
            return result is bool b && b;
        }

        public Task OpenBrowserAsync(string url)
        {
            return Browser.OpenAsync(url, BrowserLaunchMode.External);
        }

        public Task ShareFileAsync(string title, string filePath)
        {
            return Share.RequestAsync(new ShareFileRequest { Title = title, File = new ShareFile(filePath) });
        }
    }
}
