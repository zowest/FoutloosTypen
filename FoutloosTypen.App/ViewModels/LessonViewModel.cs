using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;

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
                .Where(l => l.CourseId == courseId)
                .OrderByDescending(l => l.Id)
                .ToList();

            Lessons.Clear();
            foreach (var lesson in filteredLessons)
                Lessons.Add(lesson);

            if (Lessons.Any())
                SelectedLesson = Lessons.Last();
        }

        [RelayCommand]
        private void SelectLesson(Lesson lesson) => SelectedLesson = lesson;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

