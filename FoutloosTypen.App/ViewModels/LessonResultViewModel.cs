ï»¿using CommunityToolkit.Mvvm.ComponentModel;
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
        private readonly IXAuthApiRepository? _xAuthApiRepository;
        private readonly IShareImageRepository? _shareImageRepository;
        private readonly XAuthSettings? _xSettings;
        private readonly GlobalViewModel? _globalViewModel;
        private readonly string _lessonName;

        [ObservableProperty]
        private bool _isSharing;

        public LessonResultViewModel(
            Result progress,
            string lessonName,
            IXAuthService xAuthService,
            IMediaUploadRepository mediaUploadRepository,
            IXAuthRepository xAuthRepository,
            IXAuthApiRepository xAuthApiRepository,
            IShareImageRepository shareImageRepository,
            XAuthSettings xSettings,
            GlobalViewModel globalViewModel)
        {
            _progress = progress;
            _lessonName = lessonName;
            _xAuthService = xAuthService;
            _mediaUploadRepository = mediaUploadRepository;
            _xAuthRepository = xAuthRepository;
            _xAuthApiRepository = xAuthApiRepository;
            _shareImageRepository = shareImageRepository;
            _xSettings = xSettings;
            _globalViewModel = globalViewModel;
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

        public string TimeRemaining
        {
            get
            {
                if (_comparison == null || _comparison.IsFirstAttempt) return string.Empty;
                var diff = _comparison.ScoreDifference;
                if (diff == 0) return string.Empty;
                return diff > 0 ? $"+{diff}" : $"{diff}";
            }
        }

        public string ResultTitle => _progress.Score > 0 ? "Les Voltooid!" : "Les Gefaald";
        public bool CanShare => _xAuthService != null;

        [ObservableProperty]
        private string? _currentXUsername;

        public Result GetResult() => _progress;

        public async Task LoadCurrentXAccountAsync()
        {
            if (_globalViewModel == null || _xAuthRepository == null)
                return;

            int ownerUserId = _globalViewModel.Student?.Id ?? 0;
            if (ownerUserId == 0)
                return;

            var xUserInfo = await _xAuthRepository.GetXUserInfoAsync(ownerUserId);
            CurrentXUsername = xUserInfo != null ? $"@{xUserInfo.Value.XUsername}" : null;
        }

        [RelayCommand]
        private async Task SwitchXAccount()
        {
            Debug.WriteLine("SwitchXAccount aangeroepen");

            if (_xAuthService == null || _xAuthRepository == null)
            {
                await ShowErrorAsync("niet beschikbaar", "X/Twitter service is niet beschikbaar.");
                return;
            }

            int ownerUserId = _globalViewModel?.Student?.Id ?? 0;
            if (ownerUserId == 0)
            {
                await ShowErrorAsync("Fout", "Geen gebruiker ingelogd.");
                return;
            }

            try
            {
                // Delete existing tokens and X user info
                await _xAuthRepository.DeleteOAuth1TokensAsync(ownerUserId);
                await _xAuthRepository.DeleteXUserInfoAsync(ownerUserId);
                
                CurrentXUsername = null;

                Debug.WriteLine($"Tokens verwijderd voor ownerUserId={ownerUserId}, nieuwe OAuth flow vereist");
                
                await ShowErrorAsync("X-account ontkoppeld", "Klik op 'Delen op X' om een ander X-account te koppelen.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fout in SwitchXAccount: {ex}");
                await ShowErrorAsync("Fout", $"Kon X-account niet ontkoppelen: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ShareToTwitter()
        {
            Debug.WriteLine("ShareToTwitterCommand aangeroepen");

            if (_xAuthService == null || _xAuthApiRepository == null)
            {
                await ShowErrorAsync("Delen niet mogelijk", "De X/Twitter service is niet beschikbaar.");
                return;
            }

            if (!AreSettingsValid(out var settingsError))
            {
                await ShowErrorAsync("Configuratie fout", settingsError);
                return;
            }

            try
            {
                IsSharing = true;
                Debug.WriteLine("Start authenticatie");

                // Get logged-in student ID from GlobalViewModel
                int ownerUserId = _globalViewModel?.Student?.Id ?? 0;
                if (ownerUserId == 0)
                {
                    await ShowErrorAsync("Fout", "Geen gebruiker ingelogd. Log eerst in om te kunnen delen.");
                    IsSharing = false;
                    return;
                }

                Debug.WriteLine($"Using ownerUserId={ownerUserId} for OAuth tokens");

                // Check for per-user OAuth tokens
                var tokens = await _xAuthRepository.GetOAuth1TokensAsync(ownerUserId);

                if (tokens == null)
                {
                    Debug.WriteLine("Geen tokens gevonden, start OAuth flow");
                    var authResult = await AuthenticateUserAsync();
                    if (string.IsNullOrEmpty(authResult))
                    {
                        Debug.WriteLine("Geen toestemming verleend of authenticatie afgebroken");
                        await ShowErrorAsync("Authenticatie geannuleerd", "Geen toestemming verleend.");
                        IsSharing = false;
                        return;
                    }

                    Debug.WriteLine($"AuthResult ontvangen: {authResult}");
                    var parts = authResult.Split('|');
                    if (parts.Length != 3)
                    {
                        Debug.WriteLine($"Ongeldige authResult format: {parts.Length} parts");
                        await ShowErrorAsync("Authenticatie fout", "Ongeldige authenticatie respons.");
                        IsSharing = false;
                        return;
                    }

                    Debug.WriteLine("Start token exchange");
                    var (accessToken, accessTokenSecret) = await _xAuthApiRepository.ExchangeRequestTokenAsync(
                        _xSettings.ConsumerKey,
                        _xSettings.ConsumerSecret,
                        parts[0],
                        parts[1],
                        parts[2]);

                    if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(accessTokenSecret))
                    {
                        Debug.WriteLine("Kon geen toegangstokens verkrijgen");
                        await ShowErrorAsync("Token exchange mislukt", "Kon geen toegang verkrijgen.");
                        IsSharing = false;
                        return;
                    }

                    Debug.WriteLine("Tokens ontvangen, verifieer credentials");
                    // Verify and save X user info
                    var xUserInfo = await _xAuthApiRepository.VerifyCredentialsAsync(
                        _xSettings.ConsumerKey,
                        _xSettings.ConsumerSecret,
                        accessToken,
                        accessTokenSecret);

                    if (xUserInfo == null)
                    {
                        Debug.WriteLine("Kon X gebruikersinformatie niet ophalen");
                        await ShowErrorAsync("Verificatie mislukt", "Kon X account niet verifiëren.");
                        IsSharing = false;
                        return;
                    }

                    Debug.WriteLine($"Sla tokens op voor ownerUserId={ownerUserId}");
                    await _xAuthService.SaveUserAccessTokensAsync(ownerUserId, accessToken, accessTokenSecret);
                    await _xAuthRepository.SaveXUserInfoAsync(ownerUserId, xUserInfo.Value.userId, xUserInfo.Value.username);
                    
                    CurrentXUsername = $"@{xUserInfo.Value.username}";
                    Debug.WriteLine($"Nieuwe X account gekoppeld: @{xUserInfo.Value.username} (ID: {xUserInfo.Value.userId})");
                }
                else
                {
                    Debug.WriteLine("Tokens gevonden, gebruik bestaande tokens");
                }

                Debug.WriteLine("Toon preview popup en deel");
                var shareSuccess = await ShowPreviewPopupAndShareAsync(ownerUserId);
                
                if (shareSuccess)
                {
                    // Open X profile in browser after successful share
                    var xUserInfo = await _xAuthRepository.GetXUserInfoAsync(ownerUserId);
                    if (xUserInfo != null && !string.IsNullOrEmpty(xUserInfo.Value.XUsername))
                    {
                        var profileUrl = $"https://twitter.com/{xUserInfo.Value.XUsername}";
                        Debug.WriteLine($"Open X profiel: {profileUrl}");
                        
                        try
                        {
                            await Launcher.OpenAsync(new Uri(profileUrl));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Kon X profiel niet openen: {ex.Message}");
                        }
                    }
                }
                
                IsSharing = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in ShareToTwitter: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                IsSharing = false;
                await ShowErrorAsync("Fout", $"Er is een fout opgetreden: {ex.Message}");
            }
        }

        public string SpeedDifferenceText
        {
            if (_xSettings == null)
            {
                error = "X/Twitter instellingen ontbreken.";
                return false;
            }

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(_xSettings.ConsumerKey)) missing.Add("ConsumerKey");
            if (string.IsNullOrWhiteSpace(_xSettings.ConsumerSecret)) missing.Add("ConsumerSecret");
            if (string.IsNullOrWhiteSpace(_xSettings.CallbackUrl)) missing.Add("CallbackUrl");

            if (missing.Count > 0)
            {
                if (_comparison == null || _comparison.IsFirstAttempt) return string.Empty;
                var diff = _comparison.SpeedDifference;
                if (diff == 0) return string.Empty;
                return diff > 0 ? $"+{diff}" : $"{diff}";
            }
        }

        public Color SpeedDifferenceColor
        {
            Debug.WriteLine("AuthenticateUserAsync aangeroepen");

            if (!AreSettingsValid(out var settingsError))
            {
                Debug.WriteLine("Instellingen ongeldig: " + settingsError);
                await ShowErrorAsync("Configuratie fout", settingsError);
                return null;
            }

            if (_xAuthService == null || _xSettings == null)
            {
                if (_comparison == null) return Colors.Gray;
                return _comparison.SpeedDifference > 0 ? Colors.Green :
                       _comparison.SpeedDifference < 0 ? Colors.Red : Colors.Gray;
            }

            var authResult = await _xAuthService.AuthenticateAsync(
                _xSettings.ConsumerKey,
                _xSettings.ConsumerSecret,
                _xSettings.CallbackUrl);
            await Task.Delay(500);
            return authResult;
        }

        private async Task<bool> ShowPreviewPopupAndShareAsync(int ownerUserId)
        {
            Debug.WriteLine("ShowPreviewPopupAndShareAsync aangeroepen");
            if (_mediaUploadRepository == null || _xAuthRepository == null || _shareImageRepository == null || _xSettings == null)
            {
                Debug.WriteLine($"Dependency nulls: media={_mediaUploadRepository!=null}, xRepo={_xAuthRepository!=null}, shareImg={_shareImageRepository!=null}, settings={_xSettings!=null}");
                return false;
            }

            // Use per-user OAuth tokens
            var tokens = await _xAuthRepository.GetOAuth1TokensAsync(ownerUserId);
            if (tokens == null)
            {
                Debug.WriteLine($"[XAuthRepository] No OAuth1 tokens found for ownerUserId={ownerUserId}");
                await ShowErrorAsync("Tokens ontbreken", "Geen OAuth1 tokens gevonden. Start eerst de OAuth1 autorisatie en voltooi de callback.");
                return false;
            }

            var text = $"Net weer een typ-run gedaan{_lessonName}! Score: {Score} - APM: {StrokesPerMinute} - Nauwkeurigheid: {Accuracy} #BolType";
            System.IO.Stream? logoStream = null;
            try { logoStream = await Microsoft.Maui.Storage.FileSystem.OpenAppPackageFileAsync("boltype.png"); } catch { }

            SKBitmap bitmap = logoStream != null
                ? ShareImageGenerator.GenerateWithResults(_lessonName, _progress, logoStream)
                : ShareImageGenerator.GenerateWithResults(_lessonName, _progress);

            var sharePreviewViewModel = new SharePreviewViewModel(
                _mediaUploadRepository,
                _xAuthRepository,
                _xAuthService,
                _shareImageRepository,
                _xSettings,
                _lessonName,
                $"Score: {Score}",
                _progress.LessonId,
                ownerUserId,
                bitmap,
                text);

            var preview = new Views.ImagePreviewPopup(sharePreviewViewModel);
            var page = Shell.Current?.CurrentPage ?? Application.Current?.MainPage;
            
            if (page != null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () => await page.ShowPopupAsync(preview));
                
                // Check if share was successful
                return sharePreviewViewModel.ShareSuccessful;
            }
            
            return false;
        }

        private string FormatTime(double seconds)
        {
            var page = Shell.Current?.CurrentPage ?? Application.Current?.MainPage;
            if (page != null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await page.DisplayAlert(title, message, "OK");
                });
            }
        }
    }
}
