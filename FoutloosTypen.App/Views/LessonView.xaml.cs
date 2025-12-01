using FoutloosTypen.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Diagnostics;

namespace FoutloosTypen.Views
{
    public partial class LessonView : ContentPage
    {
        private readonly LearnpathViewModel _vm;

        public LessonView()
        {
            InitializeComponent();
        }

        public LessonView(LearnpathViewModel vm) : this()
        {
            BindingContext = _vm = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_vm != null)
                await _vm.OnAppearingAsync();
        }
        
        public void SetHoverButton(Button button)
        {
            HoverButton = button;
        }
        
        private void OnHoverEnter(object sender, PointerEventArgs e)
        {
            switch (sender)
            {
                case Button btn:
                    btn.BackgroundColor = Colors.LightGray;
                    break;

                case Border border:
                    border.Background = new SolidColorBrush(Colors.LightGray);
                    break;

                case Label lbl:
                    lbl.BackgroundColor = Colors.LightGray;
                    break;
            }
        }

        private void OnHoverExit(object sender, PointerEventArgs e)
        {
            switch (sender)
            {
                case Button btn:
                    btn.BackgroundColor = Colors.White;
                    break;

                case Border border:
                    border.Background = new SolidColorBrush(Colors.White);
                    break;

                case Label lbl:
                    lbl.BackgroundColor = Colors.White;
                    break;
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
