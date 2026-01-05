using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using Microsoft.Maui.ApplicationModel;
using SkiaSharp;

namespace FoutloosTypen.ViewModels
{
    public partial class SharePreviewViewModel : ObservableObject
    {
        private readonly IMediaUploadRepository _mediaUploadRepository;
        private readonly IXAuthRepository _xAuthRepository;
        private readonly IShareImageRepository _shareImageRepository;
        private readonly XAuthSettings _xSettings;

        private readonly string _lessonName;
        private readonly string _progressText;
        private readonly int _lessonId;
        private readonly SKBitmap _bitmap;
        private readonly string _tweetText;

        [ObservableProperty]
        private bool _isSharing;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public string TweetText => _tweetText;
        public byte[] ImageBytes { get; }

        public SharePreviewViewModel(
            IMediaUploadRepository mediaUploadRepository,
            IXAuthRepository xAuthRepository,
            IShareImageRepository shareImageRepository,
            XAuthSettings xSettings,
            string lessonName,
            string progressText,
            int lessonId,
            SKBitmap bitmap,
            string tweetText)
        {
            _mediaUploadRepository = mediaUploadRepository;
            _xAuthRepository = xAuthRepository;
            _shareImageRepository = shareImageRepository;
            _xSettings = xSettings;
            _lessonName = lessonName;
            _progressText = progressText;
            _lessonId = lessonId;
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

                if (!ValidateOAuth1Credentials())
                {
                    await ShowErrorAsync("Configuratiefout", "OAuth1 credentials ontbreken.");
                    IsSharing = false;
                    return;
                }

                var imagePath = await SaveImageAsync();
                
                var mediaId = await UploadMediaAsync(imagePath);
                await PostTweetAsync(mediaId);

                await ShowSuccessAsync();
                await OpenXProfileAsync();

                IsSharing = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fout: {ex.Message}";
                IsSharing = false;
                await ShowErrorAsync("Delen mislukt", $"Er is een fout opgetreden: {ex.Message}");
            }
        }

        private bool ValidateOAuth1Credentials()
        {
            return !string.IsNullOrEmpty(_xSettings.ConsumerKey) &&
                   !string.IsNullOrEmpty(_xSettings.ConsumerSecret) &&
                   !string.IsNullOrEmpty(_xSettings.OAuthToken) &&
                   !string.IsNullOrEmpty(_xSettings.OAuthTokenSecret);
        }

        private async Task<string> SaveImageAsync()
        {
            StatusMessage = "Afbeelding opslaan...";
            return await _shareImageRepository.SaveImageToCacheAsync(_bitmap);
        }

        private async Task<string> UploadMediaAsync(string imagePath)
        {
            StatusMessage = "Afbeelding uploaden naar X...";
            return await _mediaUploadRepository.UploadMediaAsync(
                imagePath,
                "image/png",
                _xSettings.ConsumerKey,
                _xSettings.ConsumerSecret,
                _xSettings.OAuthToken,
                _xSettings.OAuthTokenSecret);
        }

        private async Task PostTweetAsync(string mediaId)
        {
            StatusMessage = "Tweet posten...";
            await _mediaUploadRepository.PostTweetAsync(
                _tweetText,
                mediaId,
                _xSettings.ConsumerKey,
                _xSettings.ConsumerSecret,
                _xSettings.OAuthToken,
                _xSettings.OAuthTokenSecret);
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
                        "Je bericht is succesvol gedeeld op X! Je wordt doorgestuurd naar de hoofdpagina.",
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

        private async Task OpenXProfileAsync()
        {
            try
            {
                var handle = await _xAuthRepository.GetAuthenticatedHandleAsync();
                var profileUrl = !string.IsNullOrEmpty(handle)
                    ? $"https://x.com/{handle}"
                    : "https://x.com/home";

                await Launcher.OpenAsync(new Uri(profileUrl));
            }
            catch
            {
                // Silent fail - not critical
            }
        }
    }
}
