using FoutloosTypen.ViewModels;
using Microsoft.Maui.Controls;

namespace FoutloosTypen.Views
{
    public partial class LessonView : ContentPage
    {
        private readonly LessonViewModel? _vm;

        // Parameterless ctor required by XAML tooling / generated partial class
        public LessonView()
        {
            InitializeComponent();
        }

        public LessonView(LessonViewModel vm) : this()
        {
            BindingContext = _vm = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_vm is not null)
                await _vm.OnAppearingAsync();
        }
    }
}