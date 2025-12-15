using FoutloosTypen.Core.Models;
using FoutloosTypen.ViewModels;
using Microsoft.Maui.Controls;

namespace FoutloosTypen.Views
{
    public partial class ResultatenPopUp : ContentPage
    {
        private TaskCompletionSource<bool> _userResponseTcs;

        public ResultatenPopUp(LessonProgress progress)
        {
            InitializeComponent();
            
            // Set the BindingContext to LessonResultViewModel
            BindingContext = new LessonResultViewModel(progress);
            
            _userResponseTcs = new TaskCompletionSource<bool>();
        }

        /// <summary>
        /// Wacht op gebruikersrespons.
        /// Returns true als "Ga verder" wordt geklikt, false als "Herstart" wordt geklikt.
        /// </summary>
        public Task<bool> WaitForUserResponseAsync()
        {
            return _userResponseTcs.Task;
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            // Signal dat de gebruiker op "Ga verder" heeft geklikt (true)
            _userResponseTcs.TrySetResult(true);
            await Navigation.PopModalAsync();
        }

        private async void OnRestartClicked(object sender, EventArgs e)
        {
            // Signal dat de gebruiker op "Herstart" heeft geklikt (false)
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
            // Voorkom dat de gebruiker de popup kan sluiten met back button
            return true;
        }
    }
}