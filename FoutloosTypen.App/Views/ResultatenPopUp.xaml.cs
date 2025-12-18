using FoutloosTypen.Core.Models;
using FoutloosTypen.ViewModels;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Interfaces.Repositories;
using Microsoft.Maui.Controls;

namespace FoutloosTypen.Views
{
    public partial class ResultatenPopUp : ContentPage
    {
        private TaskCompletionSource<bool> _userResponseTcs;
        private readonly LessonResultViewModel _viewModel;

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
            _userResponseTcs = new TaskCompletionSource<bool>();
        }

        // Constructor zonder share functionaliteit (backward compatibility)
        public ResultatenPopUp(Result progress)
        {
            InitializeComponent();
            
            _viewModel = new LessonResultViewModel(progress);
            BindingContext = _viewModel;
            
            _userResponseTcs = new TaskCompletionSource<bool>();
        }

        public Task<bool> WaitForUserResponseAsync()
        {
            return _userResponseTcs.Task;
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            _userResponseTcs.TrySetResult(true);
            await Navigation.PopModalAsync();
            
            await Shell.Current.Navigation.PopToRootAsync();
        }

        private async void OnRestartClicked(object sender, EventArgs e)
        {
            _userResponseTcs.TrySetResult(false);
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