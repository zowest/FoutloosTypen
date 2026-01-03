using System;
using System.Threading.Tasks;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Helpers;
using Microsoft.Maui.ApplicationModel;
using SkiaSharp;

namespace FoutloosTypen.ViewModels
{
    public partial class ShareViewModel : ObservableObject
    {
        private readonly IXAuthService _xAuthService;
        private readonly IMediaUploadRepository _mediaUploadRepository;
        private readonly IXAuthRepository _xAuthRepository;
        private readonly IXAuthApiRepository _xAuthApiRepository;
        private readonly IShareImageRepository _shareImageRepository;
        private readonly XAuthSettings _xSettings;
        private readonly GlobalViewModel _globalViewModel;

        [ObservableProperty]
        private bool _isShareOptionsVisible;

        [ObservableProperty]
        private bool _showShareButton = true;

        public ShareViewModel(
            IXAuthService xAuthService,
            IMediaUploadRepository mediaUploadRepository,
            IXAuthRepository xAuthRepository,
            IXAuthApiRepository xAuthApiRepository,
            IShareImageRepository shareImageRepository,
            XAuthSettings xSettings,
            GlobalViewModel globalViewModel)
        {
            _xAuthService = xAuthService;
            _mediaUploadRepository = mediaUploadRepository;
            _xAuthRepository = xAuthRepository;
            _xAuthApiRepository = xAuthApiRepository;
            _shareImageRepository = shareImageRepository;
            _xSettings = xSettings;
            _globalViewModel = globalViewModel;
        }

        [RelayCommand]
        private void ToggleShareOptions()
        {
            IsShareOptionsVisible = !IsShareOptionsVisible;
            ShowShareButton = !IsShareOptionsVisible;
        }

        [RelayCommand]
        private async Task ShareToTwitterAsync(object parameter)
        {
            if (parameter is not AssignmentViewModel assignmentVM)
                return;

            try
            {
                int ownerUserId = _globalViewModel?.Student?.Id ?? 0;
                if (ownerUserId == 0)
                {
                    await ShowErrorAsync("Fout", "Geen gebruiker ingelogd.");
                    IsShareOptionsVisible = false;
                    return;
                }

                var tokens = await _xAuthService.GetUserAccessTokensAsync(ownerUserId);
                bool needsReAuth = false;

                if (tokens != null)
                {
                    // Verify that the stored tokens still belong to the same X account
                    var currentXUser = await _xAuthApiRepository.VerifyCredentialsAsync(
                        _xSettings.ConsumerKey,
                        _xSettings.ConsumerSecret,
                        tokens.Value.AccessToken,
                        tokens.Value.AccessSecret);

                    if (currentXUser == null)
                    {
                        needsReAuth = true;
                    }
                    else
                    {
                        // Check if stored X user info matches current tokens
                        var storedXUser = await _xAuthRepository.GetXUserInfoAsync(ownerUserId);
                        
                        if (storedXUser == null || storedXUser.Value.XUserId != currentXUser.Value.userId)
                        {
                            needsReAuth = true;
                        }
                    }
                }

                if (tokens == null || needsReAuth)
                {
                    if (needsReAuth)
                    {
                        await _xAuthRepository.DeleteOAuth1TokensAsync(ownerUserId);
                        await _xAuthRepository.DeleteXUserInfoAsync(ownerUserId);
                    }

                    var authResult = await AuthenticateUserAsync();
                    if (string.IsNullOrEmpty(authResult))
                    {
                        await ShowErrorAsync("Authenticatie geannuleerd", "Je moet eerst inloggen om te delen.");
                        IsShareOptionsVisible = false;
                        return;
                    }

                    var parts = authResult.Split('|');
                    if (parts.Length != 3)
                    {
                        await ShowErrorAsync("Authenticatie fout", "Ongeldige authenticatie respons.");
                        IsShareOptionsVisible = false;
                        return;
                    }

                    var (accessToken, accessTokenSecret) = await _xAuthApiRepository.ExchangeRequestTokenAsync(
                        _xSettings.ConsumerKey,
                        _xSettings.ConsumerSecret,
                        parts[0],
                        parts[1],
                        parts[2]);

                    if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(accessTokenSecret))
                    {
                        await ShowErrorAsync("Token exchange mislukt", "Kon geen toegangstokens verkrijgen.");
                        IsShareOptionsVisible = false;
                        return;
                    }

                    // Verify and save X user info
                    var xUserInfo = await _xAuthApiRepository.VerifyCredentialsAsync(
                        _xSettings.ConsumerKey,
                        _xSettings.ConsumerSecret,
                        accessToken,
                        accessTokenSecret);

                    if (xUserInfo == null)
                    {
                        await ShowErrorAsync("Verificatie mislukt", "Kon X account niet verifiëren.");
                        IsShareOptionsVisible = false;
                        return;
                    }

                    await _xAuthService.SaveUserAccessTokensAsync(ownerUserId, accessToken, accessTokenSecret);
                    await _xAuthRepository.SaveXUserInfoAsync(ownerUserId, xUserInfo.Value.userId, xUserInfo.Value.username);
                    
                    tokens = (accessToken, accessTokenSecret);
                }

                await ShowPreviewPopupAsync(assignmentVM, ownerUserId);
                IsShareOptionsVisible = false;
            }
            catch (Exception ex)
            {
                IsShareOptionsVisible = false;
                await ShowErrorAsync("Fout", $"Er is een fout opgetreden: {ex.Message}");
            }
        }

        private async Task<string?> AuthenticateUserAsync()
        {
            var authResult = await _xAuthService.AuthenticateAsync(
                _xSettings.ConsumerKey,
                _xSettings.ConsumerSecret,
                _xSettings.CallbackUrl);

            await Task.Delay(500);
            return authResult;
        }

        private bool AreSettingsValid(out string error)
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
                error = $"X/Twitter is op dit moment niet beschikbaar.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private async Task ShowPreviewPopupAsync(AssignmentViewModel assignmentVM, int ownerUserId)
        {
            var lessonName = assignmentVM.SelectedLesson?.Name ?? "Onbekende les";
            var progressText = assignmentVM.ProgressText;
            var lessonId = assignmentVM.SelectedLesson?.Id ?? 0;

            var text = ShareImageGenerator.BuildTweetText(lessonName, progressText);
            
            System.IO.Stream? logoStream = null;
            try
            {
                logoStream = await Microsoft.Maui.Storage.FileSystem.OpenAppPackageFileAsync("boltype.png");
            }
            catch
            {
            }

            SKBitmap bitmap;
            if (logoStream != null)
            {
                using (logoStream)
                {
                    bitmap = ShareImageGenerator.Generate(lessonName, progressText, logoStream);
                }
            }
            else
            {
                bitmap = ShareImageGenerator.Generate(lessonName, progressText);
            }

            var sharePreviewViewModel = new SharePreviewViewModel(
                _mediaUploadRepository,
                _xAuthRepository,
                _xAuthService,
                _shareImageRepository,
                _xSettings,
                lessonName,
                progressText,
                lessonId,
                ownerUserId,
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
