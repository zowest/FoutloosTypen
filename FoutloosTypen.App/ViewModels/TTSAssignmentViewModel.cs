using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace FoutloosTypen.ViewModels
{
    [QueryProperty(nameof(LessonId), "lessonId")]
    public partial class TTSAssignmentViewModel : BaseViewModel
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ILessonService _lessonService;
        private readonly IPracticeMaterialService _practiceMaterialService;
        private readonly ITypingComparisonService _typingComparisonService;
        private readonly IResultService _ResultService;
        private readonly ITtsService _ttsService;

        public ObservableCollection<Lesson> Lessons { get; set; } = new();
        public ObservableCollection<Assignment> Assignments { get; set; } = new();

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

        public ITtsService Tts => _ttsService;

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
                _ = SpeakCurrentMaterialAsync(); // TTS: Speak the sentence when it loads
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
                if (_totalCharactersCount != value)
                {
                    _totalCharactersCount = value;
                    OnPropertyChanged(nameof(TotalCharactersCount));
                    OnPropertyChanged(nameof(Progress));
                    OnPropertyChanged(nameof(ProgressText));
                }
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

        #endregion

        public TTSAssignmentViewModel(
            ILessonService lessonService,
            IAssignmentService assignmentService,
            IPracticeMaterialService practiceMaterialService,
            ITypingComparisonService typingComparisonService,
            IResultService ResultService,
            ITtsService ttsService)
        {
            _lessonService = lessonService;
            _assignmentService = assignmentService;
            _practiceMaterialService = practiceMaterialService;
            _typingComparisonService = typingComparisonService;
            _ResultService = ResultService;
            _ttsService = ttsService;
            FormattedText = new FormattedString();
        }

        #region TTS Methods

        private async Task SpeakCurrentMaterialAsync()
        {
            if (CurrentMaterial == null || string.IsNullOrWhiteSpace(CurrentMaterial.Sentence))
                return;

            try
            {
                var request = new TtsRequest
                {
                    Text = CurrentMaterial.Sentence,
                    Volume = 1.0f,
                    Pitch = 1.0f,
                    Rate = 0.5f,
                    Locale = "nl-NL" // Dutch locale, change if needed
                };

                await _ttsService.SpeakAsync(request);
                Debug.WriteLine($"TTS: Speaking '{CurrentMaterial.Sentence}'");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TTS Error: {ex.Message}");
            }
        }

        #endregion

        #region Lifecycle

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

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            _ttsService.Cancel(); // TTS: Stop speech when leaving

            if (SelectedLesson != null)
            {
                _ResultService.EndLesson(SelectedLesson.Id);
                var progress = _ResultService.GetProgress(SelectedLesson.Id);
                Debug.WriteLine($"Lesson ended. Total mistakes: {progress?.TotalMistakes}, Time: {progress?.TimeSpent:F2}s");
            }
        }

        #endregion

        #region Typing Logic

        public void UpdateTypedText(string typedText)
        {
            if (CurrentMaterial == null || string.IsNullOrEmpty(CurrentMaterial.Sentence))
                return;

            string targetText = CurrentMaterial.Sentence;

            // Delegate mistake detection to BL
            if (_typingComparisonService.IsCharacterIncorrect(targetText, _previousUserInput, typedText))
            {
                if (SelectedLesson != null)
                {
                    _ResultService.RecordMistake(SelectedLesson.Id);
                    Debug.WriteLine($"Mistake recorded! Total: {_ResultService.GetProgress(SelectedLesson.Id)?.TotalMistakes}");
                }
            }

            _previousUserInput = typedText;

            // Bereken display state
            var displayState = _typingComparisonService.CalculateDisplayState(targetText, typedText);

            // Pas alleen gewijzigde waarden toe - now passing typed text for error display
            ApplyDisplayStateOptimized(displayState, typedText, targetText);

            // Update progress alleen na woorden (spatie of einde)
            UpdateProgressOnWordBoundary(typedText, displayState.CorrectCharacterCount);

            // Update result service
            if (SelectedLesson != null)
                _ResultService.UpdateCurrentProgress(SelectedLesson.Id, typedText.Length, typedText);

            // Check completion
            if (displayState.IsComplete)
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

        private void ApplyDisplayStateOptimized(TypingDisplayState state, string typedText, string targetText)
        {
            // Calculate what user actually typed incorrectly
            int correctCount = state.CorrectCharacterCount;
            string actualErrorText = string.Empty;

            // Get the incorrect characters the USER typed (not the expected ones)
            if (typedText.Length > correctCount && correctCount < targetText.Length)
            {
                int errorEnd = Math.Min(typedText.Length, targetText.Length);
                actualErrorText = typedText.Substring(correctCount, errorEnd - correctCount);
            }

            // Check of er iets veranderd is
            if (_lastCorrectText == state.CorrectText &&
                _lastErrorText == actualErrorText &&
                _lastCursorChar == state.CursorChar &&
                _lastRemainingText == state.RemainingText)
            {
                return; // Niets gewijzigd, skip UI update
            }

            _lastCorrectText = state.CorrectText;
            _lastErrorText = actualErrorText;
            _lastCursorChar = state.CursorChar;
            _lastRemainingText = state.RemainingText;

            // Bouw FormattedString voor UI
            var formatted = new FormattedString();

            // Correct getypte tekst (zwart)
            if (!string.IsNullOrEmpty(state.CorrectText))
            {
                formatted.Spans.Add(new Span
                {
                    Text = state.CorrectText,
                    TextColor = Colors.Black,
                    FontSize = 32
                });
            }

            // Fouten - SHOW WHAT USER TYPED (not expected characters)
            if (!string.IsNullOrEmpty(actualErrorText))
            {
                formatted.Spans.Add(new Span
                {
                    Text = actualErrorText,
                    TextColor = Colors.White,
                    BackgroundColor = Colors.Red,
                    FontSize = 32
                });
            }

            // Cursor karakter (onderstreept)
            if (!string.IsNullOrEmpty(state.CursorChar))
            {
                formatted.Spans.Add(new Span
                {
                    Text = state.CursorChar,
                    TextColor = Colors.Transparent,
                    BackgroundColor = Colors.LightGray,
                    FontSize = 32
                });
            }

            // Rest van de tekst (doorzichtig)
            if (!string.IsNullOrEmpty(state.RemainingText))
            {
                formatted.Spans.Add(new Span
                {
                    Text = state.RemainingText,
                    TextColor = Colors.Transparent,
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
                ApplyDisplayStateOptimized(displayState, string.Empty, CurrentMaterial.Sentence);
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

            _ResultService.StartLesson(SelectedLesson.Id, Assignments.Count);
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
                    _ResultService.EndLesson(SelectedLesson.Id);
                }

                ShowLessonResults();
                return;
            }
            // Move to next assignment
            SelectedAssignment = Assignments[_currentAssignmentIndex];
            Debug.WriteLine($"Moved to assignment {_currentAssignmentIndex + 1}/{Assignments.Count}");
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
                // Navigeer naar de volgende les in TTS mode
                await Shell.Current.GoToAsync($"..?lessonId={nextLesson.Id}");
            }
            else
            {
                // Geen volgende les, ga terug naar home
                await Shell.Current.Navigation.PopToRootAsync();
            }
        }

        #endregion

        #region Results

        private async void ShowLessonResults()
        {
            // Toon custom popup
            if (Application.Current?.MainPage != null && SelectedLesson != null)
            {
                var progress = _ResultService.GetProgress(SelectedLesson.Id);
                if (progress == null)
                    return;

                // Convert LessonProgress to Result
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

            // Reset typing
            ResetTyping();
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

        #endregion

        #region Commands

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
        private void Refresh()
        {
            RepeatSpeech().ConfigureAwait(false);
        }

        [RelayCommand]
        private async Task RepeatSpeech()
        {
            _ttsService.Cancel();
            await SpeakCurrentMaterialAsync();
        }

        [RelayCommand]
        private async Task StopSpeech()
        {
            _ttsService.Cancel();
        }

        #endregion
    }
}