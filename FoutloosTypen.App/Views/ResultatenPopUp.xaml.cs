using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using FoutloosTypen.ViewModels;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Interfaces.Repositories;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;

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

        public ResultatenPopUp(
            Result progress,
            string lessonName,
            IXAuthService xAuthService,
            IMediaUploadRepository mediaUploadRepository,
            IXAuthRepository xAuthRepository,
            IXAuthApiRepository xAuthApiRepository,
            IShareImageRepository shareImageRepository,
            XAuthSettings xSettings)
        {
            InitializeComponent();

            var services = Application.Current?.Handler?.MauiContext?.Services;
            var globalViewModel = services?.GetService<GlobalViewModel>();

            _viewModel = globalViewModel != null
                ? new LessonResultViewModel(
                    progress,
                    lessonName,
                    xAuthService,
                    mediaUploadRepository,
                    xAuthRepository,
                    xAuthApiRepository,
                    shareImageRepository,
                    xSettings,
                    globalViewModel)
                : new LessonResultViewModel(progress); 

            BindingContext = _viewModel;
            _userResponseTcs = new TaskCompletionSource<PopupResult>();
            
            // Load current X account info
            _ = _viewModel.LoadCurrentXAccountAsync();
        }

        public ResultatenPopUp(Result progress)
        {
            InitializeComponent();

            var services = Application.Current?.Handler?.MauiContext?.Services;

            var xAuthService = services?.GetService<IXAuthService>();
            var mediaUploadRepository = services?.GetService<IMediaUploadRepository>();
            var xAuthRepository = services?.GetService<IXAuthRepository>();
            var xAuthApiRepository = services?.GetService<IXAuthApiRepository>();
            var shareImageRepository = services?.GetService<IShareImageRepository>();
            var xSettings = services?.GetService<XAuthSettings>();
            var globalViewModel = services?.GetService<GlobalViewModel>();

            if (xAuthService != null &&
                mediaUploadRepository != null &&
                xAuthRepository != null &&
                xAuthApiRepository != null &&
                shareImageRepository != null &&
                xSettings != null &&
                globalViewModel != null)
            {
                _viewModel = new LessonResultViewModel(
                    progress,
                    string.Empty,
                    xAuthService,
                    mediaUploadRepository,
                    xAuthRepository,
                    xAuthApiRepository,
                    shareImageRepository,
                    xSettings,
                    globalViewModel);
            }
            else
            {
                _viewModel = new LessonResultViewModel(progress);
            }

            BindingContext = _viewModel;
            _userResponseTcs = new TaskCompletionSource<PopupResult>();
            
            // Load current X account info
            _ = _viewModel.LoadCurrentXAccountAsync();
        }

        public ResultatenPopUp(
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
            InitializeComponent();

            _viewModel = new LessonResultViewModel(
                progress,
                lessonName,
                xAuthService,
                mediaUploadRepository,
                xAuthRepository,
                xAuthApiRepository,
                shareImageRepository,
                xSettings,
                globalViewModel);

            BindingContext = _viewModel;
            _userResponseTcs = new TaskCompletionSource<PopupResult>();
            
            // Load current X account info
            _ = _viewModel.LoadCurrentXAccountAsync();
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
