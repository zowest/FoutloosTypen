using FoutloosTypen.Core.Models;
using System.ComponentModel;

namespace FoutloosTypen.ViewModels
{
    public class LessonResultViewModel : INotifyPropertyChanged
    {
        private readonly Result _progress;

        public LessonResultViewModel(Result progress)
        {
            _progress = progress;
        }

        // All calculations are now done in the service, just expose the values
        public int Score => _progress.Score;

        public int StrokesPerMinute => _progress.StrokesPerMinute;

        public int WordsPerMinute => _progress.WordsPerMinute;

        public string Accuracy => $"{Math.Round(_progress.AccuracyPercent, 1)}%";

        public int TotalMistakes => _progress.TotalMistakes;

        public string TimeRemaining
        {
            get
            {
                if (_progress.TimerExpired)
                {
                    return "00:00";
                }

                var timeSpan = TimeSpan.FromSeconds(_progress.TimeRemaining);
                return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
        }
        public string ResultTitle => _progress.Score > 0 ? "Les Voltooid!" : "Les Gefaald";

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}