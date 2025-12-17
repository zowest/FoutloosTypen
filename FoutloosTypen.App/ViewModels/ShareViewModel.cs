using System;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using Microsoft.Maui.ApplicationModel;
using SkiaSharp;

namespace FoutloosTypen.ViewModels
{
    public partial class ShareViewModel : ObservableObject
    {
        private readonly IXAuthService _xAuthService;
        private readonly IMediaUploadRepository _mediaUploadRepository;
        private readonly IXAuthRepository _xAuthRepository;
        private readonly IShareImageRepository _shareImageRepository;
        private readonly XAuthSettings _xSettings;
        private readonly ISharePostRepository _sharePostRepository;
        private readonly IShareImageService _shareImageService;

        [ObservableProperty]
        private bool _isShareOptionsVisible;

        [ObservableProperty]
        private bool _showShareButton = true;

        public ShareViewModel(
            IXAuthService xAuthService,
            IMediaUploadRepository mediaUploadRepository,
            IXAuthRepository xAuthRepository,
            IShareImageRepository shareImageRepository,
            XAuthSettings xSettings,
            ISharePostRepository sharePostRepository,
            IShareImageService shareImageService)
        {
            _xAuthService = xAuthService;
            _mediaUploadRepository = mediaUploadRepository;
            _xAuthRepository = xAuthRepository;
            _shareImageRepository = shareImageRepository;
            _xSettings = xSettings;
            _sharePostRepository = sharePostRepository;
            _shareImageService = shareImageService;
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
                var authCode = await AuthenticateUserAsync();
                if (string.IsNullOrEmpty(authCode))
                {
                    IsShareOptionsVisible = false;
                    return;
                }

                var accessToken = await ExchangeAuthCodeAsync(authCode);
                if (string.IsNullOrEmpty(accessToken))
                {
                    await ShowErrorAsync("Authenticatie mislukt", "Kon geen toegangstoken verkrijgen.");
                    IsShareOptionsVisible = false;
                    return;
                }

                await ShowPreviewPopupAsync(assignmentVM);
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
            var accessToken = await _xAuthService.ExchangeCodeForTokenAsync(
                _xSettings.ClientId,
                authCode,
                _xSettings.RedirectUri);

            await Task.Delay(200);
            return accessToken;
        }

        private async Task ShowPreviewPopupAsync(AssignmentViewModel assignmentVM)
        {
            var lessonName = assignmentVM.SelectedLesson?.Name ?? "Onbekende les";
            var progressText = assignmentVM.ProgressText;
            var lessonId = assignmentVM.SelectedLesson?.Id ?? 0;

            var text = _shareImageService.BuildTweetText(lessonName, progressText);
            
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
                    bitmap = _shareImageService.Generate(lessonName, progressText, logoStream);
                }
            }
            else
            {
                bitmap = _shareImageService.Generate(lessonName, progressText);
            }

            var sharePreviewViewModel = new SharePreviewViewModel(
                _mediaUploadRepository,
                _xAuthRepository,
                _shareImageRepository,
                _xSettings,
                _sharePostRepository,
                lessonName,
                progressText,
                lessonId,
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
