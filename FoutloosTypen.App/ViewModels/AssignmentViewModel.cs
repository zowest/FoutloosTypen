using System.Collections.ObjectModel;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Services;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

namespace FoutloosTypen.ViewModels
{
    [QueryProperty(nameof(LessonId), "lessonId")]
    public partial class AssignmentViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ILessonService _lessonService;
        private readonly IPracticeMaterialService _practiceMaterialService;
        private readonly ITimerService _timerService;

        public ObservableCollection<Lesson> Lessons { get; set; } = new();
        public ObservableCollection<Assignment> Assignments { get; set; } = new();

        private List<PracticeMaterial> _materials = new();
        private int _materialIndex = 0;

        private int _lessonId;
        public int LessonId 
        { 
            get => _lessonId; 
            set 
            { 
                _lessonId = value; 
                OnPropertyChanged(nameof(LessonId));
                Debug.WriteLine($"LessonId set to: {value}");
            } 
        }

        private PracticeMaterial _currentMaterial;
        public PracticeMaterial CurrentMaterial
        {
            get => _currentMaterial;
            set
            {
                _currentMaterial = value;
                OnPropertyChanged(nameof(CurrentMaterial));
                UpdateTotalCharactersCount();
                ResetTyping();
            }
        }

        private Lesson? _selectedLesson;
        public Lesson? SelectedLesson
        {
            get => _selectedLesson;
            set
            {
                _selectedLesson = value;
                OnPropertyChanged(nameof(SelectedLesson));
                FilterAssignmentsByLesson();
                
                // Initialize timer with lesson's total time
                if (value != null && value.TotalTime > 0)
                {
                    Debug.WriteLine($"Initializing timer with {value.TotalTime} seconds from lesson");
                    _timerService.Initialize(value.TotalTime);
                    _timerService.Start();
                }
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
                LoadPracticeMaterials();
            }
        }

        private string _userInput = string.Empty;
        public string UserInput
        {
            get => _userInput;
            set
            {
                _userInput = value;
                OnPropertyChanged(nameof(UserInput));
                UpdateTypedCharactersCount();
            }
        }

        private FormattedString _formattedText;
        public FormattedString FormattedText
        {
            get => _formattedText;
            set
            {
                _formattedText = value;
                OnPropertyChanged(nameof(FormattedText));
            }
        }

