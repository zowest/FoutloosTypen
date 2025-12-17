using FoutloosTypen.ViewModels;

namespace FoutloosTypen.Views
{
    public partial class LeaderboardView : ContentPage
    {
        private readonly LeaderboardViewModel _vm;

        public LeaderboardView(LeaderboardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _vm = viewModel;
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