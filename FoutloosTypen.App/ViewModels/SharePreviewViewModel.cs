using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using Microsoft.Maui.ApplicationModel;
using SkiaSharp;

namespace FoutloosTypen.ViewModels
{
    public partial class SharePreviewViewModel : ObservableObject
    {
        private readonly IMediaUploadRepository _mediaUploadRepository;
        private readonly IXAuthRepository _xAuthRepository;
        private readonly IXAuthService _xAuthService;
        private readonly IShareImageRepository _shareImageRepository;
        private readonly XAuthSettings _xSettings;

        private readonly string _lessonName;
        private readonly string _progressText;
        private readonly int _lessonId;
        private readonly int _ownerUserId;
        private readonly SKBitmap _bitmap;
        private readonly string _tweetText;

        [ObservableProperty]
        private bool _isSharing;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public bool ShareSuccessful { get; private set; }

        public string TweetText => _tweetText;
        public byte[] ImageBytes { get; }

        public SharePreviewViewModel(
            IMediaUploadRepository mediaUploadRepository,
            IXAuthRepository xAuthRepository,
            IXAuthService xAuthService,
            IShareImageRepository shareImageRepository,
            XAuthSettings xSettings,
            string lessonName,
            string progressText,
            int lessonId,
            int ownerUserId,
            SKBitmap bitmap,
            string tweetText)
        {
            _mediaUploadRepository = mediaUploadRepository;
            _xAuthRepository = xAuthRepository;
            _xAuthService = xAuthService;
            _shareImageRepository = shareImageRepository;
            _xSettings = xSettings;
            _lessonName = lessonName;
            _progressText = progressText;
            _lessonId = lessonId;
            _ownerUserId = ownerUserId;
            _bitmap = bitmap;
            _tweetText = tweetText;

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 90);
            ImageBytes = data.ToArray();
        }

        [RelayCommand]
        public async Task ShareAsync()
        {
            try
            {
                IsSharing = true;
                ShareSuccessful = false;

                // Get per-user OAuth1 tokens
                var tokens = await _xAuthService.GetUserAccessTokensAsync(_ownerUserId);
                if (tokens == null)
                {
                    await ShowErrorAsync("Geen tokens", "OAuth1 tokens ontbreken voor deze gebruiker. Start eerst de OAuth1 flow.");
                    IsSharing = false;
                    return;
                }

                if (!ValidateOAuth1Credentials(tokens.Value.AccessToken, tokens.Value.AccessSecret))
                {
                    await ShowErrorAsync("Configuratiefout", "OAuth1 credentials zijn ongeldig.");
                    IsSharing = false;
                    return;
                }

                var imagePath = await SaveImageAsync();
                
                var mediaId = await UploadMediaAsync(imagePath, tokens.Value.AccessToken, tokens.Value.AccessSecret);
                await PostTweetAsync(mediaId, tokens.Value.AccessToken, tokens.Value.AccessSecret);

                ShareSuccessful = true;
                await ShowSuccessAsync();

                IsSharing = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fout: {ex.Message}";
                ShareSuccessful = false;
                IsSharing = false;
                await ShowErrorAsync("Delen mislukt", $"Er is een fout opgetreden: {ex.Message}");
            }
        }

        private bool ValidateOAuth1Credentials(string accessToken, string accessSecret)
        {
            return !string.IsNullOrEmpty(_xSettings.ConsumerKey) &&
                   !string.IsNullOrEmpty(_xSettings.ConsumerSecret) &&
                   !string.IsNullOrEmpty(accessToken) &&
                   !string.IsNullOrEmpty(accessSecret);
        }

        private async Task<string> SaveImageAsync()
        {
            StatusMessage = "Afbeelding opslaan...";
            return await _shareImageRepository.SaveImageToCacheAsync(_bitmap);
        }

        private async Task<string> UploadMediaAsync(string imagePath, string accessToken, string accessSecret)
        {
            StatusMessage = "Afbeelding uploaden naar X...";
            return await _mediaUploadRepository.UploadMediaAsync(
                imagePath,
                "image/png",
                _xSettings.ConsumerKey,
                _xSettings.ConsumerSecret,
                accessToken,
                accessSecret);
        }

        private async Task PostTweetAsync(string mediaId, string accessToken, string accessSecret)
        {
            StatusMessage = "Tweet posten...";
            await _mediaUploadRepository.PostTweetAsync(
                _tweetText,
                mediaId,
                _xSettings.ConsumerKey,
                _xSettings.ConsumerSecret,
                accessToken,
                accessSecret);
            StatusMessage = "Gelukt!";
        }

        private async Task ShowSuccessAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Gelukt!",
                        "Je bericht is succesvol gedeeld op X! Je browser opent nu je X-profiel.",
                        "OK");
                }
            });
        }

        private async Task ShowErrorAsync(string title, string message)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                }
            });
        }
    }
}
