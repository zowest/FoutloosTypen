using FoutloosTypen.ViewModels;
using Microsoft.Maui.Controls; // Add this if missing

namespace FoutloosTypen.Views // Fix typo in namespace
{
    public partial class LessonView : ContentPage
    {
        private readonly LessonViewModel _vm;

        public LessonView(LessonViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.OnAppearingAsync();
        }
    }
}
