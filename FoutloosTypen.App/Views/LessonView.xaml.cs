using FoutloosTypen.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Diagnostics;

namespace FoutloosTypen.Views
{
    public partial class LessonView : ContentPage
    {
        private readonly LearnpathViewModel _vm;
        private readonly GlobalViewModel _globalViewModel;
        private Button? HoverButton;


        public LessonView()
        {
            InitializeComponent();
        }

        public LessonView(LearnpathViewModel vm, GlobalViewModel globalViewModel) : this()
        {
            BindingContext = _vm = vm;
            _globalViewModel = globalViewModel;
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                IsVisible = false,
                IsEnabled = false
            });
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
                // Get the selected lesson from the ViewModel
                var selectedLesson = _vm?.LessonsVM?.SelectedLesson;
                
                // Check if TTS mode is enabled from the current user's profile
                bool useTtsMode = _globalViewModel?.Student?.UseTtsMode ?? false;
                string targetView = useTtsMode ? "TTSAssignmentView" : nameof(AssignmentView);
                
                if (selectedLesson != null)
                {
                    Debug.WriteLine($"Navigating to {targetView} with lesson ID: {selectedLesson.Id}");
                    await Shell.Current.GoToAsync($"{targetView}?lessonId={selectedLesson.Id}");
                }
                else
                {
                    Debug.WriteLine($"No lesson selected, navigating to {targetView} without parameter");
                    await Shell.Current.GoToAsync(targetView);
                }
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
