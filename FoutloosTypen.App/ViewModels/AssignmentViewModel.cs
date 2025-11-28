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
    public partial class AssignmentViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ILessonService _lessonService;

        public ObservableCollection<Lesson> Lessons { get; set; } = new();
        public ObservableCollection<Assignment> Assignments { get; set; } = new();

        private Lesson? _selectedLesson;
        public Lesson? SelectedLesson
        {
            get => _selectedLesson;
            set
            {
                _selectedLesson = value;
                OnPropertyChanged(nameof(SelectedLesson));
                FilterAssignmentsByLesson();
            }
        }

        private Assignment? _selectedAssignment;
        public Assignment? SelectedAssignment
        {
            get => _selectedAssignment;
            set
            {
                _selectedAssignment = value;
                OnPropertyChanged(nameof(SelectedAssignment));
            }
        }

        public AssignmentViewModel(
            ILessonService lessonService,
            IAssignmentService assignmentService)
        {
            _lessonService = lessonService;
            _assignmentService = assignmentService;
        }

        public async Task OnAppearingAsync()
        {
            var lessons = _lessonService.GetAll();

            Lessons.Clear();

            if (lessons != null)
            {
                foreach (var lesson in lessons)
                {
                    Lessons.Add(lesson);
                }
            }

            if (Lessons.Any())
                SelectedLesson = Lessons.First();
        }

        private void FilterAssignmentsByLesson()
        {
            if (SelectedLesson is null)
            {
                return;
            }


            var allAssignments = _assignmentService.GetAll();

            var filteredAssignments = allAssignments
                .Where(a => a.LessonId == SelectedLesson.Id)
                .Take(10)
                .OrderBy(a => a.Id)
                .ToList();

            Assignments.Clear();
            foreach (var assignment in filteredAssignments)
            {
                Assignments.Add(assignment);
            }

            if (Assignments.Any())
                SelectedAssignment = Assignments.First();
        }

        [RelayCommand]
        private void SelectLesson(Lesson lesson)
        {
            SelectedLesson = lesson;
        }

        [RelayCommand]
        private void SelectAssignment(Assignment assignment)
        {
            SelectedAssignment = assignment;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
