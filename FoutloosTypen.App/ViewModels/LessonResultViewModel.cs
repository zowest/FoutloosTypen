using System.Diagnostics;
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
﻿using CommunityToolkit.Mvvm.ComponentModel;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;

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

        public LessonResultViewModel(Result progress)
        {
            _progress = progress;
            _lessonName = string.Empty;
        }

        public int Score => _progress.Score;
        public int StrokesPerMinute => _progress.StrokesPerMinute;
        public int WordsPerMinute => _progress.WordsPerMinute;
        public string Accuracy => $"{Math.Round(_progress.AccuracyPercent, 1)}%";
        public int TotalMistakes => _progress.TotalMistakes;
        private readonly Result _currentResult;
        private readonly ScoreComparison? _comparison;

        public Result CurrentResult => _currentResult;
        public ScoreComparison? Comparison => _comparison;

        // Basic Result Properties
        public string LessonName { get; set; } = string.Empty;
        public int Score => _currentResult.Score;
        public int WordsPerMinute => _currentResult.WordsPerMinute;
        public string Accuracy => $"{_currentResult.AccuracyPercent:F1}%";
        public int StrokesPerMinute => _currentResult.StrokesPerMinute;
        public int TotalMistakes => _currentResult.TotalMistakes;
        public string TimeRemaining => FormatTime(_currentResult.TimeRemaining);

        // Title
        public string ResultTitle => _comparison?.IsFirstAttempt == true
            ? "Eerste Poging!"
            : (_comparison?.IsNewPersonalBest == true ? "Nieuw Record!" : "Les Voltooid!");

        // Badge visibility
        public bool ShowFirstAttemptBadge => _comparison?.IsFirstAttempt == true;
        public bool ShowComparisonBadge => _comparison?.IsNewPersonalBest == true && _comparison?.IsFirstAttempt == false;
        public bool HasPreviousAttempt => _comparison?.PreviousBest != null && !(_comparison?.IsFirstAttempt == true);

        // Previous Best Properties
        public int PreviousBestScore => _comparison?.PreviousBest?.Score ?? 0;
        public int PreviousBestWPM => _comparison?.PreviousBest?.WordsPerMinute ?? 0;
        public string PreviousBestAccuracy => $"{_comparison?.PreviousBest?.AccuracyPercent ?? 0:F1}%";

        // Difference Properties with Colors
        public string ScoreDifferenceText
        {
            get
            {
                if (_comparison == null || _comparison.IsFirstAttempt) return string.Empty;
                var diff = _comparison.ScoreDifference;
                if (diff == 0) return string.Empty;
                return diff > 0 ? $"+{diff}" : $"{diff}";
            }
        }

        public Color ScoreDifferenceColor
        {
            get
            {
                if (_comparison == null) return Colors.Gray;
                return _comparison.ScoreDifference > 0 ? Colors.Green :
                       _comparison.ScoreDifference < 0 ? Colors.Red : Colors.Gray;
            }
        }

        public string SpeedDifferenceText
        {
            get
            {
                if (_comparison == null || _comparison.IsFirstAttempt) return string.Empty;
                var diff = _comparison.SpeedDifference;
                if (diff == 0) return string.Empty;
                return diff > 0 ? $"+{diff}" : $"{diff}";
            }
        }

        public Color SpeedDifferenceColor
        {
            get
            {
                if (_comparison == null) return Colors.Gray;
                return _comparison.SpeedDifference > 0 ? Colors.Green :
                       _comparison.SpeedDifference < 0 ? Colors.Red : Colors.Gray;
            }
        }

        public string ResultTitle => _progress.Score > 0 ? "Les Voltooid!" : "Les Gefaald";
        public bool CanShare => _xAuthService != null; // keep enabled; validation happens before consent

        public Result GetResult() => _progress;

        [RelayCommand]
        private async Task ShareToTwitter()
        {
            Debug.WriteLine("ShareToTwitterCommand aangeroepen");

            if (_xAuthService == null)
            {
                await ShowErrorAsync("Delen niet mogelijk", "De X/Twitter service is niet beschikbaar.");
                return;
            }

            if (!AreSettingsValid(out var settingsError))
            {
                // Show config error BEFORE any permission prompt
                await ShowErrorAsync("Configuratie fout", settingsError);
                return;
            }

            try
            {
                IsSharing = true;
                Debug.WriteLine("Start authenticatie");

                var authCode = await AuthenticateUserAsync();
                Debug.WriteLine("authCode: " + authCode);

                if (string.IsNullOrEmpty(authCode))
                {
                    Debug.WriteLine("Geen toestemming verleend of authenticatie afgebroken");
                    await ShowErrorAsync("Authenticatie geannuleerd", "Geen toestemming verleend.");
                    IsSharing = false;
                    return;
                }

                var accessToken = await ExchangeAuthCodeAsync(authCode);
                Debug.WriteLine("accessToken: " + accessToken);

                if (string.IsNullOrEmpty(accessToken))
                {
                    Debug.WriteLine("Kon geen toegangstoken verkrijgen");
                    await ShowErrorAsync("Authenticatie mislukt", "Kon geen toegang verkrijgen.");
                    IsSharing = false;
                    return;
                }

                Debug.WriteLine("Toon preview popup");
                await ShowPreviewPopupAsync();
                IsSharing = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fout in ShareToTwitter: " + ex);
                IsSharing = false;
                await ShowErrorAsync("Fout", $"Er is een fout opgetreden: {ex.Message}");
            }
        }

        private bool AreSettingsValid(out string error)
        {
            if (_xSettings == null)
            {
                error = "X/Twitter instellingen ontbreken.";
                return false;
            }

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(_xSettings.ClientId)) missing.Add("ClientId");
            if (string.IsNullOrWhiteSpace(_xSettings.RedirectUri)) missing.Add("RedirectUri");
            if (_xSettings.Scopes == null || _xSettings.Scopes.Length == 0) missing.Add("Scopes");

            if (missing.Count > 0)
            {
                error = $"X/Twitter is op dit moment niet beschikbaar.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private async Task<string?> AuthenticateUserAsync()
        {
            Debug.WriteLine("AuthenticateUserAsync aangeroepen");

            // Extra guard: never open consent if settings are invalid
            if (!AreSettingsValid(out var settingsError))
            {
                Debug.WriteLine("Instellingen ongeldig: " + settingsError);
                await ShowErrorAsync("Configuratie fout", settingsError);
                return null;
            }

            if (_xAuthService == null || _xSettings == null)
            {
                Debug.WriteLine("_xAuthService of _xSettings is null");
                return null;
            }

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
            if (_xAuthService == null || _xSettings == null)
            {
                return null;
            }

            var accessToken = await _xAuthService.ExchangeCodeForTokenAsync(
                _xSettings.ClientId,
                authCode,
                _xSettings.RedirectUri);

            Debug.WriteLine("ExchangeAuthCodeAsync resultaat: " + accessToken);
            await Task.Delay(200);
            return accessToken;
        }

        private async Task ShowPreviewPopupAsync()
        {
            Debug.WriteLine("ShowPreviewPopupAsync aangeroepen");
            if (_mediaUploadRepository == null ||
                _xAuthRepository == null || _shareImageRepository == null || _xSettings == null)
            {
                Debug.WriteLine("Een dependency is null: _mediaUploadRepository=" + (_mediaUploadRepository != null) +
                    ", _xAuthRepository=" + (_xAuthRepository != null) +
                    ", _shareImageRepository=" + (_shareImageRepository != null) +
                    ", _xSettings=" + (_xSettings != null));
                return;
            }

            var text = $"Net weer een typ-run gedaan�{_lessonName}! Score: {Score} - APM: {StrokesPerMinute} - Nauwkeurigheid: {Accuracy} #BolType";
            Debug.WriteLine("Share tekst: " + text);

            System.IO.Stream? logoStream = null;
            try
            {
                logoStream = await Microsoft.Maui.Storage.FileSystem.OpenAppPackageFileAsync("boltype.png");
                Debug.WriteLine("Logo geladen");
            }
            catch
            {
                Debug.WriteLine("Logo niet gevonden, fallback");
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
                Debug.WriteLine("Popup tonen op pagina: " + page.GetType().Name);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await page.ShowPopupAsync(preview);
                });
            }
            else
            {
                Debug.WriteLine("Geen geldige pagina gevonden voor popup");
            }
        }

        private async Task ShowErrorAsync(string title, string message)
        {
            Debug.WriteLine($"ShowErrorAsync: {title} - {message}");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                }
                else
                {
                    Debug.WriteLine("Geen MainPage beschikbaar voor DisplayAlert");
                }
            });

        public string AccuracyDifferenceText
        {
            get
            {
                if (_comparison == null || _comparison.IsFirstAttempt) return string.Empty;
                var diff = _comparison.AccuracyDifference;
                if (Math.Abs(diff) < 0.1) return string.Empty;
                return diff > 0 ? $"+{diff:F1}%" : $"{diff:F1}%";
            }
        }

        public Color AccuracyDifferenceColor
        {
            get
            {
                if (_comparison == null) return Colors.Gray;
                return _comparison.AccuracyDifference > 0 ? Colors.Green :
                       _comparison.AccuracyDifference < 0 ? Colors.Red : Colors.Gray;
            }
        }

        public LessonResultViewModel(Result currentResult, ScoreComparison? comparison = null)
        {
            _currentResult = currentResult ?? throw new ArgumentNullException(nameof(currentResult));
            _comparison = comparison;
        }

        private string FormatTime(double seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s";
            return $"{(int)seconds}s";
        }
    }
}