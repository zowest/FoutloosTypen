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
        private readonly IResultService _ResultService;
        private readonly GlobalViewModel _globalViewModel; // ADD THIS

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
            IResultService ResultService,
            GlobalViewModel globalViewModel)  // ADD THIS
        {
            _lessonService = lessonService;
            _assignmentService = assignmentService;
            _practiceMaterialService = practiceMaterialService;
            _timerService = timerService;
            _typingComparisonService = typingComparisonService;
            _ResultService = ResultService;
            _globalViewModel = globalViewModel;  // ADD THIS

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
            Debug.WriteLine("Timer expired! Showing results...");

            if (SelectedLesson == null) return;

            // Mark that the timer expired
            _ResultService.MarkTimerExpired(SelectedLesson.Id);

            // End the lesson (this will automatically calculate results)
            _ResultService.EndLesson(SelectedLesson.Id);

            // Show results popup
            await ShowLessonResultsAsync();
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
            try
            {
                Debug.WriteLine("=== ShowLessonResultsAsync START ===");
                
                StopTimer();

                if (SelectedLesson == null)
                {
                    Debug.WriteLine("ERROR: SelectedLesson is null");
                    return;
                }

                if (Application.Current?.MainPage == null)
                {
                    Debug.WriteLine("ERROR: Application.Current.MainPage is null");
                    return;
                }

                var progress = _ResultService.GetProgress(SelectedLesson.Id);
                if (progress == null)
                {
                    Debug.WriteLine("ERROR: Progress is null");
                    return;
                }

                Debug.WriteLine($"Progress retrieved: Speed={progress.Speed}, Accuracy={progress.Accuracy}, Score={progress.Score}");

                // **SAVE TO DATABASE**
                if (_globalViewModel.Student != null)
                {
                    _ResultService.SaveResult(SelectedLesson.Id, _globalViewModel.Student.Id, progress);
                    Debug.WriteLine($"Result saved for student {_globalViewModel.Student.Id}, lesson {SelectedLesson.Id}");
                }
                else
                {
                    Debug.WriteLine("WARNING: No student logged in, result not saved");
                }

                // Convert LessonProgress to Result
                var result = ConvertProgressToResult(progress);
                Debug.WriteLine($"Result converted: WPM={result.WordsPerMinute}, Score={result.Score}");

                // Get score comparison
                ScoreComparison? comparison = null;
                if (_globalViewModel.Student != null)
                {
                    comparison = _ResultService.CompareWithPrevious(SelectedLesson.Id, _globalViewModel.Student.Id, result);
                    Debug.WriteLine($"Comparison: IsNewBest={comparison?.IsNewPersonalBest}, IsFirst={comparison?.IsFirstAttempt}");
                }

                // Show custom popup with result data
                Debug.WriteLine("Creating ResultatenPopUp...");
                var popup = new ResultatenPopUp(result, comparison);
                
                Debug.WriteLine("Pushing modal...");
                await Application.Current.MainPage.Navigation.PushModalAsync(popup);
                Debug.WriteLine("Modal pushed successfully");

                // Wait for user response
                Debug.WriteLine("Waiting for user response...");
                bool shouldContinue = await popup.WaitForUserResponseAsync();
                Debug.WriteLine($"User response: shouldContinue={shouldContinue}");

                if (shouldContinue)
                {
                    // User clicked "Ga verder" - navigate to lesson-specific leaderboard
                    Debug.WriteLine($"Navigating to leaderboard for lesson {SelectedLesson.Id}");
                    await Shell.Current.GoToAsync($"{nameof(LessonLeaderboardView)}?lessonId={SelectedLesson.Id}");
                }
                else
                {
                    // User clicked "Herstart" - restart the lesson
                    Debug.WriteLine("Restarting lesson...");
                    RestartLesson();
                }

                Debug.WriteLine("=== ShowLessonResultsAsync END ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== EXCEPTION in ShowLessonResultsAsync ===");
                Debug.WriteLine($"Message: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
            }
        }

        private Result ConvertProgressToResult(LessonProgress progress)
        {
            var timeSpent = (DateTime.Now - progress.StartTime).TotalSeconds;
            var expectedTime = SelectedLesson?.TotalTime ?? 300;

            return new Result
            {
                LessonId = progress.LessonId,
                StudentId = _globalViewModel.Student?.Id ?? 0,  // USE GLOBAL VIEWMODEL
                TotalMistakes = progress.TotalMistakes,
                SentencesCompleted = progress.SentencesCompleted,
                TotalCharactersTyped = progress.CharactersTyped,
                CompletedSentences = new List<string>(),
                CurrentIncompleteText = progress.CurrentText,
                StartTime = progress.StartTime,
                EndTime = DateTime.Now,
                ExpectedTime = expectedTime,
                TimerExpired = progress.TimerExpired,
                Score = progress.Score,
                StrokesPerMinute = progress.StrokesPerMinute,
                WordsPerMinute = (int)progress.Speed,
                AccuracyPercent = progress.Accuracy,
                TimeRemaining = Math.Max(0, expectedTime - timeSpent)
            };
        }

        public async Task OnAppearingAsync()
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
                    TextColor = Colors.White,
                    BackgroundColor = Colors.Red,
                    FontSize = 32
                });
                    SelectedLesson = targetLesson;
                    Debug.WriteLine($"Selected lesson: {targetLesson.Name} (ID: {targetLesson.Id}) with TotalTime: {targetLesson.TotalTime}");
                    return;
                }
            }

            // Cursor karakter (onderstreept)
            if (!string.IsNullOrEmpty(state.CursorChar))
            {
                formatted.Spans.Add(new Span
                {
                    Text = state.CursorChar,
                    TextColor = Colors.LightGrey,
                    TextDecorations = TextDecorations.Underline,
                    FontSize = 32
                });
            }

            // Rest van de tekst (grijs)
            if (!string.IsNullOrEmpty(state.RemainingText))
            {
                formatted.Spans.Add(new Span
                {
                    Text = state.RemainingText,
                    TextColor = Colors.LightGray,
                    FontSize = 32
                });
                SelectedLesson = Lessons.First();
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
            _ResultService.StartLesson(SelectedLesson.Id, Assignments.Count);
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
                // All assignments completed - Show results popup
                Debug.WriteLine("All assignments completed! Showing results...");

                if (SelectedLesson != null)
                {
                    _ResultService.EndLesson(SelectedLesson.Id);
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
                        Debug.WriteLine($"ERROR showing results: {ex.Message}");
                        Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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

            // Vind lessen in dezelfde cursus, gesorteerd op ID
            var lessonsInCourse = Lessons
                .Where(l => l.CourseId == SelectedLesson.CourseId)
                .OrderBy(l => l.Id)
                .ToList();

            // Vind de index van de huidige les
            var currentIndex = lessonsInCourse.FindIndex(l => l.Id == SelectedLesson.Id);
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

        #region Timer & Results
            // Check if user made a mistake with the newly typed character
            if (_typingComparisonService.IsCharacterIncorrect(
                CurrentMaterial.Sentence,
                _previousUserInput,
                typedText))
            {
                if (SelectedLesson != null)
                {
                    _ResultService.RecordMistake(SelectedLesson.Id);
                    Debug.WriteLine($"Mistake recorded! Total: {_ResultService.GetProgress(SelectedLesson.Id)?.TotalMistakes}");
                }
            }

        private async void OnTimerExpired(object? sender, EventArgs e)
        {
            if (SelectedLesson == null) return;

            _resultService.MarkTimerExpired(SelectedLesson.Id);
            _resultService.EndLesson(SelectedLesson.Id);

            await ShowLessonResultsAsync();
            // Check if sentence is complete and correct
            if (typedText == CurrentMaterial.Sentence)
            {
                if (SelectedLesson != null)
                {
                    _ResultService.CompleteSentence(SelectedLesson.Id, typedText);
                    var progress = _ResultService.GetProgress(SelectedLesson.Id);
                    Debug.WriteLine($"Sentence completed! Total mistakes: {progress?.TotalMistakes}, Sentences: {progress?.SentencesCompleted}");
                }

                // Move to next sentence or assignment
                MoveToNextMaterial();
            }
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

            var result = await popup.WaitForUserResponseAsync();

            switch (result)
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
        public override void OnDisappearing()
        {
            base.OnDisappearing();

            if (SelectedLesson != null)
            {
                _ResultService.EndLesson(SelectedLesson.Id);
                var progress = _ResultService.GetProgress(SelectedLesson.Id);
                Debug.WriteLine($"Lesson ended. Total mistakes: {progress?.TotalMistakes}, Time: {progress?.TimeSpent:F2}s");
            }

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
    }

    // Add this enum at the top of the file or in a shared location if it is used elsewhere
}