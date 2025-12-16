using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using FoutloosTypen.Views;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;

namespace FoutloosTypen.Services
{
    public interface ISocialShareService
    {
        Task<bool> ShareToXWithConfirmationAsync(string lessonName, string progressText);
        Task<bool> RegrantPostingConsentAsync();
    }

    public class SocialShareService : ISocialShareService
    {
        private readonly IShareImageService _shareImageService;
        private readonly IFileSaver _fileSaver;
        private readonly IXAuthService _xAuthService;
        private readonly IXMediaUploadService _mediaUploadService;
        private readonly XAuthSettings _settings;

        public SocialShareService(IShareImageService shareImageService, IXAuthService xAuthService, XAuthSettings settings, IXMediaUploadService mediaUploadService)
        {
            _shareImageService = shareImageService;
            _xAuthService = xAuthService;
            _settings = settings;
            _fileSaver = FileSaver.Default;
            _mediaUploadService = mediaUploadService;
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
                // Use OAuth1 user access token & secret from settings for v1.1 media upload via mediaUploadService
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
                // Missing OAuth1 credentials: fall back to native sharing and browser intent
                await Share.RequestAsync(new ShareFileRequest { Title = "Deel op X", File = new ShareFile(imagePath) });
                await ShareToXViaBrowserAsync(lessonName, progressText);
                return false;
            }
            catch (HttpRequestException hre) when (hre.Message.Contains("403") || hre.Message.Contains("Forbidden", StringComparison.OrdinalIgnoreCase))
            {
                // Force user consent to gain missing scopes like media.write
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

            // If OAuth1 tokens available post using OAuth1 user context, otherwise use OAuth2 v2 tweet
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
            // Always request user consent when authenticating explicitly
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
            // Try existing access token
            var accessToken = await _xAuthService.GetStoredAccessTokenAsync();
            if (!string.IsNullOrEmpty(accessToken))
            {
                return accessToken;
            }

            // Try refresh token first
            var refreshed = await TryRefreshTokenAsync();
            if (!string.IsNullOrEmpty(refreshed))
            {
                return refreshed;
            }

            // No tokens available: require explicit consent before posting
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
            // Clear local tokens and force user consent to acquire new scopes (e.g., media.write)
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
            // Require fresh consent before showing preview
            var regranted = await RegrantPostingConsentAsync();
            if (!regranted)
            {
                // If user cancels consent, stop early
                return false;
            }

            var imagePath = await _shareImageService.SaveLessonSummaryImageAsync(lessonName, progressText);
            var tweetText = $"{lessonName} - {progressText} #FoutloosTypen";

            var mainPage = Microsoft.Maui.Controls.Application.Current?.MainPage;
            if (mainPage == null) return false;

            var popup = new ImagePreviewPopup(imagePath, tweetText);
            var confirmed = (await mainPage.ShowPopupAsync(popup)) is bool b && b;
            if (!confirmed) return false;

            var accessToken = await _xAuthService.GetStoredAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await Share.RequestAsync(new ShareFileRequest { Title = "Deel op X", File = new ShareFile(imagePath) });
                await ShareToXViaBrowserAsync(lessonName, progressText);
                return false;
            }

            var contentType = Path.GetExtension(imagePath).Equals(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";
            string? mediaId = null;
            try
            {
                // Use OAuth1 user tokens for media upload. Prefer tokens from settings; if not present, fall back to stored OAuth1 tokens.
                var oauth1Token = !string.IsNullOrEmpty(_settings.OAuthToken) ? _settings.OAuthToken : await _xAuthService.GetStoredAccessTokenAsync();
                var oauth1Secret = !string.IsNullOrEmpty(_settings.OAuthTokenSecret) ? _settings.OAuthTokenSecret : await _xAuthService.GetStoredRefreshTokenAsync();

                if (string.IsNullOrWhiteSpace(oauth1Token) || string.IsNullOrWhiteSpace(oauth1Secret))
                {
                    // Can't upload via v1.1 without OAuth1 credentials
                    await Share.RequestAsync(new ShareFileRequest { Title = "Deel op X", File = new ShareFile(imagePath) });
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
                // If 403 occurs even after consent, allow one retry by re-consenting
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
                await Share.RequestAsync(new ShareFileRequest { Title = "Deel op X", File = new ShareFile(imagePath) });
                await ShareToXViaBrowserAsync(lessonName, progressText);
                return false;
            }

            // Call CreateTweetWithOAuth1 only if media was uploaded with OAuth1
            if (!string.IsNullOrEmpty(_settings.OAuthToken) && !string.IsNullOrEmpty(_settings.OAuthTokenSecret))
            {
                await _mediaUploadService.CreateTweetWithOAuth1Async(tweetText, mediaId, _settings.ConsumerKey, _settings.ConsumerSecret, _settings.OAuthToken, _settings.OAuthTokenSecret);
            }
            else
            {
                await _xAuthService.CreateTweetAsync(tweetText, mediaId);
            }

            await Browser.OpenAsync("https://twitter.com", BrowserLaunchMode.External);
            return true;
        }
    }
}
