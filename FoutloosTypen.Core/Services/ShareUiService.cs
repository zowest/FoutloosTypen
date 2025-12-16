using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using CommunityToolkit.Maui.Views;
using FoutloosTypen.Core.Interfaces.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FoutloosTypen.Core.Services
{
    public class ShareUiService : IShareUiService
    {
        public async Task<bool> ShowImagePreviewAsync(string imagePath, string text)
        {
            var mainPage = Microsoft.Maui.Controls.Application.Current?.MainPage;
            if (mainPage == null) return false;

            // Create popup content at runtime to avoid referencing App project views from Core
            var cancelButton = new Button { Text = "Cancel", Margin = new Thickness(8, 0) };
            var shareButton = new Button { Text = "Share", Margin = new Thickness(8, 0) };

            var headerGrid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }, Margin = new Thickness(0, 0, 0, 8) };
            headerGrid.Add(new Label { Text = "Preview", VerticalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold });
            headerGrid.Add(cancelButton, 1, 0);
            headerGrid.Add(shareButton, 2, 0);

            var previewImage = new Image
            {
                Source = ImageSource.FromFile(imagePath),
                Aspect = Aspect.AspectFit,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            var tweetLabel = new Label
            {
                Text = text,
                LineBreakMode = LineBreakMode.TailTruncation,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var grid = new Grid { RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
            grid.Add(headerGrid, 0, 0);
            grid.Add(previewImage, 0, 1);
            grid.Add(tweetLabel, 0, 2);

            var frame = new Frame
            {
                Padding = 12,
                BackgroundColor = Colors.White,
                CornerRadius = 12,
                HasShadow = true,
                WidthRequest = 600,
                HeightRequest = 800,
                Content = grid
            };

            var popup = new Popup { Size = new Size(600, 800), Content = frame };

            cancelButton.Clicked += (s, e) => popup.Close(false);
            shareButton.Clicked += (s, e) => popup.Close(true);

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

        public void Dispose() { }
    }
}
