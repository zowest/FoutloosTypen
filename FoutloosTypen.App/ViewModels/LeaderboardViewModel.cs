using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.ViewModels
{
    public partial class LeaderboardViewModel : BaseViewModel
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly GlobalViewModel _global;

        public ObservableCollection<LeaderboardEntry> TopStudents { get; set; } = new();

        private LeaderboardCategory _selectedCategory = LeaderboardCategory.Speed;
        public LeaderboardCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    LoadLeaderboard();
                }
            }
        }

        private int _currentStudentRank;
        public int CurrentStudentRank
        {
            get => _currentStudentRank;
            set => SetProperty(ref _currentStudentRank, value);
        }

        private string _categoryTitle = "Snelheid (WPM)";
        public string CategoryTitle
        {
            get => _categoryTitle;
            set => SetProperty(ref _categoryTitle, value);
        }

        public LeaderboardViewModel(ILeaderboardService leaderboardService, GlobalViewModel global)
        {
            _leaderboardService = leaderboardService;
            _global = global;
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            LoadLeaderboard();
        }

        private void LoadLeaderboard()
        {
            var students = _leaderboardService.GetLeaderboard(SelectedCategory, 10);

            TopStudents.Clear();
            int rank = 1;
            
            foreach (var student in students)
            {
                var entry = new LeaderboardEntry
                {
                    Rank = rank++,
                    Name = student.Name,
                    StudentId = student.Id
                };

                // Set the correct value based on category
                switch (SelectedCategory)
                {
                    case LeaderboardCategory.Speed:
                        entry.Value = student.AvgSpeed;
                        entry.FormattedValue = $"{student.AvgSpeed:F1}";
                        break;
                    case LeaderboardCategory.Precision:
                        entry.Value = student.AvgPrecision;
                        entry.FormattedValue = $"{student.AvgPrecision:F1}%";
                        break;
                    case LeaderboardCategory.Score:
                        entry.Value = student.TotalScore;
                        entry.FormattedValue = student.TotalScore.ToString();
                        break;
                    case LeaderboardCategory.CompletedLessons:
                        entry.Value = student.CompletedLessons;
                        entry.FormattedValue = student.CompletedLessons.ToString();
                        break;
                }

                TopStudents.Add(entry);
            }

            // Update category title
            CategoryTitle = SelectedCategory switch
            {
                LeaderboardCategory.Speed => "Snelheid (WPM)",
                LeaderboardCategory.Precision => "Nauwkeurigheid (%)",
                LeaderboardCategory.Score => "Totale Score",
                LeaderboardCategory.CompletedLessons => "Voltooide Lessen",
                _ => "Leaderboard"
            };

            // Get current student rank if logged in
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
    }

    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string Name { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public double Value { get; set; }
        public string FormattedValue { get; set; } = string.Empty;
    }
}