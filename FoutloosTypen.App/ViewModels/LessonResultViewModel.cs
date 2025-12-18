using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using FoutloosTypen.App.Models; // ADD THIS

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
                if (SetProperty(ref _lessonId, value))
                {
                    LoadLeaderboard();
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
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            LoadLeaderboard();
        }

        private void LoadLeaderboard()
        {
            if (LessonId == 0) return;

            var lesson = _lessonService.Get(LessonId);
            LessonName = lesson?.Name ?? $"Les {LessonId}";

            var results = _resultService.GetLessonLeaderboard(LessonId, 10);

            TopResults.Clear();
            int rank = 1;
            foreach (var result in results)
            {
                var student = _studentService.Get(result.StudentId);
                if (student != null)
                {
                    TopResults.Add(new LeaderboardEntry
                    {
                        Rank = rank++,
                        StudentName = student.Name,
                        Speed = result.WordsPerMinute,
                        Accuracy = result.AccuracyPercent,
                        Score = result.Score,
                        IsCurrentUser = _global.Student?.Id == student.Id
                    });
                }
            }

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
                }
            }
        }

        [RelayCommand]
        private async Task NavigateHome()
        {
            await Shell.Current.GoToAsync("//LessonView");
        }

        [RelayCommand]
        private async Task ViewGeneralLeaderboard()
        {
            await Shell.Current.GoToAsync("//LeaderboardView");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
    
    // REMOVED: LeaderboardEntry class - now in separate file
}