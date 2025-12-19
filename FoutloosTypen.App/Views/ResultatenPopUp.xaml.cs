using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using FoutloosTypen.ViewModels;
using Microsoft.Maui.Controls;

namespace FoutloosTypen.Views
{
    public partial class ResultatenPopUp : ContentPage
    {
        private TaskCompletionSource<bool> _userResponseTcs;

        public ResultatenPopUp(Result currentResult, ScoreComparison? comparison = null)
        {
            InitializeComponent();
            
            BindingContext = new LessonResultViewModel(currentResult, comparison);
            
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

        private void OnHoverEnter(object sender, PointerEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.BackgroundColor = Colors.LightGray;
            }
        }

        private void OnHoverExit(object sender, PointerEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.BackgroundColor = Colors.White;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}