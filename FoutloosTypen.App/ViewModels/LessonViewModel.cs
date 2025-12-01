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

        public ObservableCollection<Lesson> Lessons { get; set; } = new();

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

        public LessonViewModel(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        public async Task LoadLessonsForCourseAsync(int courseId)
        {

            var allLessons = _lessonService.GetAll();

            var filteredLessons = allLessons
                .Where(l => l.CourseId == SelectedCourse.Id)
                .OrderByDescending(l => l.Id)
                .ToList();

            Lessons.Clear();
            foreach (var lesson in filteredLessons)
            {
                Lessons.Add(lesson);
            }

            if (Lessons.Any())
                SelectedLesson = Lessons.First();
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
