using System;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using FoutloosTypen.Core.Interfaces.Services;

namespace FoutloosTypen.Core.Services
{
    public class SocialShareService : ISocialShareService
    {
        private readonly IShareImageService _shareImageService;
        private readonly IFileSaver _fileSaver;
        private readonly IXAuthService _xAuthService;
        private readonly IXMediaUploadService _mediaUploadService;
        private readonly XAuthSettings _settings;
        private readonly IShareUiService _shareUiService;

        public SocialShareService(IShareImageService shareImageService, IXAuthService xAuthService, XAuthSettings settings, IXMediaUploadService mediaUploadService, IShareUiService shareUiService)
        {
            _shareImageService = shareImageService;
            _xAuthService = xAuthService;
            _settings = settings;
            _fileSaver = FileSaver.Default;
            _mediaUploadService = mediaUploadService;
            _shareUiService = shareUiService;
        }

        public async Task<bool> ShareToTwitterAsync(string lessonName, string progressText)
        {
            var accessToken = await EnsureAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return false;
            }

            var imagePath = await _shareImageService.SaveLessonSummaryImageAsync(lessonName, progressText);
            var contentType = Path.GetExtension(imagePath).Equals(".png", StringComparison.OrdinalIgnoreCase)
                ? "image/png"
                : "image/jpeg";

            string? mediaId = null;
            try
            {
                mediaId = await _mediaUploadService.UploadMediaV11Async(
                    imagePath,
                    contentType,
                    _settings.ConsumerKey,
                    _settings.ConsumerSecret,
                    _settings.OAuthToken,
                    _settings.OAuthTokenSecret);
            }
            catch (ArgumentException)
            {
                await _shareUiService.ShareFileAsync(imagePath, "Deel op X");
                await ShareToXViaBrowserAsync(lessonName, progressText);
                return false;
            }
            catch (HttpRequestException hre) when (hre.Message.Contains("403") || hre.Message.Contains("Forbidden", StringComparison.OrdinalIgnoreCase))
            {
                var regranted = await RegrantPostingConsentAsync();
                if (regranted)
                {
                    mediaId = await _mediaUploadService.UploadMediaV11Async(
                        imagePath,
                        contentType,
                        _settings.ConsumerKey,
                        _settings.ConsumerSecret,
                        _settings.OAuthToken,
                        _settings.OAuthTokenSecret);
                }
            }

            if (string.IsNullOrEmpty(mediaId))
            {
                var refreshed = await TryRefreshTokenAsync();
                if (string.IsNullOrEmpty(refreshed)) return false;

                mediaId = await _mediaUploadService.UploadMediaV11Async(
                    imagePath,
                    contentType,
                    _settings.ConsumerKey,
                    _settings.ConsumerSecret,
                    _settings.OAuthToken,
                    _settings.OAuthTokenSecret);
                if (string.IsNullOrEmpty(mediaId)) return false;
            }

            var tweetText = $"{lessonName} - {progressText} #FoutloosTypen";

            if (!string.IsNullOrEmpty(_settings.OAuthToken) && !string.IsNullOrEmpty(_settings.OAuthTokenSecret))
            {
                await _mediaUploadService.CreateTweetWithOAuth1Async(tweetText, mediaId, _settings.ConsumerKey, _settings.ConsumerSecret, _settings.OAuthToken, _settings.OAuthTokenSecret);
            }
            else
            {
                await _xAuthService.CreateTweetAsync(tweetText, mediaId);
            }

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
            var code = await _xAuthService.AuthenticateAsync(_settings.ClientId, _settings.RedirectUri, _settings.Scopes, forceConsent: true);
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

            var refreshed = await TryRefreshTokenAsync();
            if (!string.IsNullOrEmpty(refreshed))
            {
                return refreshed;
            }

            var code = await _xAuthService.AuthenticateAsync(_settings.ClientId, _settings.RedirectUri, _settings.Scopes, forceConsent: true);
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }

            var exchanged = await _xAuthService.ExchangeCodeForTokenAsync(_settings.ClientId, code, _settings.RedirectUri);
            return exchanged;
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