        // AANGEPASTE PROPERTIES VOOR PROGRESSIEBAR (PER KARAKTER)
        private int _typedCharactersCount;
        public int TypedCharactersCount
        {
            get => _typedCharactersCount;
            set
            {
                _typedCharactersCount = value;
                OnPropertyChanged(nameof(TypedCharactersCount));
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        private int _totalCharactersCount;
        public int TotalCharactersCount
        {
            get => _totalCharactersCount;
            set
            {
                _totalCharactersCount = value;
                OnPropertyChanged(nameof(TotalCharactersCount));
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public double Progress
        {
            get
            {
                if (TotalCharactersCount == 0)
                    return 0;
                return (double)TypedCharactersCount / TotalCharactersCount;
            }
        }

        public string ProgressText
        {
            get => $"{Math.Round(Progress * 100)}%";
        }

        public ITimerService Timer => _timerService;

        public AssignmentViewModel(
            ILessonService lessonService,
            IAssignmentService assignmentService,
            IPracticeMaterialService practiceMaterialService,
            ITimerService timerService)
        {
            _lessonService = lessonService;
            _assignmentService = assignmentService;
            _practiceMaterialService = practiceMaterialService;
            _timerService = timerService;

            FormattedText = new FormattedString();
        }

        public async Task OnAppearingAsync()
        {
            // Get all lessons with TotalTime calculated
            var lessons = _lessonService.GetAll();

            Lessons.Clear();

            if (lessons != null)
            {
                foreach (var lesson in lessons)
                {
                    Lessons.Add(lesson);
                    Debug.WriteLine($"Loaded lesson: {lesson.Name} with TotalTime: {lesson.TotalTime} seconds");
                }
            }

            // If LessonId was passed via navigation, select that lesson
            if (LessonId > 0)
            {
                var targetLesson = Lessons.FirstOrDefault(l => l.Id == LessonId);
                if (targetLesson != null)
                {
                    SelectedLesson = targetLesson;
                    Debug.WriteLine($"Selected lesson: {targetLesson.Name} (ID: {targetLesson.Id}) with TotalTime: {targetLesson.TotalTime}");
                    return;
                }
            }

            // Otherwise select first lesson
            if (Lessons.Any())
            {
                SelectedLesson = Lessons.First();
            }
        }

        private void FilterAssignmentsByLesson()
        {
            if (SelectedLesson is null)
                return;

            Debug.WriteLine($"Filtering assignments for lesson ID: {SelectedLesson.Id}");

            var allAssignments = _assignmentService.GetAll();

            var filteredAssignments = allAssignments
                .Where(a => a.LessonId == SelectedLesson.Id)
                .OrderBy(a => a.Id)
                .ToList();

            Debug.WriteLine($"Found {filteredAssignments.Count} assignments for lesson {SelectedLesson.Id}");

            Assignments.Clear();
            foreach (var assignment in filteredAssignments)
            {
                Assignments.Add(assignment);
                Debug.WriteLine($"Added assignment: Id={assignment.Id}, LessonId={assignment.LessonId}");
            }

            if (Assignments.Any())
                SelectedAssignment = Assignments.First();
        }

        private void LoadPracticeMaterials()
        {
            Debug.WriteLine("LoadPracticeMaterials aangeroepen");

            if (SelectedAssignment == null)
                return;

            _materials = _practiceMaterialService
                .GetAll()
                .Where(pm => pm.AssignmentId == SelectedAssignment.Id)
                .ToList();

            _materialIndex = 0;

            if (_materials.Any())
                CurrentMaterial = _materials[_materialIndex];
            else
                CurrentMaterial = new PracticeMaterial { Sentence = "Geen zinnen gevonden." };
        }

        private void UpdateTotalCharactersCount()
        {
            if (CurrentMaterial == null || string.IsNullOrWhiteSpace(CurrentMaterial.Sentence))
            {
                TotalCharactersCount = 0;
                return;
            }

            TotalCharactersCount = CurrentMaterial.Sentence.Length;
        }

        private void UpdateTypedCharactersCount()
        {
            if (string.IsNullOrEmpty(UserInput) || CurrentMaterial == null)
            {
                TypedCharactersCount = 0;
                return;
            }

            string targetText = CurrentMaterial.Sentence ?? string.Empty;
            int correctChars = 0;

            for (int i = 0; i < UserInput.Length && i < targetText.Length; i++)
            {
                if (UserInput[i] == targetText[i])
                {
                    correctChars++;
                }
            }

            TypedCharactersCount = correctChars;
        }

        private void ResetTyping()
        {
            UserInput = string.Empty;
            TypedCharactersCount = 0;
            UpdateFormattedText();
        }

        public void UpdateTypedText(string typedText)
        {
            if (CurrentMaterial == null || string.IsNullOrEmpty(CurrentMaterial.Sentence))
                return;

            UserInput = typedText;
            UpdateFormattedText();

            if (typedText.Length == CurrentMaterial.Sentence.Length)
            {
                bool allCorrect = true;
                for (int i = 0; i < typedText.Length; i++)
                {
                    if (typedText[i] != CurrentMaterial.Sentence[i])
                    {
                        allCorrect = false;
                        break;
                    }
                }

                if (allCorrect)
                {
                    Debug.WriteLine("Sentence completed correctly!");
                }
            }
        }

        private void UpdateFormattedText()
        {
            var formatted = new FormattedString();
            string targetText = CurrentMaterial?.Sentence ?? string.Empty;
            string typedText = UserInput ?? string.Empty;

            for (int i = 0; i < targetText.Length; i++)
            {
                var span = new Span
                {
                    Text = targetText[i].ToString(),
                    FontSize = 32,
                };

                if (i < typedText.Length)
                {
                    if (typedText[i] == targetText[i])
                    {
                        span.TextColor = Colors.Black;
                        span.BackgroundColor = Colors.Transparent;
                    }
                    else
                    {
                        span.TextColor = Colors.White;
                        span.BackgroundColor = Colors.Red;
                    }
                }
                else if (i == typedText.Length)
                {
                    span.TextColor = Colors.Gray;
                    span.BackgroundColor = Colors.LightGray;
                }
                else
                {
                    span.TextColor = Colors.LightGray;
                    span.BackgroundColor = Colors.Transparent;
                }

                formatted.Spans.Add(span);
            }

            FormattedText = formatted;
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

        [RelayCommand]
        private void StartTimer()
        {
            _timerService.Start();
        }

        [RelayCommand]
        private void StopTimer()
        {
            _timerService.Stop();
        }

        [RelayCommand]
        private void RestartTimer()
        {
            _timerService.Restart();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        ~AssignmentViewModel()
        {
            StopTimer();
        }
    }
}