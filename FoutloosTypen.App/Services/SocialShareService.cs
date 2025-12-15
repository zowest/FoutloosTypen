using System.Diagnostics;
using System.IO;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel; // Browser
using CommunityToolkit.Maui.Views; // Popup
using FoutloosTypen.Views; // ImagePreviewPopup

namespace FoutloosTypen.Services
{
    public interface ISocialShareService
    {
        Task<bool> ShareToXWithConfirmationAsync(string lessonName, string progressText);
    }

    public class SocialShareService : ISocialShareService
    {
        private readonly IShareImageService _shareImageService;
        private readonly IFileSaver _fileSaver;
        private readonly IXAuthService _xAuthService;
        private readonly XAuthSettings _settings;

        public SocialShareService(IShareImageService shareImageService, IXAuthService xAuthService, XAuthSettings settings)
        {
            _shareImageService = shareImageService;
            _xAuthService = xAuthService;
            _settings = settings;
            _fileSaver = FileSaver.Default;
        }

        // Uses X API to upload the generated SkiaSharp image and tweet with media
        public async Task<bool> ShareToTwitterAsync(string lessonName, string progressText)
        {
            var accessToken = await EnsureAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return false;
            }

            // Derive correct content type from generated file
            var imagePath = await _shareImageService.SaveLessonSummaryImageAsync(lessonName, progressText);
            var contentType = Path.GetExtension(imagePath).Equals(".png", StringComparison.OrdinalIgnoreCase)
                ? "image/png"
                : "image/jpeg";

            var mediaId = await _xAuthService.UploadMediaAsync(imagePath, contentType);
            if (string.IsNullOrEmpty(mediaId))
            {
                var refreshed = await TryRefreshTokenAsync();
                if (string.IsNullOrEmpty(refreshed)) return false;

                mediaId = await _xAuthService.UploadMediaAsync(imagePath, contentType);
                if (string.IsNullOrEmpty(mediaId)) return false;
            }

            var tweetText = $"{lessonName} - {progressText} #FoutloosTypen";
            await _xAuthService.CreateTweetAsync(tweetText, mediaId);
            return true;
        }

        public async Task ShareGenericAsync(string lessonName, string progressText)
        {
            await _shareImageService.ShareLessonSummaryImageAsync(lessonName, progressText);
        }

        public async Task<string?> SaveWithPickerAsync(string lessonName, string progressText)
        {
            return await _shareImageService.SaveWithPickerAsync(lessonName, progressText);
        }

        public async Task<bool> AuthenticateWithXAsync()
        {
            var code = await _xAuthService.AuthenticateAsync(_settings.ClientId, _settings.RedirectUri, _settings.Scopes);
            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

            var access = await _xAuthService.ExchangeCodeForTokenAsync(_settings.ClientId, code, _settings.RedirectUri);
            return !string.IsNullOrEmpty(access);
        }

        public async Task<bool> HasXAccessTokenAsync()
        {
            var access = await _xAuthService.GetStoredAccessTokenAsync();
            if (!string.IsNullOrEmpty(access))
            {
                return true;
            }

            var refresh = await _xAuthService.GetStoredRefreshTokenAsync();
            if (string.IsNullOrEmpty(refresh))
            {
                return false;
            }

            var refreshed = await _xAuthService.RefreshAccessTokenAsync(_settings.ClientId, refresh, _settings.RedirectUri);
            return !string.IsNullOrEmpty(refreshed);
        }

        private async Task<string?> EnsureAccessTokenAsync()
        {
            var accessToken = await _xAuthService.GetStoredAccessTokenAsync();
            if (!string.IsNullOrEmpty(accessToken))
            {
                return accessToken;
            }

            return await TryRefreshTokenAsync();
        }

        private async Task<string?> TryRefreshTokenAsync()
        {
            var refreshToken = await _xAuthService.GetStoredRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
            {
                return null;
            }

            return await _xAuthService.RefreshAccessTokenAsync(_settings.ClientId, refreshToken, _settings.RedirectUri);
        }

        private async Task<bool> ShareToXViaBrowserAsync(string lessonName, string progressText)
        {
            var tweetText = Uri.EscapeDataString($"{lessonName} - {progressText} #FoutloosTypen");
            var url = $"https://twitter.com/intent/tweet?text={tweetText}";
            try { await Browser.OpenAsync(url, BrowserLaunchMode.External); return true; }
            catch { return false; }
        }

        public async Task<bool> ShareToXWithConfirmationAsync(string lessonName, string progressText)
        {
            var imagePath = await _shareImageService.SaveLessonSummaryImageAsync(lessonName, progressText);
            var tweetText = $"{lessonName} - {progressText} #FoutloosTypen";

            var mainPage = Microsoft.Maui.Controls.Application.Current?.MainPage;
            if (mainPage == null) return false;

            var popup = new ImagePreviewPopup(imagePath, tweetText);
            var confirmed = (await mainPage.ShowPopupAsync(popup)) is bool b && b;
            if (!confirmed) return false;

            var accessToken = await EnsureAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                // Guide user to login or fallback
                await Share.RequestAsync(new ShareFileRequest { Title = "Deel op X", File = new ShareFile(imagePath) });
                await ShareToXViaBrowserAsync(lessonName, progressText);
                return false;
            }

            var contentType = Path.GetExtension(imagePath).Equals(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";
            var mediaId = await _xAuthService.UploadMediaAsync(imagePath, contentType);
            if (string.IsNullOrEmpty(mediaId))
            {
                var refreshed = await TryRefreshTokenAsync();
                if (!string.IsNullOrEmpty(refreshed))
                    mediaId = await _xAuthService.UploadMediaAsync(imagePath, contentType);
            }
            if (string.IsNullOrEmpty(mediaId))
            {
                await Share.RequestAsync(new ShareFileRequest { Title = "Deel op X", File = new ShareFile(imagePath) });
                await ShareToXViaBrowserAsync(lessonName, progressText);
                return false;
            }

            await _xAuthService.CreateTweetAsync(tweetText, mediaId);

            // Optionally, you can open the generic X (Twitter) homepage instead:
            await Browser.OpenAsync("https://twitter.com", BrowserLaunchMode.External);

            return true;
        }
    }
}
