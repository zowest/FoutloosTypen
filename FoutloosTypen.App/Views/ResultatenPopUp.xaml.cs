using System;
using FoutloosTypen.Core.Models;
using FoutloosTypen.ViewModels;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Interfaces.Repositories;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection; // added

namespace FoutloosTypen.Views
{
    public partial class ResultatenPopUp : ContentPage
    {
        private TaskCompletionSource<PopupResult> _userResponseTcs;
        private readonly LessonResultViewModel _viewModel;

        public enum PopupResult
        {
            Home,
            Restart,
            NextLesson
        }

        // Constructor met share functionaliteit
        public ResultatenPopUp(
            Result progress,
            string lessonName,
            IXAuthService xAuthService,
            IMediaUploadRepository mediaUploadRepository,
            IXAuthRepository xAuthRepository,
            IShareImageRepository shareImageRepository,
            XAuthSettings xSettings)
        {
            InitializeComponent();

            _viewModel = new LessonResultViewModel(
                progress,
                lessonName,
                xAuthService,
                mediaUploadRepository,
                xAuthRepository,
                shareImageRepository,
                xSettings);

            BindingContext = _viewModel;
            _userResponseTcs = new TaskCompletionSource<PopupResult>();
        }

        // Constructor zonder expliciete parameters: probeer DI te gebruiken voor share
        public ResultatenPopUp(Result progress)
        {
            InitializeComponent();

            // Try to resolve sharing dependencies from DI; if unavailable, fall back
            var services = Application.Current?.Handler?.MauiContext?.Services;

            var xAuthService = services?.GetService<IXAuthService>();
            var mediaUploadRepository = services?.GetService<IMediaUploadRepository>();
            var xAuthRepository = services?.GetService<IXAuthRepository>();
            var shareImageRepository = services?.GetService<IShareImageRepository>();
            var xSettings = services?.GetService<XAuthSettings>();

            if (xAuthService != null &&
                mediaUploadRepository != null &&
                xAuthRepository != null &&
                shareImageRepository != null &&
                xSettings != null)
            {
                // No lesson name available here; use empty string
                _viewModel = new LessonResultViewModel(
                    progress,
                    string.Empty,
                    xAuthService,
                    mediaUploadRepository,
                    xAuthRepository,
                    shareImageRepository,
                    xSettings);
            }
            else
            {
                // Fallback: still functional UI, share disabled
                _viewModel = new LessonResultViewModel(progress);
            }

            BindingContext = _viewModel;
            _userResponseTcs = new TaskCompletionSource<PopupResult>();
        }

        public Task<PopupResult> WaitForUserResponseAsync()
        {
            return _userResponseTcs.Task;
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            _userResponseTcs.TrySetResult(PopupResult.Home);
            await Navigation.PopModalAsync();
        }

        private async void OnRestartClicked(object sender, EventArgs e)
        {
            _userResponseTcs.TrySetResult(PopupResult.Restart);
            await Navigation.PopModalAsync();
        }

        private async void OnNextLessonClicked(object sender, EventArgs e)
        {
            _userResponseTcs.TrySetResult(PopupResult.NextLesson);
            await Navigation.PopModalAsync();
        }

        private async void OnShareClicked(object sender, EventArgs e)
        {
            if (_viewModel.CanShare)
            {
                await _viewModel.ShareToTwitterCommand.ExecuteAsync(null);
            }
        }

        private void OnHoverEnter(object sender, PointerEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.BackgroundColor = Colors.LightGray;
            }
            else if (sender is ImageButton imgBtn)
            {
                imgBtn.Opacity = 0.7;
            }
        }

        private void OnHoverExit(object sender, PointerEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.BackgroundColor = Colors.White;
            }
            else if (sender is ImageButton imgBtn)
            {
                imgBtn.Opacity = 1.0;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