        public async Task<bool> RegrantPostingConsentAsync()
        {
            await _xAuthService.SignOutAsync();
            var code = await _xAuthService.AuthenticateAsync(_settings.ClientId, _settings.RedirectUri, _settings.Scopes, forceConsent: true);
            if (string.IsNullOrEmpty(code)) return false;
            var access = await _xAuthService.ExchangeCodeForTokenAsync(_settings.ClientId, code, _settings.RedirectUri);
            return !string.IsNullOrEmpty(access);
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
            var regranted = await RegrantPostingConsentAsync();
            if (!regranted)
            {
                return false;
            }

            var imagePath = await _shareImageService.SaveLessonSummaryImageAsync(lessonName, progressText);
            var tweetText = $"{lessonName} - {progressText} #FoutloosTypen";

            var confirmed = await _shareUiService.ShowImagePreviewAsync(imagePath, tweetText);
            if (!confirmed) return false;

            var accessToken = await _xAuthService.GetStoredAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _shareUiService.ShareFileAsync(imagePath, "Deel op X");
                await ShareToXViaBrowserAsync(lessonName, progressText);
                return false;
            }

            var contentType = Path.GetExtension(imagePath).Equals(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";
            string? mediaId = null;
            try
            {
                var oauth1Token = !string.IsNullOrEmpty(_settings.OAuthToken) ? _settings.OAuthToken : await _xAuthService.GetStoredAccessTokenAsync();
                var oauth1Secret = !string.IsNullOrEmpty(_settings.OAuthTokenSecret) ? _settings.OAuthTokenSecret : await _xAuthService.GetStoredRefreshTokenAsync();

                if (string.IsNullOrWhiteSpace(oauth1Token) || string.IsNullOrWhiteSpace(oauth1Secret))
                {
                    await _shareUiService.ShareFileAsync(imagePath, "Deel op X");
                    await ShareToXViaBrowserAsync(lessonName, progressText);
                    return false;
                }

                mediaId = await _mediaUploadService.UploadMediaV11Async(
                    imagePath,
                    contentType,
                    _settings.ConsumerKey,
                    _settings.ConsumerSecret,
                    oauth1Token,
                    oauth1Secret);
            }
            catch (HttpRequestException hre) when (hre.Message.Contains("403") || hre.Message.Contains("Forbidden", StringComparison.OrdinalIgnoreCase))
            {
                var consentRetry = await RegrantPostingConsentAsync();
                if (consentRetry)
                {
                    var oauth1Token = !string.IsNullOrEmpty(_settings.OAuthToken) ? _settings.OAuthToken : await _xAuthService.GetStoredAccessTokenAsync();
                    var oauth1Secret = !string.IsNullOrEmpty(_settings.OAuthTokenSecret) ? _settings.OAuthTokenSecret : await _xAuthService.GetStoredRefreshTokenAsync();

                    if (!string.IsNullOrWhiteSpace(oauth1Token) && !string.IsNullOrWhiteSpace(oauth1Secret))
                    {
                        mediaId = await _mediaUploadService.UploadMediaV11Async(
                            imagePath,
                            contentType,
                            _settings.ConsumerKey,
                            _settings.ConsumerSecret,
                            oauth1Token,
                            oauth1Secret);
                    }
                }
            }

            if (string.IsNullOrEmpty(mediaId))
            {
                await _shareUiService.ShareFileAsync(imagePath, "Deel op X");
                await ShareToXViaBrowserAsync(lessonName, progressText);
                return false;
            }

            if (!string.IsNullOrEmpty(_settings.OAuthToken) && !string.IsNullOrEmpty(_settings.OAuthTokenSecret))
            {
                await _mediaUploadService.CreateTweetWithOAuth1Async(tweetText, mediaId, _settings.ConsumerKey, _settings.ConsumerSecret, _settings.OAuthToken, _settings.OAuthTokenSecret);
            }
            else
            {
                await _xAuthService.CreateTweetAsync(tweetText, mediaId);
            }
            var handle = await _xAuthService.GetAuthenticatedHandleAsync();
            var profileUrl = !string.IsNullOrWhiteSpace(handle) ? $"https://x.com/{handle}" : "https://x.com";
            try { await Browser.OpenAsync(profileUrl, BrowserLaunchMode.External); }
            catch { /* ignore browser open failures */ }

            return true;
        }
    }
}

