using System.Collections.ObjectModel;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Services;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Views;

namespace FoutloosTypen.ViewModels
{
    [QueryProperty(nameof(LessonId), "lessonId")]
    public partial class AssignmentViewModel : BaseViewModel, INotifyPropertyChanged
    {
        // Services (Business Layer)
        private readonly IAssignmentService _assignmentService;
        private readonly ILessonService _lessonService;
        private readonly IPracticeMaterialService _practiceMaterialService;
        private readonly ITimerService _timerService;
        private readonly ITypingComparisonService _typingComparisonService;
        private readonly IResultService _resultService;

        private const double TIMER_DURATION = 60;

        private string _userInput = string.Empty;
        public string UserInput
        {
            get => _userInput;
            set
            {
                if (_userInput != value)
                {
                    _userInput = value;
                    OnPropertyChanged(nameof(UserInput));
                }
            }
        }

        // Collections
        public ObservableCollection<Lesson> Lessons { get; set; } = new();
        public ObservableCollection<Assignment> Assignments { get; set; } = new();

        // State
        private List<PracticeMaterial> _materials = new();
        private int _materialIndex = 0;
        private int _currentAssignmentIndex = 0;
        private string _previousUserInput = string.Empty;

        #region Bindable Properties

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

        // Display properties bound to UI
        private string _correctText = string.Empty;
        public string CorrectText
        {
            get => _correctText;
            set { if (_correctText != value) { _correctText = value; OnPropertyChanged(nameof(CorrectText)); } }
        }

        private string _errorText = string.Empty;
        public string ErrorText
        {
            get => _errorText;
            set { if (_errorText != value) { _errorText = value; OnPropertyChanged(nameof(ErrorText)); OnPropertyChanged(nameof(HasError)); } }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorText);

        private string _cursorChar = string.Empty;
        public string CursorChar
        {
            get => _cursorChar;
            set { if (_cursorChar != value) { _cursorChar = value; OnPropertyChanged(nameof(CursorChar)); } }
        }

        private string _remainingText = string.Empty;
        public string RemainingText
        {
            get => _remainingText;
            set { if (_remainingText != value) { _remainingText = value; OnPropertyChanged(nameof(RemainingText)); } }
        }

        private int _typedCharactersCount;
        public int TypedCharactersCount
        {
            get => _typedCharactersCount;
            set => _typedCharactersCount = value;
        }

        private int _totalCharactersCount;
        public int TotalCharactersCount
        {
            get => _totalCharactersCount;
            set { if (_totalCharactersCount != value) { _totalCharactersCount = value; OnPropertyChanged(nameof(TotalCharactersCount)); } }
        }

        public double Progress => TotalCharactersCount == 0 ? 0 : (double)TypedCharactersCount / TotalCharactersCount;
        public string ProgressText => $"{Math.Round(Progress * 100)}% - {AssignmentProgress}";
        public string AssignmentProgress => Assignments.Count == 0 ? "Opdracht 0/0" : $"Opdracht {_currentAssignmentIndex + 1}/{Assignments.Count}";
        public ITimerService Timer => _timerService;

        #endregion

        public AssignmentViewModel(
            ILessonService lessonService,
            IAssignmentService assignmentService,
            IPracticeMaterialService practiceMaterialService,
            ITimerService timerService,
            ShareViewModel shareViewModel,
            ITypingComparisonService typingComparisonService,
            IResultService resultService)
        {
            _lessonService = lessonService;
            _assignmentService = assignmentService;
            _practiceMaterialService = practiceMaterialService;
            _timerService = timerService;
            ShareVM = shareViewModel;
            _typingComparisonService = typingComparisonService;
            _resultService = resultService;

            _timerService.TimerExpired += OnTimerExpired;
        }

        #region Lifecycle

        public async Task OnAppearingAsync()
        {
            var lessons = _lessonService.GetAll();
            Lessons.Clear();

            if (lessons != null)
            {
                foreach (var lesson in lessons)
                    Lessons.Add(lesson);
            }

            if (LessonId > 0)
            {
                var targetLesson = Lessons.FirstOrDefault(l => l.Id == LessonId);
                if (targetLesson != null)
                {
                    SelectedLesson = targetLesson;
                    return;
                }
            }

            if (Lessons.Any())
                SelectedLesson = Lessons.First();
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            
            if (SelectedLesson != null)
                _resultService.EndLesson(SelectedLesson.Id);

            _timerService.Stop();
        }

        #endregion

        #region Typing Logic (delegates to Service)

