using FoutloosTypen.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Diagnostics;

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

        private async void OnPlayClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Navigating to AssignmentView...");
                await Shell.Current.GoToAsync(nameof(AssignmentView));
                Debug.WriteLine("Navigation successful!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation failed: {ex.Message}");
                Debug.WriteLine($"Exception type: {ex.GetType().Name}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Show error to user
                await DisplayAlert("Navigation Error", $"Could not navigate to assignment: {ex.Message}", "OK");
            }
        }
    }
}