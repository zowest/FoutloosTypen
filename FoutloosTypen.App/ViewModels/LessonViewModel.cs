using System.Collections.ObjectModel;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Services;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
                FilterLessonsByCourse();
            }
        }

        public LessonViewModel(ILessonService lessonService, ICourseService courseService)
        {
            _lessonService = lessonService;
            _courseService = courseService;
        }

        public async Task OnAppearingAsync()
        {
            // Load all courses from database
            var courseItems = _courseService.GetAll();
            Courses.Clear();
            foreach (var course in courseItems)
                Courses.Add(course);

            // Select first course by default
            if (Courses.Any())
            {
                SelectedCourse = Courses.First();
            }
        }

        private void FilterLessonsByCourse()
        {
            if (SelectedCourse is null)
                return;

            // Get lessons from database for selected course
            var allLessons = _lessonService.GetAll();
            var filteredLessons = allLessons
                .Where(l => l.CourseId == SelectedCourse.Id)
                .Take(5) // Max 5 lessons
                .ToList();

            Lessons.Clear();
            foreach (var lesson in filteredLessons)
                Lessons.Add(lesson);

            // Auto-select first lesson
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