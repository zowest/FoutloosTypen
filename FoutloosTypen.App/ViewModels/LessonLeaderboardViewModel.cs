using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using System.Diagnostics;

namespace FoutloosTypen.ViewModels
{
    [QueryProperty(nameof(LessonId), "lessonId")]
    public partial class LessonLeaderboardViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly IResultService _resultService;
        private readonly ILessonService _lessonService;
        private readonly IStudentService _studentService;
        private readonly GlobalViewModel _global;

        public ObservableCollection<LeaderboardEntry> TopResults { get; set; } = new();

        private int _lessonId;
        public int LessonId
        {
            get => _lessonId;
            set
            {
                Debug.WriteLine($"[LessonLeaderboard] LessonId setter called: old={_lessonId}, new={value}");
                if (SetProperty(ref _lessonId, value))
                {
                    Debug.WriteLine($"[LessonLeaderboard] LessonId changed, calling LoadLeaderboard()");
                    LoadLeaderboard();
                }
                else
                {
                    Debug.WriteLine($"[LessonLeaderboard] LessonId not changed, skipping LoadLeaderboard()");
                }
            }
        }

        private string _lessonName = string.Empty;
        public string LessonName
        {
            get => _lessonName;
            set => SetProperty(ref _lessonName, value);
        }

        private LeaderboardEntry? _currentStudentResult;
        public LeaderboardEntry? CurrentStudentResult
        {
            get => _currentStudentResult;
            set => SetProperty(ref _currentStudentResult, value);
        }

        private int _currentStudentRank;
        public int CurrentStudentRank
        {
            get => _currentStudentRank;
            set => SetProperty(ref _currentStudentRank, value);
        }

        public LessonLeaderboardViewModel(
            IResultService resultService,
            ILessonService lessonService,
            IStudentService studentService,
            GlobalViewModel global)
        {
            _resultService = resultService;
            _lessonService = lessonService;
            _studentService = studentService;
            _global = global;
            Debug.WriteLine("[LessonLeaderboard] ViewModel constructed");
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine($"[LessonLeaderboard] OnAppearing called, LessonId={LessonId}");
            LoadLeaderboard();
        }

        private void LoadLeaderboard()
        {
            Debug.WriteLine($"[LessonLeaderboard] LoadLeaderboard called, LessonId={LessonId}");

            if (LessonId == 0)
            {
                Debug.WriteLine("[LessonLeaderboard] LessonId is 0, returning early");
                return;
            }

            try
            {
                var lesson = _lessonService.Get(LessonId);
                LessonName = lesson?.Name ?? $"Les {LessonId}";
                Debug.WriteLine($"[LessonLeaderboard] Loading leaderboard for lesson {LessonId}: {LessonName}");

                var results = _resultService.GetLessonLeaderboard(LessonId, 10);
                Debug.WriteLine($"[LessonLeaderboard] Found {results.Count} results");

                TopResults.Clear();
                int rank = 1;
                foreach (var result in results)
                {
                    var student = _studentService.Get(result.StudentId);
                    if (student != null)
                    {
                        var entry = new LeaderboardEntry
                        {
                            Rank = rank++,
                            StudentName = student.Name,
                            Speed = result.WordsPerMinute,
                            Accuracy = result.AccuracyPercent,
                            Score = result.Score,
                            IsCurrentUser = _global.Student?.Id == student.Id
                        };
                        TopResults.Add(entry);
                        Debug.WriteLine($"[LessonLeaderboard] Added: Rank {entry.Rank}, {entry.StudentName}, Score {entry.Score}");
                    }
                }

                Debug.WriteLine($"[LessonLeaderboard] Total entries in TopResults: {TopResults.Count}");

                if (_global.Student != null)
                {
                    var bestResult = _resultService.GetStudentBestResult(LessonId, _global.Student.Id);
                    if (bestResult != null)
                    {
                        var studentRankIndex = results.FindIndex(r => r.StudentId == _global.Student.Id);
                        CurrentStudentRank = studentRankIndex >= 0 ? studentRankIndex + 1 : results.Count + 1;

                        CurrentStudentResult = new LeaderboardEntry
                        {
                            Rank = CurrentStudentRank,
                            StudentName = _global.Student.Name,
                            Speed = bestResult.WordsPerMinute,
                            Accuracy = bestResult.AccuracyPercent,
                            Score = bestResult.Score,
                            IsCurrentUser = true
                        };
                        Debug.WriteLine($"[LessonLeaderboard] Current student rank: {CurrentStudentRank}");
                    }
                    else
                    {
                        Debug.WriteLine($"[LessonLeaderboard] No best result found for student {_global.Student.Id}");
                    }
                }
                else
                {
                    Debug.WriteLine("[LessonLeaderboard] No student logged in");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LessonLeaderboard] ERROR: {ex.Message}");
                Debug.WriteLine($"[LessonLeaderboard] StackTrace: {ex.StackTrace}");
            }
        }

        [RelayCommand]
        private async Task NavigateHome()
        {
            Debug.WriteLine("[LessonLeaderboard] NavigateHome command");
            await Shell.Current.GoToAsync("//LessonView");
        }

        [RelayCommand]
        private async Task ViewGeneralLeaderboard()
        {
            Debug.WriteLine("[LessonLeaderboard] ViewGeneralLeaderboard command");
            await Shell.Current.GoToAsync("//LeaderboardView");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}