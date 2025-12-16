using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Services;

namespace FoutloosTypen.ViewModels
{
    public partial class CoursesViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly ICourseService _courseService;

        public ObservableCollection<Course> Courses { get; set; } = new();

        private Course? _selectedCourse;
        public Course? SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                _selectedCourse = value;
                OnPropertyChanged(nameof(SelectedCourse));

                if (value != null)
                {
                    CourseSelected?.Invoke(value.Id);     
                }
            }
        }

        // Event waarmee LessonViewModel op een CourseId kan reageren
        public event Action<int>? CourseSelected;

        public CoursesViewModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public async Task LoadCoursesAsync()
        {

            var courseItems = _courseService.GetAll();

            Courses.Clear();
            if (courseItems != null)
            {
                foreach (var course in courseItems)
                {
                    Courses.Add(course);
                }
            }

            if (Courses.Any())
                SelectedCourse = Courses.First();
        }

        [RelayCommand]
        private void SelectCourse(Course course)
        {
            SelectedCourse = course;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
