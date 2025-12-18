using FoutloosTypen.Core.Models;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Helpers;
using SkiaSharp;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using CommunityToolkit.Maui.Views;

namespace FoutloosTypen.ViewModels
{
    public partial class LessonResultViewModel : ObservableObject
    {
        private readonly Result _progress;
        private readonly IXAuthService? _xAuthService;
        private readonly IMediaUploadRepository? _mediaUploadRepository;
        private readonly IXAuthRepository? _xAuthRepository;
        private readonly IShareImageRepository? _shareImageRepository;
        private readonly XAuthSettings? _xSettings;
        private readonly string _lessonName;

        [ObservableProperty]
        private bool _isSharing;

        // Constructor voor gebruik met share functionaliteit
        public LessonResultViewModel(
            Result progress,
            string lessonName,
            IXAuthService xAuthService,
            IMediaUploadRepository mediaUploadRepository,
            IXAuthRepository xAuthRepository,
            IShareImageRepository shareImageRepository,
            XAuthSettings xSettings)
        {
            _progress = progress;
            _lessonName = lessonName;
            _xAuthService = xAuthService;
            _mediaUploadRepository = mediaUploadRepository;
            _xAuthRepository = xAuthRepository;
            _shareImageRepository = shareImageRepository;
            _xSettings = xSettings;
        }

        // Constructor zonder share functionaliteit (backward compatibility)
        public LessonResultViewModel(Result progress)
        {
            _progress = progress;
            _lessonName = string.Empty;
        }

        // All calculations are now done in the service, just expose the values
        public int Score => _progress.Score;

        public int StrokesPerMinute => _progress.StrokesPerMinute;

        public int WordsPerMinute => _progress.WordsPerMinute;

        public string Accuracy => $"{Math.Round(_progress.AccuracyPercent, 1)}%";

        public int TotalMistakes => _progress.TotalMistakes;

        public string TimeRemaining
        {
            get
            {
                if (_progress.TimerExpired)
                {
                    return "00:00";
                }

                var timeSpan = TimeSpan.FromSeconds(_progress.TimeRemaining);
                return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
        }
        public string ResultTitle => _progress.Score > 0 ? "Les Voltooid!" : "Les Gefaald";

        public bool CanShare => _xAuthService != null;

        // Method to get the result for download functionality
        public Result GetResult() => _progress;

        [RelayCommand]
        private async Task ShareToTwitter()
        {
            if (!CanShare || _xAuthService == null || _xSettings == null)
                return;

            try
            {
                IsSharing = true;

                var authCode = await AuthenticateUserAsync();
                if (string.IsNullOrEmpty(authCode))
                {
                    IsSharing = false;
                    return;
                }

                var accessToken = await ExchangeAuthCodeAsync(authCode);
                if (string.IsNullOrEmpty(accessToken))
                {
                    await ShowErrorAsync("Authenticatie mislukt", "Kon geen toegangstoken verkrijgen.");
                    IsSharing = false;
                    return;
                }

                await ShowPreviewPopupAsync();
                IsSharing = false;
            }
            catch (Exception ex)
            {
                IsSharing = false;
                await ShowErrorAsync("Fout", $"Er is een fout opgetreden: {ex.Message}");
            }
        }

        private async Task<string?> AuthenticateUserAsync()
        {
            if (_xAuthService == null || _xSettings == null) return null;

            var authCode = await _xAuthService.AuthenticateAsync(
                _xSettings.ClientId,
                _xSettings.RedirectUri,
                _xSettings.Scopes,
                forceConsent: true);

            await Task.Delay(500);
            return authCode;
        }

        private async Task<string?> ExchangeAuthCodeAsync(string authCode)
        {
            if (_xAuthService == null || _xSettings == null) return null;

            var accessToken = await _xAuthService.ExchangeCodeForTokenAsync(
                _xSettings.ClientId,
                authCode,
                _xSettings.RedirectUri);

            await Task.Delay(200);
            return accessToken;
        }

        private async Task ShowPreviewPopupAsync()
        {
            if (_mediaUploadRepository == null || 
                _xAuthRepository == null || _shareImageRepository == null || _xSettings == null)
                return;

            var text = $"{_lessonName} voltooid! Score: {Score} - APM: {StrokesPerMinute} - Nauwkeurigheid: {Accuracy} #BolType";
            
            // Load logo stream from app package
            System.IO.Stream? logoStream = null;
            try
            {
                logoStream = await Microsoft.Maui.Storage.FileSystem.OpenAppPackageFileAsync("boltype.png");
            }
            catch
            {
                // Logo not found, will use fallback
            }

            SKBitmap bitmap;
            if (logoStream != null)
            {
                using (logoStream)
                {
                    bitmap = ShareImageGenerator.GenerateWithResults(_lessonName, _progress, logoStream);
                }
            }
            else
            {
                bitmap = ShareImageGenerator.GenerateWithResults(_lessonName, _progress);
            }

            var sharePreviewViewModel = new SharePreviewViewModel(
                _mediaUploadRepository,
                _xAuthRepository,
                _shareImageRepository,
                _xSettings,
                _lessonName,
                $"Score: {Score}",
                _progress.LessonId,
                bitmap,
                text);

            var preview = new Views.ImagePreviewPopup(sharePreviewViewModel);
            var page = Shell.Current?.CurrentPage ?? Application.Current?.MainPage;

            if (page != null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await page.ShowPopupAsync(preview);
                });
            }
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