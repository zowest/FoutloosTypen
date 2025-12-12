using Microsoft.Maui.Controls;

namespace FoutloosTypen.Views
{
    public partial class LessonCompletedPopup : ContentPage
    {
        public LessonCompletedPopup()
        {
            InitializeComponent();
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
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
    }
}