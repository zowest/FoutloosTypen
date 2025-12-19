namespace FoutloosTypen.Core.Models
{
    public class Result
    {
        public int LessonId { get; set; }
        public int TotalMistakes { get; set; } = 0;
        public int SentencesCompleted { get; set; } = 0;
        public int TotalCharactersTyped { get; set; } = 0;
        public List<string> CompletedSentences { get; set; } = new();
        public string CurrentIncompleteText { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double ExpectedTime { get; set; } = 300;
        public bool TimerExpired { get; set; } = false;
        public int StudentId { get; set; }
        public bool IsEndlessMode { get; set; } = false; // Add this

        // Calculated properties
        public int Score { get; set; }
        public int StrokesPerMinute { get; set; }
        public int WordsPerMinute { get; set; }
        public double AccuracyPercent { get; set; }
        public double TimeRemaining { get; set; }

        public double TimeSpent 
        {
            get
            {
                var endTime = EndTime ?? DateTime.Now;
                var timeSpent = (endTime - StartTime).TotalSeconds;
                System.Diagnostics.Debug.WriteLine($"TimeSpent calculation: Start={StartTime}, End={endTime}, Spent={timeSpent}s");
                return timeSpent;
            }
        }
    }
}