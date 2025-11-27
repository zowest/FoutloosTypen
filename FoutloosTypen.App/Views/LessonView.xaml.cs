using FoutloosTypen.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace FoutloosTypen.Views
{
    public partial class LessonView : ContentPage
    {
        private readonly LessonViewModel? _vm;

        private Button? HoverButton;

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
        public void SetHoverButton(Button button)
        {
            HoverButton = button;
        }
        private void OnHoverEnter(object sender, PointerEventArgs e)
        {
            if (sender is Button button)
            {
                button.BackgroundColor = Colors.LightGrey;
            }
        }

        private void OnHoverExit(object sender, PointerEventArgs e)
        {
            if (sender is Button button)
            {
                button.BackgroundColor = Colors.White;
            }
        }

    }
}