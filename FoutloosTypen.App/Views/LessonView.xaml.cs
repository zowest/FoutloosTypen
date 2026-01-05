using FoutloosTypen.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Diagnostics;

namespace FoutloosTypen.Views
{
    public partial class LessonView : ContentPage
    {
        private readonly LearnpathViewModel? _vm;
        private readonly GlobalViewModel? _globalViewModel;
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

        private async void OnEndlessModeClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(EndlessModeView));
        }

        private async void OnTrophyClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("[LessonView] Trophy button clicked - navigating to LeaderboardView");
                await Shell.Current.GoToAsync($"///{nameof(LeaderboardView)}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LessonView] Trophy navigation failed: {ex.Message}");
                await DisplayAlert("Navigatie fout", "Kon niet naar klassement navigeren", "OK");
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("[LessonView] Settings button clicked - navigating to SettingsView");
                await Shell.Current.GoToAsync(nameof(SettingsView));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LessonView] Settings navigation failed: {ex.Message}");
                await DisplayAlert("Navigatie fout", "Kon niet naar instellingen navigeren", "OK");
            }
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("[LessonView] Profile button clicked - navigating to ProfileView");
                await Shell.Current.GoToAsync(nameof(ProfileView));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LessonView] Profile navigation failed: {ex.Message}");
                await DisplayAlert("Navigatie fout", "Kon niet naar profiel navigeren", "OK");
            }
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

                case ImageButton imgBtn:
                    imgBtn.Opacity = 0.7;
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

                case ImageButton imgBtn:
                    imgBtn.Opacity = 1.0;
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