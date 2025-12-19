using System.Threading.Tasks;
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

        // New command to open the general leaderboard
        [RelayCommand]
        private async Task OpenLeaderboard()
        {
            await Shell.Current.GoToAsync("//LeaderboardView");
        }
    }
}
