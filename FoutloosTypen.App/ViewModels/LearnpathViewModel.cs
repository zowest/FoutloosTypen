using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Views;

namespace FoutloosTypen.ViewModels
{
    public partial class LearnpathViewModel : BaseViewModel
    {
        public CoursesViewModel CoursesVM { get; }
        public LessonViewModel LessonsVM { get; }

        public LearnpathViewModel(CoursesViewModel coursesVM, LessonViewModel lessonsVM)
        {
            CoursesVM = coursesVM;
            LessonsVM = lessonsVM;

            CoursesVM.CourseSelected += async (courseId) =>
            {
                await LessonsVM.LoadLessonsForCourseAsync(courseId);
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
    }
}
