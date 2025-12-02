using System.Collections.ObjectModel;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Services;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

namespace FoutloosTypen.ViewModels
{
    public partial class LessonViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly ILessonService _lessonService;
        private readonly ICourseService _courseService;

        public ObservableCollection<Lesson> Lessons { get; set; } = new();
        public ObservableCollection<Course> Courses { get; set; } = new();

        private Lesson _selectedLesson = new();
        public Lesson SelectedLesson
        {
            get => _selectedLesson;
            set
            {
                _selectedLesson = value;
                OnPropertyChanged(nameof(SelectedLesson));
                Debug.WriteLine($"Selected lesson: {value?.Name}");
            }
        }

        private Course? _selectedCourse;
        public Course? SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                _selectedCourse = value;
                OnPropertyChanged(nameof(SelectedCourse));
                Debug.WriteLine($"Selected course: {value?.Name}");
                FilterLessonsByCourse();
            }
        }

        public LessonViewModel(ILessonService lessonService, ICourseService courseService)
        {
            _lessonService = lessonService;
            _courseService = courseService;
            Debug.WriteLine("LessonViewModel created");
        }

        public async Task OnAppearingAsync()
        {
            Debug.WriteLine("OnAppearingAsync called");
            
            // Load all courses from database
            var courseItems = _courseService.GetAll();
            Debug.WriteLine($"Courses retrieved: {courseItems?.Count ?? 0}");
            
            Courses.Clear();
            if (courseItems != null)
            {

                foreach (var course in courseItems)
                {
                    Courses.Add(course);
                    Debug.WriteLine($"Added course to collection: {course.Name}");
                }
            }

            // Select first course by default
            if (Courses.Any())
            {
                SelectedCourse = Courses.First();
            }
            else
            {
                Debug.WriteLine("WARNING: No courses found in database!");
            }
        }

        private void FilterLessonsByCourse()
        {
            if (SelectedCourse is null)
            {
                Debug.WriteLine("No course selected, skipping filter");
                return;
            }

            Debug.WriteLine($"Filtering lessons for course: {SelectedCourse.Name} (ID: {SelectedCourse.Id})");

            // Get lessons from database for selected course
            var allLessons = _lessonService.GetAll();
            Debug.WriteLine($"Total lessons from service: {allLessons?.Count() ?? 0}");

            var filteredLessons = allLessons
                .Where(l => l.CourseId == SelectedCourse.Id)
                .OrderByDescending(l => l.Id)
                .ToList();

            Debug.WriteLine($"Filtered lessons count: {filteredLessons.Count}");

            Lessons.Clear();
            foreach (var lesson in filteredLessons)
            {
                Lessons.Add(lesson);
                Debug.WriteLine($"Added lesson: {lesson.Name} (ID: {lesson.Id})");
            }

            if (Lessons.Any())
                SelectedLesson = Lessons.First();
        }

        [RelayCommand]
        private void SelectCourse(Course course)
        {
            SelectedCourse = course;
        }

        [RelayCommand]
        private void SelectLesson(Lesson lesson)
        {
            SelectedLesson = lesson;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;


    }
}