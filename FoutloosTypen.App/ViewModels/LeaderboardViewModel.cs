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
    public partial class LeaderboardViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly IResultService _resultService;
        private readonly ILessonService _lessonService;
        private readonly IStudentService _studentService;
        private readonly GlobalViewModel _global;

        public ObservableCollection<Student> TopStudents { get; set; } = new();
        public ObservableCollection<LeaderboardEntry> LessonResults { get; set; } = new();

        private int _lessonId;
        public int LessonId
        {
            get => _lessonId;
            set
            {
                if (SetProperty(ref _lessonId, value))
                {
                    IsLessonSpecific = value > 0;
                    LoadLeaderboardAsync();
                }
            }
        }

        private bool _isLessonSpecific;
        public bool IsLessonSpecific
        {
            get => _isLessonSpecific;
            set => SetProperty(ref _isLessonSpecific, value);
        }

        private string _lessonName = string.Empty;
        public string LessonName
        {
            get => _lessonName;
            set => SetProperty(ref _lessonName, value);
        }

        private LeaderboardCategory _selectedCategory = LeaderboardCategory.Speed;
        public LeaderboardCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    LoadLeaderboardAsync();
                }
            }
        }

        private int _currentStudentRank;
        public int CurrentStudentRank
        {
            get => _currentStudentRank;
            set => SetProperty(ref _currentStudentRank, value);
        }

        private string _categoryTitle = "Snelheid";
        public string CategoryTitle
        {
            get => _categoryTitle;
            set => SetProperty(ref _categoryTitle, value);
        }

        private string _pageTitle = "Klassement";
        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

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

        public override void OnAppearing()
        {
            base.OnAppearing();
            LoadLeaderboardAsync();
        }

        private void LoadLeaderboardAsync()
        {
            if (IsLessonSpecific)
            {
                LoadLessonLeaderboard();
            }
            else
            {
                LoadGeneralLeaderboard();
            }
        }

        private void LoadLessonLeaderboard()
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

        private void LoadGeneralLeaderboard()
        {
            PageTitle = "Algemeen Klassement";

            var students = _leaderboardService.GetLeaderboard(SelectedCategory, 10);

            TopStudents.Clear();
            foreach (var student in students)
            {
                TopStudents.Add(student);
            }

            CategoryTitle = SelectedCategory switch
            {
                LeaderboardCategory.Speed => "Snelheid (WPM)",
                LeaderboardCategory.Precision => "Nauwkeurigheid (%)",
                LeaderboardCategory.Score => "Totale Score",
                LeaderboardCategory.CompletedLessons => "Voltooide Lessen",
                _ => "Leaderboard"
            };

            if (_global.Student != null)
            {
                CurrentStudentRank = _leaderboardService.GetStudentRank(_global.Student.Id, SelectedCategory);
            }
        }

        [RelayCommand]
        private void SelectCategorySpeed()
        {
            SelectedCategory = LeaderboardCategory.Speed;
        }

        [RelayCommand]
        private void SelectCategoryPrecision()
        {
            SelectedCategory = LeaderboardCategory.Precision;
        }

        [RelayCommand]
        private void SelectCategoryScore()
        {
            SelectedCategory = LeaderboardCategory.Score;
        }

        [RelayCommand]
        private void SelectCategoryLessons()
        {
            SelectedCategory = LeaderboardCategory.CompletedLessons;
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