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
        private readonly ITypingComparisonService _typingComparisonService;

        private const double TIMER_DURATION = 60; // 60 seconden per opdracht

        public ObservableCollection<Lesson> Lessons { get; set; } = new();
        public ObservableCollection<Assignment> Assignments { get; set; } = new();

        private List<PracticeMaterial> _materials = new();
        private int _materialIndex = 0;
        private int _currentAssignmentIndex = 0;

        // Share functionality delegated to ShareViewModel
        public ShareViewModel ShareVM { get; }

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
                    _timerService.Initialize(TIMER_DURATION);
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
                OnPropertyChanged(nameof(AssignmentProgress));
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

        // Progress voor opdrachten (1/n)
        public string AssignmentProgress
        {
            get
            {
                if (Assignments.Count == 0)
                    return "Opdracht 0/0";
                return $"Opdracht {_currentAssignmentIndex + 1}/{Assignments.Count}";
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
            get => $"{Math.Round(Progress * 100)}% - {AssignmentProgress}";
        }

        public ITimerService Timer => _timerService;

        public AssignmentViewModel(
            ILessonService lessonService,
            IAssignmentService assignmentService,
            IPracticeMaterialService practiceMaterialService,
            ITimerService timerService,
            ShareViewModel shareViewModel,
            ITypingComparisonService typingComparisonService)
        {
            _lessonService = lessonService;
            _assignmentService = assignmentService;
            _practiceMaterialService = practiceMaterialService;
            _timerService = timerService;
            ShareVM = shareViewModel;
            _typingComparisonService = typingComparisonService;

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

            _currentAssignmentIndex = 0;
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

        private async void MoveToNextAssignment()
        {
            // If current is the last assignment, do not advance or change the displayed count
            if (_currentAssignmentIndex + 1 >= Assignments.Count)
            {
                Debug.WriteLine("All assignments completed. Staying on the last assignment and not incrementing count.");
                OnPropertyChanged(nameof(AssignmentProgress));
                OnPropertyChanged(nameof(ProgressText));
                return;
            }

            _currentAssignmentIndex++;
            OnPropertyChanged(nameof(AssignmentProgress));
            OnPropertyChanged(nameof(ProgressText));

            // Reset timer for next assignment
            RestartTimer();
            
            // Move to next assignment
            SelectedAssignment = Assignments[_currentAssignmentIndex];
            Debug.WriteLine($"Moved to assignment {_currentAssignmentIndex + 1}/{Assignments.Count}");
        }

        private void MoveToNextMaterial()
        {
            if (_materials == null || !_materials.Any())
                return;

            _materialIndex++;

            if (_materialIndex < _materials.Count)
            {
                CurrentMaterial = _materials[_materialIndex];
                Debug.WriteLine($"Moved to next material: {_materialIndex + 1}/{_materials.Count}");
            }
            else
            {
                // All materials in current assignment completed, move to next assignment
                Debug.WriteLine("All materials completed in this assignment");
                MoveToNextAssignment();
            }
        }

        /// <summary>
        /// Herstart de huidige les vanaf het begin
        /// </summary>
        public void RestartLesson()
        {
            Debug.WriteLine("Restarting lesson...");
            
            // Reset naar de eerste opdracht
            _currentAssignmentIndex = 0;
            OnPropertyChanged(nameof(AssignmentProgress));
            OnPropertyChanged(nameof(ProgressText));
            
            // Selecteer de eerste opdracht
            if (Assignments.Any())
            {
                SelectedAssignment = Assignments.First();
            }
            
            // Reset typing en timer
            ResetTyping();
            RestartTimer();
            
            Debug.WriteLine("Lesson restarted successfully");
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

            // Check if sentence is complete and correct
            if (typedText == CurrentMaterial.Sentence)
            {
                Debug.WriteLine("Sentence completed correctly!");
                
                // Move to next sentence or assignment
                MoveToNextMaterial();
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
                        // Correct character - show in black
                        span.TextColor = Colors.Black;
                        span.BackgroundColor = Colors.Transparent;
                    }
                    else
                    {
                        // Incorrect character - show in red with background
                        span.TextColor = Colors.White;
                        span.BackgroundColor = Colors.Red;
                    }
                }
                else if (i == typedText.Length)
                {
                    // Current character cursor position
                    span.TextColor = Colors.Gray;
                    span.BackgroundColor = Colors.LightGray;
                }
                else
                {
                    // Not yet typed - show in light gray
                    span.TextColor = Colors.LightGray;
                    span.BackgroundColor = Colors.Transparent;
                }

                formatted.Spans.Add(span);
            }

            FormattedText = formatted;
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            _timerService.Stop();
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
            _currentAssignmentIndex = 0;
            OnPropertyChanged(nameof(AssignmentProgress));
            OnPropertyChanged(nameof(ProgressText));
            
            if (Assignments.Any())
                SelectedAssignment = Assignments.First();
                
            RestartTimer();
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

        [RelayCommand]
        private void Refresh()
        {
            ResetTyping();
            RestartTimer();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}