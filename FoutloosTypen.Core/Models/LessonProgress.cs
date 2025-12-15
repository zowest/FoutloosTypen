namespace FoutloosTypen.Core.Models
{
    public class LessonProgress
    {
        public int LessonId { get; set; }
        public int TotalMistakes { get; set; } = 0;
        public int SentencesCompleted { get; set; } = 0;
        public int TotalCharactersTyped { get; set; } = 0;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double ExpectedTime { get; set; } = 300; // 5 minutes default
        public bool TimerExpired { get; set; } = false;
        
        // Calculated properties
        public int Score { get; set; }
        public int StrokesPerMinute { get; set; }
        public int WordsPerMinute { get; set; }
        public double AccuracyPercent { get; set; }
        
        public double TimeSpent => EndTime.HasValue 
            ? (EndTime.Value - StartTime).TotalSeconds 
            : (DateTime.Now - StartTime).TotalSeconds;
            
        public double TimeRemaining => Math.Max(0, ExpectedTime - TimeSpent);
    }
}