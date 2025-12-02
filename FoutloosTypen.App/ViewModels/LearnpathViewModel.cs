using System.Threading.Tasks;

namespace FoutloosTypen.ViewModels
{
    public class LearnpathViewModel : BaseViewModel
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
    }
}
