using System.Data.Common;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Repositories;
using System.Diagnostics;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class ResultRepository : DatabaseConnection, IResultRepository
    {
        public ResultRepository()
        {
            CreateTable("""
                CREATE TABLE IF NOT EXISTS Results (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    StudentId INT NOT NULL,
                    LessonId INT NOT NULL,
                    StrokesPerMinute INT NOT NULL,
                    WordsPerMinute INT NOT NULL,
                    TotalMistakes INT NOT NULL,
                    Score INT NOT NULL,
                    TimeRemaining DOUBLE NOT NULL,
                    AccuracyPercent DOUBLE NOT NULL,
                    SentencesCompleted INT NOT NULL DEFAULT 0,
                    TotalCharactersTyped INT NOT NULL DEFAULT 0,
                    StartTime DATETIME NOT NULL,
                    EndTime DATETIME NULL,
                    TimerExpired TINYINT(1) DEFAULT 0
                );
            """);
        }

        public void Save(Result result)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            // CHANGED: Simple INSERT without ON DUPLICATE KEY UPDATE
            command.CommandText = """
                INSERT INTO Results
                (StudentId, LessonId, StrokesPerMinute, WordsPerMinute,
                 TotalMistakes, Score, TimeRemaining, AccuracyPercent,
                 SentencesCompleted, TotalCharactersTyped, StartTime, EndTime, TimerExpired)
                VALUES
                (@studentId, @lessonId, @spm, @wpm,
                 @mistakes, @score, @timeRemaining, @accuracy,
                 @sentences, @characters, @startTime, @endTime, @timerExpired)
            """;

            AddParam(command, "@studentId", result.StudentId);
            AddParam(command, "@lessonId", result.LessonId);
            AddParam(command, "@spm", result.StrokesPerMinute);
            AddParam(command, "@wpm", result.WordsPerMinute);
            AddParam(command, "@mistakes", result.TotalMistakes);
            AddParam(command, "@score", result.Score);
            AddParam(command, "@timeRemaining", result.TimeRemaining);
            AddParam(command, "@accuracy", result.AccuracyPercent);
            AddParam(command, "@sentences", result.SentencesCompleted);
            AddParam(command, "@characters", result.TotalCharactersTyped);
            AddParam(command, "@startTime", result.StartTime);
            AddParam(command, "@endTime", result.EndTime ?? (object)DBNull.Value);
            AddParam(command, "@timerExpired", result.TimerExpired ? 1 : 0);

            command.ExecuteNonQuery();
            
            Debug.WriteLine($"[ResultRepository] Saved new result for student {result.StudentId}, lesson {result.LessonId}, score {result.Score}");
            
            CloseConnection();
        }

        // CHANGED: Get BEST result (highest score) for leaderboard
        public Result? GetByStudentAndLesson(int studentId, int lessonId)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT Id, StudentId, LessonId, StrokesPerMinute, WordsPerMinute,
                       TotalMistakes, Score, TimeRemaining, AccuracyPercent,
                       SentencesCompleted, TotalCharactersTyped, StartTime, EndTime, TimerExpired
                FROM Results
                WHERE StudentId = @studentId AND LessonId = @lessonId
                ORDER BY Score DESC, EndTime DESC
                LIMIT 1
            """;

            AddParam(command, "@studentId", studentId);
            AddParam(command, "@lessonId", lessonId);

            using DbDataReader reader = command.ExecuteReader();
            Result? result = null;

            if (reader.Read())
                result = Map(reader);

            CloseConnection();
            return result;
        }

        public List<Result> GetByStudent(int studentId)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT Id, StudentId, LessonId, StrokesPerMinute, WordsPerMinute,
                       TotalMistakes, Score, TimeRemaining, AccuracyPercent,
                       SentencesCompleted, TotalCharactersTyped, StartTime, EndTime, TimerExpired
                FROM Results
                WHERE StudentId = @studentId
                ORDER BY EndTime DESC
            """;

            AddParam(command, "@studentId", studentId);

            var results = new List<Result>();
            using DbDataReader reader = command.ExecuteReader();

            while (reader.Read())
                results.Add(Map(reader));

            CloseConnection();
            return results;
        }

        public List<Result> GetByLesson(int lessonId)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT Id, StudentId, LessonId, StrokesPerMinute, WordsPerMinute,
                       TotalMistakes, Score, TimeRemaining, AccuracyPercent,
                       SentencesCompleted, TotalCharactersTyped, StartTime, EndTime, TimerExpired
                FROM Results
                WHERE LessonId = @lessonId
                ORDER BY Score DESC, WordsPerMinute DESC
            """;

            AddParam(command, "@lessonId", lessonId);

            var results = new List<Result>();
            using DbDataReader reader = command.ExecuteReader();

            while (reader.Read())
                results.Add(Map(reader));

            CloseConnection();
            return results;
        }

        public List<Result> GetTopByLesson(int lessonId, int limit = 10)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            // Get best score per student for this lesson
            command.CommandText = $"""
                SELECT r.Id, r.StudentId, r.LessonId, r.StrokesPerMinute, r.WordsPerMinute,
                       r.TotalMistakes, r.Score, r.TimeRemaining, r.AccuracyPercent,
                       r.SentencesCompleted, r.TotalCharactersTyped, r.StartTime, r.EndTime, r.TimerExpired
                FROM Results r
                INNER JOIN (
                    SELECT StudentId, MAX(Score) as BestScore
                    FROM Results
                    WHERE LessonId = @lessonId
                    GROUP BY StudentId
                ) best ON r.StudentId = best.StudentId AND r.Score = best.BestScore
                WHERE r.LessonId = @lessonId
                ORDER BY r.Score DESC, r.WordsPerMinute DESC
                LIMIT {limit}
            """;

            AddParam(command, "@lessonId", lessonId);

            var results = new List<Result>();
            using DbDataReader reader = command.ExecuteReader();

            while (reader.Read())
                results.Add(Map(reader));

            CloseConnection();
            return results;
        }

        public List<Result> GetTopByScore(int limit = 10)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = $"""
                SELECT Id, StudentId, LessonId, StrokesPerMinute, WordsPerMinute,
                       TotalMistakes, Score, TimeRemaining, AccuracyPercent,
                       SentencesCompleted, TotalCharactersTyped, StartTime, EndTime, TimerExpired
                FROM Results
                ORDER BY Score DESC, WordsPerMinute DESC
                LIMIT {limit}
            """;

            var results = new List<Result>();
            using DbDataReader reader = command.ExecuteReader();

            while (reader.Read())
                results.Add(Map(reader));

            CloseConnection();
            return results;
        }

        public List<Result> GetAllAttemptsByStudentAndLesson(int studentId, int lessonId)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT Id, StudentId, LessonId, StrokesPerMinute, WordsPerMinute,
                       TotalMistakes, Score, TimeRemaining, AccuracyPercent,
                       SentencesCompleted, TotalCharactersTyped, StartTime, EndTime, TimerExpired
                FROM Results
                WHERE StudentId = @studentId AND LessonId = @lessonId
                ORDER BY EndTime DESC
            """;

            AddParam(command, "@studentId", studentId);
            AddParam(command, "@lessonId", lessonId);

            var results = new List<Result>();
            using DbDataReader reader = command.ExecuteReader();

            while (reader.Read())
                results.Add(Map(reader));

            Debug.WriteLine($"[ResultRepository] Found {results.Count} attempts for student {studentId}, lesson {lessonId}");

            CloseConnection();
            return results;
        }

        private static Result Map(DbDataReader reader)
        {
            return new Result
            {
                StudentId = reader.GetInt32(1),
                LessonId = reader.GetInt32(2),
                StrokesPerMinute = reader.GetInt32(3),
                WordsPerMinute = reader.GetInt32(4),
                TotalMistakes = reader.GetInt32(5),
                Score = reader.GetInt32(6),
                TimeRemaining = reader.GetDouble(7),
                AccuracyPercent = reader.GetDouble(8),
                SentencesCompleted = reader.GetInt32(9),
                TotalCharactersTyped = reader.GetInt32(10),
                StartTime = reader.GetDateTime(11),
                EndTime = reader.IsDBNull(12) ? null : reader.GetDateTime(12),
                TimerExpired = reader.GetInt32(13) == 1
            };
        }

        private static void AddParam(DbCommand command, string name, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            command.Parameters.Add(p);
        }
    }
}
