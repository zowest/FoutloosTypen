using FoutloosTypen.ViewModels;
using Microsoft.Maui.Controls;

namespace FoutloosTypen.Views
{
    public partial class LeaderboardView : ContentPage
    {
        private readonly LeaderboardViewModel _vm;

        public LeaderboardView(LeaderboardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _vm = viewModel;

            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                IsVisible = false,
                IsEnabled = false
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm?.OnAppearing();
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LessonView");
        }
    }
}