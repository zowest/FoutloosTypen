using Microsoft.Maui.Controls;

namespace FoutloosTypen.Views
{
    public partial class LessonCompletedPopup : ContentPage
    {
        private TaskCompletionSource<bool> _userResponseTcs;

        public LessonCompletedPopup()
        {
            InitializeComponent();
            _userResponseTcs = new TaskCompletionSource<bool>();
        }

        public Task<bool> WaitForUserResponseAsync()
        {
            return _userResponseTcs.Task;
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            // Signal dat de gebruiker heeft geklikt
            _userResponseTcs.TrySetResult(true);
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

        private void Button_Clicked(object sender, EventArgs e)
        {

        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {

        }
    }
}