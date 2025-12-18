using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.ViewModels
{
    public class LessonResultViewModel
    {
        private readonly Result _currentResult;
        private readonly ScoreComparison? _comparison;

        public LessonResultViewModel(Result currentResult, ScoreComparison? comparison = null)
        {
            _currentResult = currentResult;
            _comparison = comparison;
        }

        // Current result properties
        public int Score => _currentResult.Score;
        public int StrokesPerMinute => _currentResult.StrokesPerMinute;
        public int WordsPerMinute => _currentResult.WordsPerMinute;
        public string Accuracy => $"{Math.Round(_currentResult.AccuracyPercent, 1)}%";
        public int TotalMistakes => _currentResult.TotalMistakes;

        public string TimeRemaining
        {
            get
            {
                if (_currentResult.TimerExpired)
                {
                    return "00:00";
                }

                var timeSpan = TimeSpan.FromSeconds(_currentResult.TimeRemaining);
                return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
        }

        public string ResultTitle => _currentResult.Score > 0 ? "Les Voltooid!" : "Les Gefaald";

        // Comparison properties
        public bool HasPreviousAttempt => _comparison != null && !_comparison.IsFirstAttempt && _comparison.PreviousBest != null;
        public bool IsNewPersonalBest => _comparison?.IsNewPersonalBest ?? false;
        public bool ShowFirstAttemptBadge => _comparison?.IsFirstAttempt ?? true;
        public bool ShowComparisonBadge => HasPreviousAttempt && IsNewPersonalBest;

        public string ComparisonTitle
        {
            get
            {
                if (ShowFirstAttemptBadge)
                    return "🎯 Eerste poging!";
                if (IsNewPersonalBest)
                    return "🎉 Nieuw persoonlijk record!";
                return "Vergelijking met vorige beste poging";
            }
        }

        public string ScoreDifferenceText
        {
            get
            {
                if (!HasPreviousAttempt) return "";
                var diff = _comparison!.ScoreDifference;
                if (diff > 0) return $"(+{diff})";
                if (diff < 0) return $"({diff})";
                return "(=)";
            }
        }

        public string SpeedDifferenceText
        {
            get
            {
                if (!HasPreviousAttempt) return "";
                var diff = _comparison!.SpeedDifference;
                if (diff > 0) return $"(+{diff})";
                if (diff < 0) return $"({diff})";
                return "(=)";
            }
        }

        public string AccuracyDifferenceText
        {
            get
            {
                if (!HasPreviousAttempt) return "";
                var diff = _comparison!.AccuracyDifference;
                if (diff > 0) return $"(+{diff:F1}%)";
                if (diff < 0) return $"({diff:F1}%)";
                return "(=)";
            }
        }

        public Color ScoreDifferenceColor
        {
            get
            {
                if (!HasPreviousAttempt) return Colors.Gray;
                var diff = _comparison!.ScoreDifference;
                if (diff > 0) return Colors.Green;
                if (diff < 0) return Colors.Red;
                return Colors.Gray;
            }
        }

        public Color SpeedDifferenceColor
        {
            get
            {
                if (!HasPreviousAttempt) return Colors.Gray;
                var diff = _comparison!.SpeedDifference;
                if (diff > 0) return Colors.Green;
                if (diff < 0) return Colors.Red;
                return Colors.Gray;
            }
        }

        public Color AccuracyDifferenceColor
        {
            get
            {
                if (!HasPreviousAttempt) return Colors.Gray;
                var diff = _comparison!.AccuracyDifference;
                if (diff > 0) return Colors.Green;
                if (diff < 0) return Colors.Red;
                return Colors.Gray;
            }
        }

        public int PreviousBestScore => _comparison?.PreviousBest?.Score ?? 0;
        public int PreviousBestWPM => _comparison?.PreviousBest?.WordsPerMinute ?? 0;
        public string PreviousBestAccuracy => _comparison?.PreviousBest != null
            ? $"{Math.Round(_comparison.PreviousBest.AccuracyPercent, 1)}%"
            : "N/A";
    }
}