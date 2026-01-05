using System.Threading.Tasks;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Views;
using Microsoft.Maui.Controls;

namespace FoutloosTypen.ViewModels
{
    public partial class LearnpathViewModel : BaseViewModel
    {
        public CoursesViewModel CoursesVM { get; }
        public LessonViewModel LessonsVM { get; }
        public LeaderboardViewModel LeaderboardVM { get; }

        public LearnpathViewModel(CoursesViewModel coursesVM, LessonViewModel lessonsVM, LeaderboardViewModel leaderboardVM)
        {
            CoursesVM = coursesVM;
            LessonsVM = lessonsVM;
            LeaderboardVM = leaderboardVM;

            CoursesVM.CourseSelected += async (courseId) =>
            {
                await LessonsVM.LoadLessonsForCourseAsync(courseId);
            };

            // Update leaderboard when lesson changes
            LessonsVM.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(LessonsVM.SelectedLesson) && LessonsVM.SelectedLesson != null)
                {
                    LeaderboardVM.LessonId = LessonsVM.SelectedLesson.Id;
                }
            };
        }

        public async Task OnAppearingAsync()
        {
            await CoursesVM.LoadCoursesAsync();
        }

        [RelayCommand]
        private async Task Profile()
        {
            await Shell.Current.GoToAsync(nameof(ProfileView));
        }

        [RelayCommand]
        private async Task Settings()
        {
            await Shell.Current.GoToAsync(nameof(SettingsView));
        }

        // New command to open the general leaderboard
        [RelayCommand]
        private async Task OpenLeaderboard()
        {
            try
            {
                Debug.WriteLine("[LearnpathViewModel] OpenLeaderboard called");

                if (Shell.Current == null)
                {
                    Debug.WriteLine("[LearnpathViewModel] Shell.Current is null - cannot navigate");
                    return;
                }

                // Use registered route name (more robust than absolute '//' route)
                await Shell.Current.GoToAsync(nameof(LeaderboardView));
                Debug.WriteLine("[LearnpathViewModel] Navigation to LeaderboardView requested");
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[LearnpathViewModel] OpenLeaderboard navigation failed: {ex}");
            }
        }
    }
}
