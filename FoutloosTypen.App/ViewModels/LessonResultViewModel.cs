using CommunityToolkit.Mvvm.ComponentModel;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.ViewModels
{
    public partial class LessonResultViewModel : ObservableObject
    {
        private readonly Result _currentResult;
        private readonly ScoreComparison? _comparison;

        public Result CurrentResult => _currentResult;
        public ScoreComparison? Comparison => _comparison;

        // Basic Result Properties
        public string LessonName { get; set; } = string.Empty;
        public int Score => _currentResult.Score;
        public int WordsPerMinute => _currentResult.WordsPerMinute;
        public string Accuracy => $"{_currentResult.AccuracyPercent:F1}%";
        public int StrokesPerMinute => _currentResult.StrokesPerMinute;
        public int TotalMistakes => _currentResult.TotalMistakes;
        public string TimeRemaining => FormatTime(_currentResult.TimeRemaining);

        // Title
        public string ResultTitle => _comparison?.IsFirstAttempt == true
            ? "Eerste Poging!"
            : (_comparison?.IsNewPersonalBest == true ? "Nieuw Record!" : "Les Voltooid!");

        // Badge visibility
        public bool ShowFirstAttemptBadge => _comparison?.IsFirstAttempt == true;
        public bool ShowComparisonBadge => _comparison?.IsNewPersonalBest == true && _comparison?.IsFirstAttempt == false;
        public bool HasPreviousAttempt => _comparison?.PreviousBest != null && !(_comparison?.IsFirstAttempt == true);

        // Previous Best Properties
        public int PreviousBestScore => _comparison?.PreviousBest?.Score ?? 0;
        public int PreviousBestWPM => _comparison?.PreviousBest?.WordsPerMinute ?? 0;
        public string PreviousBestAccuracy => $"{_comparison?.PreviousBest?.AccuracyPercent ?? 0:F1}%";

        // Difference Properties with Colors
        public string ScoreDifferenceText
        {
            get
            {
                if (_comparison == null || _comparison.IsFirstAttempt) return string.Empty;
                var diff = _comparison.ScoreDifference;
                if (diff == 0) return string.Empty;
                return diff > 0 ? $"+{diff}" : $"{diff}";
            }
        }

        public Color ScoreDifferenceColor
        {
            get
            {
                if (_comparison == null) return Colors.Gray;
                return _comparison.ScoreDifference > 0 ? Colors.Green :
                       _comparison.ScoreDifference < 0 ? Colors.Red : Colors.Gray;
            }
        }

        public string SpeedDifferenceText
        {
            get
            {
                if (_comparison == null || _comparison.IsFirstAttempt) return string.Empty;
                var diff = _comparison.SpeedDifference;
                if (diff == 0) return string.Empty;
                return diff > 0 ? $"+{diff}" : $"{diff}";
            }
        }

        public Color SpeedDifferenceColor
        {
            get
            {
                if (_comparison == null) return Colors.Gray;
                return _comparison.SpeedDifference > 0 ? Colors.Green :
                       _comparison.SpeedDifference < 0 ? Colors.Red : Colors.Gray;
            }
        }

        public string AccuracyDifferenceText
        {
            get
            {
                if (_comparison == null || _comparison.IsFirstAttempt) return string.Empty;
                var diff = _comparison.AccuracyDifference;
                if (Math.Abs(diff) < 0.1) return string.Empty;
                return diff > 0 ? $"+{diff:F1}%" : $"{diff:F1}%";
            }
        }

        public Color AccuracyDifferenceColor
        {
            get
            {
                if (_comparison == null) return Colors.Gray;
                return _comparison.AccuracyDifference > 0 ? Colors.Green :
                       _comparison.AccuracyDifference < 0 ? Colors.Red : Colors.Gray;
            }
        }

        public LessonResultViewModel(Result currentResult, ScoreComparison? comparison = null)
        {
            _currentResult = currentResult ?? throw new ArgumentNullException(nameof(currentResult));
            _comparison = comparison;
        }

        private string FormatTime(double seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s";
            return $"{(int)seconds}s";
        }
    }
}