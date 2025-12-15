using FoutloosTypen.Core.Models;
using System.ComponentModel;

namespace FoutloosTypen.ViewModels
{
    public class LessonResultViewModel : INotifyPropertyChanged
    {
        private readonly LessonProgress _progress;

        public LessonResultViewModel(LessonProgress progress)
        {
            _progress = progress;
        }

        public int Score
        {
            get
            {
                // Calculate score based on accuracy and time
                double accuracyScore = AccuracyPercent * 10; // Max 1000 points for 100% accuracy
                double timeBonus = _progress.TimeRemaining * 7; // Bonus for time remaining
                return (int)((accuracyScore + timeBonus)*10);
            }
        }

        public int StrokesPerMinute
        {
            get
            {
                if (_progress.TimeSpent == 0) return 0;
                
                double minutes = _progress.TimeSpent / 60.0;
                int totalCharacters = _progress.TotalCharactersTyped;
                
                return totalCharacters > 0 ? (int)(totalCharacters / minutes) : 0;
            }
        }

        public int WordsPerMinute
        {
            get
            {
                // Temporary solution until data layer is finished
                return StrokesPerMinute / 5;
            }
        }

        private double AccuracyPercent
        {
            get
            {
                if (_progress.TotalCharactersTyped == 0) return 0;
                
                double accuracyPercent = ((double)(_progress.TotalCharactersTyped - _progress.TotalMistakes) / _progress.TotalCharactersTyped) * 100;
                return Math.Max(0, accuracyPercent);
            }
        }

        public string Accuracy => $"{Math.Round(AccuracyPercent, 1)}%";

        public int TotalMistakes => _progress.TotalMistakes;

        public string TimeRemaining
        {
            get
            {
                var timeSpan = TimeSpan.FromSeconds(_progress.TimeRemaining);
                return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}