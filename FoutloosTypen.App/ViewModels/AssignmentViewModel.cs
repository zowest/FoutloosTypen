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
        public enum PopupResult
        {
            Home,
            Restart,
            NextLesson
        }

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

        // Optimalisatie: cache vorige display state waarden
        private string _lastCorrectText = string.Empty;
        private string _lastErrorText = string.Empty;
        private string _lastCursorChar = string.Empty;
        private string _lastRemainingText = string.Empty;
        private int _lastWordCount = 0;

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

        private FormattedString _formattedText = new();
        public FormattedString FormattedText
        {
            get => _formattedText;
            set
            {
                if (_formattedText != value)
                {
                    _formattedText = value;
                    OnPropertyChanged(nameof(FormattedText));
                }
            }
        }

        #endregion

        public AssignmentViewModel(
            ILessonService lessonService,
            IAssignmentService assignmentService,
            IPracticeMaterialService practiceMaterialService,
            ITimerService timerService,
            ITypingComparisonService typingComparisonService,
            IResultService resultService)
        {
            _lessonService = lessonService;
            _assignmentService = assignmentService;
            _practiceMaterialService = practiceMaterialService;
            _timerService = timerService;
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

            // Bereken display state
            var displayState = _typingComparisonService.CalculateDisplayState(targetText, typedText);

            // Pas alleen gewijzigde waarden toe (voorkomt onnodige UI updates)
            ApplyDisplayStateOptimized(displayState);

            // Update progress alleen na woorden (spatie of einde)
            UpdateProgressOnWordBoundary(typedText, displayState.CorrectCharacterCount);

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

        private void ApplyDisplayStateOptimized(TypingDisplayState state)
        {
            // Check of er iets veranderd is
            if (_lastCorrectText == state.CorrectText &&
                _lastErrorText == state.ErrorText &&
                _lastCursorChar == state.CursorChar &&
                _lastRemainingText == state.RemainingText)
            {
                return; // Niets gewijzigd, skip UI update
            }

            _lastCorrectText = state.CorrectText;
            _lastErrorText = state.ErrorText;
            _lastCursorChar = state.CursorChar;
            _lastRemainingText = state.RemainingText;

            // Bouw FormattedString voor UI
            var formatted = new FormattedString();

            // Groene tekst (correct getypt)
            if (!string.IsNullOrEmpty(state.CorrectText))
            {
                formatted.Spans.Add(new Span
                {
                    Text = state.CorrectText,
                    TextColor = Colors.Black,
                    FontSize = 32
                });
            }

            // Rode tekst (fouten)
            if (!string.IsNullOrEmpty(state.ErrorText))
            {
                formatted.Spans.Add(new Span
                {
                    Text = state.ErrorText,
                    TextColor = Colors.Red,
                    FontSize = 32
                });
            }

            // Cursor karakter (onderstreept)
            if (!string.IsNullOrEmpty(state.CursorChar))
            {
                formatted.Spans.Add(new Span
                {
                    Text = state.CursorChar,
                    TextColor = Colors.Black,
                    TextDecorations = TextDecorations.Underline,
                    FontSize = 32
                });
            }

            // Resterende tekst (grijs)
            if (!string.IsNullOrEmpty(state.RemainingText))
            {
                formatted.Spans.Add(new Span
                {
                    Text = state.RemainingText,
                    TextColor = Colors.Gray,
                    FontSize = 32
                });
            }

            FormattedText = formatted;
        }

        private void UpdateProgressOnWordBoundary(string typedText, int correctChars)
        {
            _typedCharactersCount = correctChars;

            int currentWordCount = 0;
            for (int i = 0; i < typedText.Length; i++)
            {
                if (typedText[i] == ' ')
                    currentWordCount++;
            }

            // Update UI alleen bij nieuw woord of bij voltooiing
            bool isComplete = correctChars == TotalCharactersCount;
            if (currentWordCount != _lastWordCount || isComplete)
            {
                _lastWordCount = currentWordCount;
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        private void ResetTyping()
        {
            _previousUserInput = string.Empty;
            _typedCharactersCount = 0;
            _lastWordCount = 0;
            _lastCorrectText = string.Empty;
            _lastErrorText = string.Empty;
            _lastCursorChar = string.Empty;
            _lastRemainingText = string.Empty;
            UserInput = string.Empty;

            if (CurrentMaterial?.Sentence != null)
            {
                var displayState = _typingComparisonService.CalculateDisplayState(CurrentMaterial.Sentence, string.Empty);

                // Reset cache zodat ApplyDisplayStateOptimized alles opnieuw bouwt
                _lastCorrectText = "FORCE_RESET";
                ApplyDisplayStateOptimized(displayState);
            }
            else
            {
                FormattedText = new FormattedString();
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
                // All assignments completed - Show results popup
                if (SelectedLesson != null)
                {
                    _resultService.EndLesson(SelectedLesson.Id);
                }

                // Call async method properly on UI thread
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await ShowLessonResultsAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"ERROR showing results: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                });
                return;
            }

            RestartTimer();

            // Move to next assignment
            SelectedAssignment = Assignments[_currentAssignmentIndex];
        }

        private Lesson? GetNextLesson()
        {
            if (SelectedLesson == null)
                return null;

            // Haal alle lessen uit dezelfde cursus op
            var lessonsInCourse = Lessons
                .Where(l => l.CourseId == SelectedLesson.CourseId)
                .OrderBy(l => l.Id)
                .ToList();

            // Vind de huidige les index
            int currentIndex = lessonsInCourse.FindIndex(l => l.Id == SelectedLesson.Id);

            // Return de volgende les als die bestaat
            if (currentIndex >= 0 && currentIndex < lessonsInCourse.Count - 1)
            {
                return lessonsInCourse[currentIndex + 1];
            }

            return null;
        }

        private async Task NavigateToNextLessonAsync()
        {
            var nextLesson = GetNextLesson();
            if (nextLesson != null)
            {
                // Navigeer naar de volgende les
                await Shell.Current.GoToAsync($"..?lessonId={nextLesson.Id}");
            }
            else
            {
                // Geen volgende les, ga terug naar home
                await Shell.Current.Navigation.PopToRootAsync();
            }
        }

        #endregion

        #region Timer

        private async void OnTimerExpired(object? sender, EventArgs e)
        {
            if (SelectedLesson == null) return;

            _resultService.MarkTimerExpired(SelectedLesson.Id);
            _resultService.EndLesson(SelectedLesson.Id);

            await ShowLessonResultsAsync();
        }

        #endregion

        #region Results

        private async Task ShowLessonResultsAsync()
        {
            StopTimer();

            if (SelectedLesson == null || Application.Current?.MainPage == null)
                return;

            var progress = _resultService.GetProgress(SelectedLesson.Id);
            if (progress == null)
                return;

         
            var result = ConvertLessonProgressToResult(progress);

            var popup = new ResultatenPopUp(result);
            await Application.Current.MainPage.Navigation.PushModalAsync(popup);

            var popupResult = await popup.WaitForUserResponseAsync();

            switch (popupResult)
            {
                case ResultatenPopUp.PopupResult.Home:
                    await Shell.Current.Navigation.PopToRootAsync();
                    break;
                case ResultatenPopUp.PopupResult.Restart:
                    RestartLesson();
                    break;
                case ResultatenPopUp.PopupResult.NextLesson:
                    await NavigateToNextLessonAsync();
                    break;
            }
        }

        private Result ConvertLessonProgressToResult(Core.Interfaces.Services.LessonProgress progress)
        {
            return new Result
            {
                LessonId = progress.LessonId,
                TotalMistakes = progress.TotalMistakes,
                SentencesCompleted = progress.SentencesCompleted,
                TotalCharactersTyped = progress.CharactersTyped,
                CompletedSentences = new List<string>(),
                CurrentIncompleteText = progress.CurrentText,
                StartTime = progress.StartTime,
                EndTime = DateTime.Now,
                ExpectedTime = 60,
                TimerExpired = progress.TimerExpired,
                StudentId = 0, 
                IsEndlessMode = false
            };
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

        #region Helpers

        private void RestartLesson()
        {
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
        }

        private void UpdateTotalCharactersCount()
        {
            TotalCharactersCount = CurrentMaterial?.Sentence?.Length ?? 0;
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
    }
}