namespace FoutloosTypen.Core.Models
{
    public class LessonProgress
    {
        public int LessonId { get; set; }
        public int TotalMistakes { get; set; } = 0;
        public int SentencesCompleted { get; set; } = 0;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double TimeSpent => EndTime.HasValue 
            ? (EndTime.Value - StartTime).TotalSeconds 
            : (DateTime.Now - StartTime).TotalSeconds;
    }
}