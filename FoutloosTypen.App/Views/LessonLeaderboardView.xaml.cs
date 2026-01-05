using FoutloosTypen.ViewModels;

namespace FoutloosTypen.Views
{
    public partial class LessonLeaderboardView : ContentPage
    {
        private readonly LessonLeaderboardViewModel _vm;

        public LessonLeaderboardView(LessonLeaderboardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _vm = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm?.OnAppearing();
        }
    }
}