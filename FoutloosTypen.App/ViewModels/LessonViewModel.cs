using System.Collections.ObjectModel;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Services;
using System.ComponentModel;

namespace FoutloosTypen.ViewModels
{
    public partial class LessonViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly ILessonService _lessonService;
        private readonly ICourseService _courseService;

        public ObservableCollection<Lesson> Lessons { get; set; } = new();
        public ObservableCollection<Course> Courses { get; set; } = new();

        private Lesson _selectedLesson;
        public Lesson SelectedLesson
        {
            get => _selectedLesson;
            set
            {
                _selectedLesson = value;
                OnPropertyChanged(nameof(SelectedLesson)); // Pass property name
            }
        }

        public LessonViewModel(ILessonService lessonService, ICourseService courseService)
        {
            _lessonService = lessonService;
            _courseService = courseService;
        }

        public async Task OnAppearingAsync()
        {
            var lessonItems = _lessonService.GetAll();
            Lessons.Clear();
            foreach (var lesson in lessonItems.Reverse())
                Lessons.Add(lesson);

            var courseItems = _courseService.GetAll();
            Courses.Clear();
            foreach (var course in courseItems)
                Courses.Add(course);
        }

        // If not already present in BaseViewModel, add this method:
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}