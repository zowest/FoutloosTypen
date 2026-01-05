using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Views;

namespace FoutloosTypen.ViewModels
{
    [QueryProperty(nameof(LessonId), "lessonId")]
    public partial class LeaderboardViewModel : BaseViewModel
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly IResultService _resultService;
        private readonly ILessonService _lessonService;
        private readonly IStudentService _studentService;
        private readonly GlobalViewModel _global;

        // Display model used in the UI so we can expose Rank easily
        public class LeaderboardStudent
        {
            public int Id { get; set; }
            public int Rank { get; set; }
            public string Name { get; set; } = string.Empty;
            public int TotalScore { get; set; }
            public bool IsCurrentUser { get; set; }
        }

        public ObservableCollection<LeaderboardStudent> TopStudents { get; set; } = new();
        public ObservableCollection<LeaderboardStudent> EndlessResults { get; set; } = new();
        public ObservableCollection<LeaderboardEntry> LessonResults { get; set; } = new();

        [ObservableProperty]
        private int _lessonId;

        [ObservableProperty]
        private bool _isLessonSpecific;

        [ObservableProperty]
        private string _lessonName = string.Empty;

        [ObservableProperty]
        private int _currentStudentRank;

        [ObservableProperty]
        private string _pageTitle = "Klassement";

        // Tabs
        public enum LeaderboardTab { TotalScore, Endless }
        
        [ObservableProperty]
        private LeaderboardTab _selectedTab = LeaderboardTab.TotalScore;

        public bool IsTotalScoreTabSelected => SelectedTab == LeaderboardTab.TotalScore;
        public bool IsEndlessTabSelected => SelectedTab == LeaderboardTab.Endless;

        public LeaderboardViewModel(
            ILeaderboardService leaderboardService,
            IResultService resultService,
            ILessonService lessonService,
            IStudentService studentService,
            GlobalViewModel global)
        {
            _leaderboardService = leaderboardService;
            _resultService = resultService;
            _lessonService = lessonService;
            _studentService = studentService;
            _global = global;
        }

        partial void OnLessonIdChanged(int value)
        {
            IsLessonSpecific = value > 0;
            LoadLeaderboardAsync();
        }

        partial void OnSelectedTabChanged(LeaderboardTab value)
        {
            OnPropertyChanged(nameof(IsTotalScoreTabSelected));
            OnPropertyChanged(nameof(IsEndlessTabSelected));
            LoadLeaderboardAsync();
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            LoadLeaderboardAsync();
        }

        private void LoadLeaderboardAsync()
        {
            try
            {
                if (IsLessonSpecific)
                {
                    LoadLessonLeaderboard();
                    return;
                }

                if (SelectedTab == LeaderboardTab.TotalScore)
                    LoadTotalScoreLeaderboard();
                else
                    LoadEndlessLeaderboard();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[LeaderboardViewModel] LoadLeaderboardAsync failed: {ex}");
                TopStudents.Clear();
                EndlessResults.Clear();
                LessonResults.Clear();
                PageTitle = "Klassement (error)";
            }
        }

        private void LoadLessonLeaderboard()
        {
            try
            {
                var lesson = _lessonService.Get(LessonId);
                LessonName = lesson?.Name ?? $"Les {LessonId}";
                PageTitle = $"Klassement - {LessonName}";

                var results = _resultService.GetLessonLeaderboard(LessonId, 10);

                LessonResults.Clear();
                int rank = 1;
                foreach (var result in results)
                {
                    var student = _studentService.Get(result.StudentId);
                    if (student != null)
                    {
                        LessonResults.Add(new LeaderboardEntry
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
                    }
                    else
                    {
                        CurrentStudentRank = 0;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[LeaderboardViewModel] LoadLessonLeaderboard failed: {ex}");
                LessonResults.Clear();
                PageTitle = "Les klassement (error)";
            }
        }

        private void LoadTotalScoreLeaderboard()
        {
            PageTitle = "Algemeen Klassement";

            var students = _studentService.GetAll() ?? new List<Student>();
            var ordered = students.OrderByDescending(s => s.TotalScore).ToList();

            TopStudents.Clear();
            int rank = 1;
            foreach (var s in ordered)
            {
                TopStudents.Add(new LeaderboardStudent
                {
                    Id = s.Id,
                    Rank = rank++,
                    Name = s.Name,
                    TotalScore = s.TotalScore,
                    IsCurrentUser = _global.Student?.Id == s.Id
                });
            }

            if (_global.Student != null)
            {
                var idx = ordered.FindIndex(s => s.Id == _global.Student.Id);
                CurrentStudentRank = idx >= 0 ? idx + 1 : 0;
            }
            else
            {
                CurrentStudentRank = 0;
            }
        }

        private void LoadEndlessLeaderboard()
        {
            PageTitle = "Algemeen Klassement - Endless Mode";
            EndlessResults.Clear();
            CurrentStudentRank = 0;
            Debug.WriteLine("[LeaderboardViewModel] Endless leaderboard placeholder");
        }

        [RelayCommand]
        private void SelectTotalScoreTab() => SelectedTab = LeaderboardTab.TotalScore;

        [RelayCommand]
        private void SelectEndlessTab() => SelectedTab = LeaderboardTab.Endless;

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateHome()
        {
            await Microsoft.Maui.Controls.Shell.Current.GoToAsync("//LessonView");
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateSettings()
        {
            await Microsoft.Maui.Controls.Shell.Current.GoToAsync(nameof(SettingsView));
        }
    }
}