using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using FoutloosTypen.Core.Interfaces.Services;

namespace FoutloosTypen.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly GlobalViewModel _global;
        private readonly IStudentService _studentService;
        private readonly IResultService _resultService;

        [ObservableProperty]
        private string _studentName = string.Empty;

        [ObservableProperty]
        private string _averageWordsPerMinute = "0";

        [ObservableProperty]
        private string _averageStrokesPerMinute = "0";

        [ObservableProperty]
        private string _averageAccuracy = "0%";

        [ObservableProperty]
        private string _averageSessionMistakes = "0";

        [ObservableProperty]
        private int _completedLessons = 0;

        [ObservableProperty]
        private int _totalScore = 0;

        public ProfileViewModel(GlobalViewModel global, IStudentService studentService, IResultService resultService)
        {
            _global = global;
            _studentService = studentService;
            _resultService = resultService;
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            LoadStudentProfile();
        }

        private void LoadStudentProfile()
        {
            try
            {
                if (_global.Student == null)
                {
                    Debug.WriteLine("[ProfileViewModel] No student logged in");
                    return;
                }

                // Refresh student data from database to get latest stats
                var student = _studentService.Get(_global.Student.Id);
                if (student == null)
                {
                    Debug.WriteLine($"[ProfileViewModel] Student {_global.Student.Id} not found in database");
                    return;
                }

                // Update GlobalViewModel with fresh data
                _global.Student = student;

                // Set basic info
                StudentName = student.Name;
                CompletedLessons = student.CompletedLessons;
                TotalScore = student.TotalScore;

                // Get all results for this student to calculate averages
                var results = _resultService.GetOverallLeaderboard(int.MaxValue)
                    .Where(r => r.StudentId == student.Id)
                    .ToList();

                if (results.Any())
                {
                    // Calculate averages from actual results
                    double avgWpm = results.Average(r => r.WordsPerMinute);
                    double avgSpm = results.Average(r => r.StrokesPerMinute);
                    double avgAccuracy = results.Average(r => r.AccuracyPercent);
                    double avgMistakes = results.Average(r => r.TotalMistakes);

                    AverageWordsPerMinute = $"{avgWpm:F1}";
                    AverageStrokesPerMinute = $"{avgSpm:F1}";
                    AverageAccuracy = $"{avgAccuracy:F1}%";
                    AverageSessionMistakes = $"{avgMistakes:F1}";

                    Debug.WriteLine($"[ProfileViewModel] Loaded profile for {student.Name}: WPM={avgWpm:F1}, Accuracy={avgAccuracy:F1}%");
                }
                else
                {
                    // No results yet - use student stored averages or defaults
                    AverageWordsPerMinute = student.AvgSpeed > 0 ? $"{student.AvgSpeed:F1}" : "0";
                    AverageStrokesPerMinute = "0"; // Not stored in student table
                    AverageAccuracy = student.AvgPrecision > 0 ? $"{student.AvgPrecision:F1}%" : "0%";
                    AverageSessionMistakes = "0";

                    Debug.WriteLine($"[ProfileViewModel] No results found for {student.Name}, using stored averages");
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[ProfileViewModel] Error loading profile: {ex.Message}");
                // Set default values on error
                StudentName = _global.Student?.Name ?? "Onbekend";
                AverageWordsPerMinute = "0";
                AverageStrokesPerMinute = "0";
                AverageAccuracy = "0%";
                AverageSessionMistakes = "0";
            }
        }
    }
}
