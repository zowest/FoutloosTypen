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
                CREATE TABLE IF NOT EXISTS lessonresults (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    StudentId INT NOT NULL,
                    LessonId INT NOT NULL,
                    Score INT NOT NULL,
                    StrokesPerMinute INT NOT NULL,
                    WordsPerMinute INT NOT NULL,
                    AccuracyPercent DOUBLE NOT NULL,
                    TotalMistakes INT NOT NULL,
                    SentencesCompleted INT NOT NULL DEFAULT 0,
                    TimeSpent DOUBLE NOT NULL DEFAULT 0,
                    CompletedAt DATETIME NOT NULL
                );
            """);

            try
            {
                OpenConnection();
                using var command = Connection.CreateCommand();
                command.CommandText = "ALTER TABLE lessonresults DROP INDEX unique_student_lesson";
                command.ExecuteNonQuery();
                CloseConnection();
                Debug.WriteLine("[ResultRepository] Removed old unique_student_lesson constraint");
            }
            catch (MySqlConnector.MySqlException)
            {
                CloseConnection();
            }
        }

        public void Save(Result result)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                INSERT INTO lessonresults
                (StudentId, LessonId, Score, StrokesPerMinute, WordsPerMinute,
                 AccuracyPercent, TotalMistakes, SentencesCompleted, TimeSpent, CompletedAt)
                VALUES
                (@studentId, @lessonId, @score, @spm, @wpm,
                 @accuracy, @mistakes, @sentences, @timeSpent, @completedAt)
            """;

            AddParam(command, "@studentId", result.StudentId);
            AddParam(command, "@lessonId", result.LessonId);
            AddParam(command, "@score", result.Score);
            AddParam(command, "@spm", result.StrokesPerMinute);
            AddParam(command, "@wpm", result.WordsPerMinute);
            AddParam(command, "@accuracy", result.AccuracyPercent);
            AddParam(command, "@mistakes", result.TotalMistakes);
            AddParam(command, "@sentences", result.SentencesCompleted);
            AddParam(command, "@timeSpent", result.TimeSpent);
            AddParam(command, "@completedAt", result.EndTime ?? DateTime.Now);

            command.ExecuteNonQuery();
            
            Debug.WriteLine($"[ResultRepository] Saved new result for student {result.StudentId}, lesson {result.LessonId}, score {result.Score}");
            
            CloseConnection();
        }

        public void SaveEndlessModeResult(int studentId, int score)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                INSERT INTO EndlessModeResults
                (StudentId, Score, CompletedAt)
                VALUES
                (@studentId, @score, @completedAt)
            """;

            AddParam(command, "@studentId", studentId);
            AddParam(command, "@score", score);
            AddParam(command, "@completedAt", DateTime.Now);

            command.ExecuteNonQuery();
            CloseConnection();
        }

        public Result? GetByStudentAndLesson(int studentId, int lessonId)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT Id, StudentId, LessonId, Score, StrokesPerMinute, WordsPerMinute,
                       AccuracyPercent, TotalMistakes, SentencesCompleted, TimeSpent, CompletedAt
                FROM lessonresults
                WHERE StudentId = @studentId AND LessonId = @lessonId
                ORDER BY Score DESC, CompletedAt DESC
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
                SELECT Id, StudentId, LessonId, Score, StrokesPerMinute, WordsPerMinute,
                       AccuracyPercent, TotalMistakes, SentencesCompleted, TimeSpent, CompletedAt
                FROM lessonresults
                WHERE StudentId = @studentId
                ORDER BY CompletedAt DESC
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
                SELECT Id, StudentId, LessonId, Score, StrokesPerMinute, WordsPerMinute,
                       AccuracyPercent, TotalMistakes, SentencesCompleted, TimeSpent, CompletedAt
                FROM lessonresults
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
            command.CommandText = $"""
                SELECT r.Id, r.StudentId, r.LessonId, r.Score, r.StrokesPerMinute, r.WordsPerMinute,
                       r.AccuracyPercent, r.TotalMistakes, r.SentencesCompleted, r.TimeSpent, r.CompletedAt
                FROM lessonresults r
                INNER JOIN (
                    SELECT StudentId, MAX(Score) as BestScore
                    FROM lessonresults
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
                SELECT Id, StudentId, LessonId, Score, StrokesPerMinute, WordsPerMinute,
                       AccuracyPercent, TotalMistakes, SentencesCompleted, TimeSpent, CompletedAt
                FROM lessonresults
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
                SELECT Id, StudentId, LessonId, Score, StrokesPerMinute, WordsPerMinute,
                       AccuracyPercent, TotalMistakes, SentencesCompleted, TimeSpent, CompletedAt
                FROM lessonresults
                WHERE StudentId = @studentId AND LessonId = @lessonId
                ORDER BY CompletedAt DESC
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
            var completedAt = reader.GetDateTime(10);
            var timeSpent = reader.GetDouble(9);

            return new Result
            {
                StudentId = reader.GetInt32(1),
                LessonId = reader.GetInt32(2),
                Score = reader.GetInt32(3),
                StrokesPerMinute = reader.GetInt32(4),
                WordsPerMinute = reader.GetInt32(5),
                AccuracyPercent = reader.GetDouble(6),
                TotalMistakes = reader.GetInt32(7),
                SentencesCompleted = reader.GetInt32(8),
                StartTime = completedAt.AddSeconds(-timeSpent),
                EndTime = completedAt,
                CompletedSentences = new List<string>(),
                CurrentIncompleteText = string.Empty,
                ExpectedTime = 300, // Default
                TimerExpired = false,
                TimeRemaining = 0
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