        /// <summary>
        /// Main entry point for typing - delegates to Business Layer
        /// </summary>
        public void UpdateTypedText(string typedText)
        {
            if (CurrentMaterial?.Sentence == null)
                return;

            string targetText = CurrentMaterial.Sentence;

            // Delegate mistake detection to BL
            if (_typingComparisonService.IsCharacterIncorrect(targetText, _previousUserInput, typedText))
            {
                if (SelectedLesson != null)
                    _resultService.RecordMistake(SelectedLesson.Id);
            }

            _previousUserInput = typedText;

            // Delegate display calculation to BL
            var displayState = CalculateDisplayState(targetText, typedText);
            
            // Update UI bindings from display state
            ApplyDisplayState(displayState);

            // Update progress
            UpdateProgress(displayState.CorrectCharacterCount);

            // Update result service
            if (SelectedLesson != null)
                _resultService.UpdateCurrentProgress(SelectedLesson.Id, typedText.Length, typedText);

            // Check completion
            if (displayState.IsComplete)
            {
                if (SelectedLesson != null)
                    _resultService.CompleteSentence(SelectedLesson.Id, typedText);
                
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

        /// <summary>
        /// Apply display state from BL to UI properties
        /// </summary>
        private void ApplyDisplayState(TypingDisplayState state)
        {
            CorrectText = state.CorrectText;
            ErrorText = state.ErrorText;
            CursorChar = state.CursorChar;
            RemainingText = state.RemainingText;
        }

        private void UpdateProgress(int correctChars)
        {
            if (_typedCharactersCount != correctChars)
            {
                _typedCharactersCount = correctChars;
                // Only update UI every 5 characters for performance
                if (correctChars % 5 == 0 || correctChars == TotalCharactersCount)
                {
                    OnPropertyChanged(nameof(Progress));
                    OnPropertyChanged(nameof(ProgressText));
                }
            }
        }

        private void ResetTyping()
        {
            _previousUserInput = string.Empty;
            _typedCharactersCount = 0;
            UserInput = string.Empty; // Clear the Entry text
            
            if (CurrentMaterial?.Sentence != null)
            {
                var displayState = CalculateDisplayState(CurrentMaterial.Sentence, string.Empty);
                ApplyDisplayState(displayState);
            }
            else
            {
                CorrectText = string.Empty;
                ErrorText = string.Empty;
                CursorChar = string.Empty;
                RemainingText = string.Empty;
            }
            
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(ProgressText));
        }

        #endregion

        #region Navigation

        private void FilterAssignmentsByLesson()
        {
            if (SelectedLesson is null)
                return;

            var filteredAssignments = _assignmentService.GetAll()
                .Where(a => a.LessonId == SelectedLesson.Id)
                .OrderBy(a => a.Id)
                .ToList();

            Assignments.Clear();
            foreach (var assignment in filteredAssignments)
                Assignments.Add(assignment);

            _currentAssignmentIndex = 0;
            if (Assignments.Any())
                SelectedAssignment = Assignments.First();
            
            _resultService.StartLesson(SelectedLesson.Id, Assignments.Count);
        }

        private void LoadPracticeMaterials()
        {
            if (SelectedAssignment == null)
                return;

            _materials = _practiceMaterialService.GetAll()
                .Where(pm => pm.AssignmentId == SelectedAssignment.Id)
                .ToList();

            _materialIndex = 0;

            CurrentMaterial = _materials.Any() 
                ? _materials[_materialIndex] 
                : new PracticeMaterial { Sentence = "Geen zinnen gevonden." };
        }

        private void MoveToNextMaterial()
        {
            if (_materials == null || !_materials.Any())
                return;

            _materialIndex++;

            if (_materialIndex < _materials.Count)
                CurrentMaterial = _materials[_materialIndex];
            else
                MoveToNextAssignment();
        }

        private void MoveToNextAssignment()
        {
            _currentAssignmentIndex++;
            OnPropertyChanged(nameof(AssignmentProgress));
            OnPropertyChanged(nameof(ProgressText));

            if (_currentAssignmentIndex >= Assignments.Count)
            {
                if (SelectedLesson != null)
                    _resultService.EndLesson(SelectedLesson.Id);
                
                ShowLessonResults();
                return;
            }

            RestartTimer();
            SelectedAssignment = Assignments[_currentAssignmentIndex];
        }

        #endregion

        #region Timer & Results

        private async void OnTimerExpired(object? sender, EventArgs e)
        {
            if (SelectedLesson == null) return;

            _resultService.MarkTimerExpired(SelectedLesson.Id);
            _resultService.EndLesson(SelectedLesson.Id);
            
            await ShowLessonResultsAsync();
        }

        private async Task ShowLessonResultsAsync()
        {
            StopTimer();
            
            if (SelectedLesson == null || Application.Current?.MainPage == null)
                return;

            var progress = _resultService.GetProgress(SelectedLesson.Id);
            if (progress == null)
                return;

            var popup = new ResultatenPopUp(progress);
            await Application.Current.MainPage.Navigation.PushModalAsync(popup);
            
            bool shouldContinue = await popup.WaitForUserResponseAsync();
            
            if (shouldContinue)
                await Shell.Current.GoToAsync("..");
            else
                RestartLesson();
        }

        private async void ShowLessonResults()
        {
            await ShowLessonResultsAsync();
        }

        public void RestartLesson()
        {
            _currentAssignmentIndex = 0;
            OnPropertyChanged(nameof(AssignmentProgress));
            OnPropertyChanged(nameof(ProgressText));
            
            if (Assignments.Any())
                SelectedAssignment = Assignments.First();
            
            ResetTyping();
            RestartTimer();
        }

        private void UpdateTotalCharactersCount()
        {
            TotalCharactersCount = CurrentMaterial?.Sentence?.Length ?? 0;
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void SelectLesson(Lesson lesson) => SelectedLesson = lesson;

        [RelayCommand]
        private void SelectAssignment(Assignment assignment) => SelectedAssignment = assignment;

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
        private void StopTimer() => _timerService.Stop();

        [RelayCommand]
        private void RestartTimer() => _timerService.Restart();

        [RelayCommand]
        private void Refresh()
        {
            ResetTyping();
            RestartTimer();
        }

        #endregion

        #region INotifyPropertyChanged

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        ~AssignmentViewModel()
        {
            _timerService.TimerExpired -= OnTimerExpired;
            _timerService.Stop();
        }

        private TypingDisplayState CalculateDisplayState(string expectedText, string typedText)
        {
            // Delegate to the service which has the correct implementation
            return _typingComparisonService.CalculateDisplayState(expectedText, typedText);
        }
    }
}