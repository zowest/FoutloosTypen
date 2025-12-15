using System.Collections.ObjectModel;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Services;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Views;

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
        private readonly ILessonProgressService _lessonProgressService;

        private const double TIMER_DURATION = 60; // 60 seconden per opdracht

        public ObservableCollection<Lesson> Lessons { get; set; } = new();
        public ObservableCollection<Assignment> Assignments { get; set; } = new();

        private List<PracticeMaterial> _materials = new();
        private int _materialIndex = 0;
        private int _currentAssignmentIndex = 0;
        private string _previousUserInput = string.Empty;

        private int _lessonId;
        public int LessonId 
        { 
            get => _lessonId; 
            set 
            { 
                _lessonId = value; 
                OnPropertyChanged(nameof(LessonId));
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

        // Properties voor progressiebar (per karakter)
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
            ITypingComparisonService typingComparisonService,
            ILessonProgressService lessonProgressService)
        {
            _lessonService = lessonService;
            _assignmentService = assignmentService;
            _practiceMaterialService = practiceMaterialService;
            _timerService = timerService;
            _typingComparisonService = typingComparisonService;
            _lessonProgressService = lessonProgressService;

            // Subscribe to timer expired event
            _timerService.TimerExpired += OnTimerExpired;

            FormattedText = new FormattedString();
        }

        private async void OnTimerExpired(object? sender, EventArgs e)
        {
            Debug.WriteLine("Timer expired! Showing results...");
            
            if (SelectedLesson == null) return;

            // Mark that the timer expired
            _lessonProgressService.MarkTimerExpired(SelectedLesson.Id);
            
            // End the lesson (this will automatically calculate results)
            _lessonProgressService.EndLesson(SelectedLesson.Id);
            
            // Show results popup
            await ShowLessonResultsAsync();
        }

        private async Task ShowLessonResultsAsync()
        {
            StopTimer();
            
            if (SelectedLesson == null || Application.Current?.MainPage == null)
                return;

            var progress = _lessonProgressService.GetProgress(SelectedLesson.Id);
            if (progress == null)
                return;

            // Show custom popup with progress data
            var popup = new ResultatenPopUp(progress);
            await Application.Current.MainPage.Navigation.PushModalAsync(popup);
            
            // Wait for user response
            bool shouldContinue = await popup.WaitForUserResponseAsync();
            
            if (shouldContinue)
            {
                // User clicked "Ga verder" - navigate back
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                // User clicked "Herstart" - restart the lesson
                RestartLesson();
            }
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
                    // StartLesson is now called in FilterAssignmentsByLesson
                    return;
                }
            }

            // Otherwise select first lesson
            if (Lessons.Any())
            {
                SelectedLesson = Lessons.First();
                // StartLesson is now called in FilterAssignmentsByLesson
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
            
            _lessonProgressService.StartLesson(SelectedLesson.Id, Assignments.Count);
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

        private void MoveToNextAssignment()
        {
            _currentAssignmentIndex++;
            OnPropertyChanged(nameof(AssignmentProgress));
            OnPropertyChanged(nameof(ProgressText));

            if (_currentAssignmentIndex >= Assignments.Count)
            {
                // All assignments completed - Show results popup
                Debug.WriteLine("All assignments completed! Showing results...");
                
                if (SelectedLesson != null)
                {
                    _lessonProgressService.EndLesson(SelectedLesson.Id);
                }
                
                ShowLessonResults();
                return;
            }

            // Reset timer for next assignment
            RestartTimer();
            
            // Move to next assignment
            SelectedAssignment = Assignments[_currentAssignmentIndex];
            Debug.WriteLine($"Moved to assignment {_currentAssignmentIndex + 1}/{Assignments.Count}");
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

        private async void ShowLessonResults()
        {
            StopTimer();

            // Toon custom popup
            if (Application.Current?.MainPage != null && SelectedLesson != null)
            {
                var progress = _lessonProgressService.GetProgress(SelectedLesson.Id);
                var popup = new Views.ResultatenPopUp(progress);
                await Application.Current.MainPage.Navigation.PushModalAsync(popup);

                // Wacht tot de gebruiker op de knop klikt
                // true = Ga verder, false = Herstart
                bool shouldContinue = await popup.WaitForUserResponseAsync();

                if (shouldContinue)
                {
                    // Gebruiker klikte op "Ga verder" - navigeer terug
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    // Gebruiker klikte op "Herstart" - herstart de les
                    RestartLesson();
                }
            }
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

        public void UpdateTypedText(string typedText)
        {
            if (CurrentMaterial == null || string.IsNullOrEmpty(CurrentMaterial.Sentence))
                return;

            // Check if user made a mistake with the newly typed character
            if (_typingComparisonService.IsCharacterIncorrect(
                CurrentMaterial.Sentence, 
                _previousUserInput, 
                typedText))
            {
                if (SelectedLesson != null)
                {
                    _lessonProgressService.RecordMistake(SelectedLesson.Id);
                    Debug.WriteLine($"Mistake recorded! Total: {_lessonProgressService.GetProgress(SelectedLesson.Id)?.TotalMistakes}");
                }
            }

            _previousUserInput = typedText;
            UserInput = typedText;
            UpdateFormattedText();

            // Update current progress (including incomplete sentences)
            if (SelectedLesson != null)
            {
                _lessonProgressService.UpdateCurrentProgress(SelectedLesson.Id, typedText.Length, typedText);
            }

            // Check if sentence is complete and correct
            if (typedText == CurrentMaterial.Sentence)
            {
                if (SelectedLesson != null)
                {
                    _lessonProgressService.CompleteSentence(SelectedLesson.Id, typedText);
                    var progress = _lessonProgressService.GetProgress(SelectedLesson.Id);
                    Debug.WriteLine($"Sentence completed! Total mistakes: {progress?.TotalMistakes}, Sentences: {progress?.SentencesCompleted}");
                }
                
                // Move to next sentence or assignment
                MoveToNextMaterial();
            }
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

        private void ResetTyping()
        {
            UserInput = string.Empty;
            _previousUserInput = string.Empty;
            TypedCharactersCount = 0;
            UpdateFormattedText();
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            
            if (SelectedLesson != null)
            {
                _lessonProgressService.EndLesson(SelectedLesson.Id);
                var progress = _lessonProgressService.GetProgress(SelectedLesson.Id);
                Debug.WriteLine($"Lesson ended. Total mistakes: {progress?.TotalMistakes}, Time: {progress?.TimeSpent:F2}s");
            }

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

        ~AssignmentViewModel()
        {
            // Unsubscribe from event
            _timerService.TimerExpired -= OnTimerExpired;
            _timerService.Stop();
        }
    }
}