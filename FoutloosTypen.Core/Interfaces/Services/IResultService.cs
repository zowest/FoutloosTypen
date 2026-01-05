using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IResultService
    {
        void StartLesson(int lessonId, int totalAssignments);
        void CompleteSentence(int lessonId, string sentence);
        void RecordMistake(int lessonId);
        void UpdateCurrentProgress(int lessonId, int charactersTyped, string currentText);
        void EndLesson(int lessonId);
        void MarkTimerExpired(int lessonId);
        LessonProgress? GetProgress(int lessonId);
        
        // Leaderboard methods using Result
        void SaveResult(int lessonId, int studentId, LessonProgress progress);
        List<Result> GetLessonLeaderboard(int lessonId, int limit = 10);
        List<Result> GetOverallLeaderboard(int limit = 10);
        Result? GetStudentBestResult(int lessonId, int studentId);
        Result? GetStudentPreviousResult(int lessonId, int studentId); // NEW
        ScoreComparison CompareWithPrevious(int lessonId, int studentId, Result currentResult); // NEW
    }

    public class LessonProgress
    {
        public int LessonId { get; set; }
        public int TotalAssignments { get; set; }
        public int SentencesCompleted { get; set; }
        public int CharactersTyped { get; set; }
        public int TotalMistakes { get; set; }
        public double TimeSpent { get; set; }
        public DateTime StartTime { get; set; }
        public bool TimerExpired { get; set; }
        public string CurrentText { get; set; } = string.Empty;

        // Calculated properties
        public double Speed => TimeSpent > 0 ? (CharactersTyped / 5.0) / (TimeSpent / 60.0) : 0; // WPM
        public double Accuracy => CharactersTyped > 0 ? ((CharactersTyped - TotalMistakes) / (double)CharactersTyped) * 100 : 100;
        public int Score => (int)(Speed * (Accuracy / 100.0) * 10);
        public int StrokesPerMinute => TimeSpent > 0 ? (int)(CharactersTyped / (TimeSpent / 60.0)) : 0;
    }

    // NEW CLASS
    public class ScoreComparison
    {
        public Result? PreviousBest { get; set; }
        public bool IsNewPersonalBest { get; set; }
        public int ScoreDifference { get; set; }
        public int SpeedDifference { get; set; }
        public double AccuracyDifference { get; set; }
        public bool IsFirstAttempt { get; set; }
    }
}