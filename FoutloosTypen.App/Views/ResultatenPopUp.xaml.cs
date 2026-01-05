using FoutloosTypen.Core.Interfaces.Services;
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

        public ResultatenPopUp(Result currentResult, ScoreComparison? comparison = null)
        {
            InitializeComponent();
            
            BindingContext = new LessonResultViewModel(currentResult, comparison);
            
            _userResponseTcs = new TaskCompletionSource<bool>();
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
