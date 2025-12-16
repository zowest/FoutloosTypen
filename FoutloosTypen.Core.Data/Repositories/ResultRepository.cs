using System.Data.Common;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Repositories;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class ResultRepository : DatabaseConnection, IResultRepository
    {
        public void Save(Result result)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Results
                (StudentId, LessonId, StrokesPerMinute, WordsPerMinute,
                 TotalMistakes, Score, TimeRemaining)
                VALUES
                (@studentId, @lessonId, @spm, @wpm,
                 @mistakes, @score, @timeRemaining)
                ON DUPLICATE KEY UPDATE
                    StrokesPerMinute = VALUES(StrokesPerMinute),
                    WordsPerMinute   = VALUES(WordsPerMinute),
                    TotalMistakes    = VALUES(TotalMistakes),
                    Score            = VALUES(Score),
                    TimeRemaining    = VALUES(TimeRemaining),
            """;

            AddParam(command, "@studentId", result.StudentId);
            AddParam(command, "@lessonId", result.LessonId);
            AddParam(command, "@spm", result.StrokesPerMinute);
            AddParam(command, "@wpm", result.WordsPerMinute);
            AddParam(command, "@mistakes", result.TotalMistakes);
            AddParam(command, "@score", result.Score);
            AddParam(command, "@timeRemaining", result.TimeRemaining);

            command.ExecuteNonQuery();
            CloseConnection();
        }

        public Result? GetByStudentAndLesson(int studentId, int lessonId)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT Id, StudentId, LessonId, StrokesPerMinute, WordsPerMinute,
                       TotalMistakes, Score, TimeRemaining, MistakeType
                FROM Results
                WHERE StudentId = @studentId AND LessonId = @lessonId
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
                       TotalMistakes, Score, TimeRemaining
                FROM Results
                WHERE StudentId = @studentId
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
                       TotalMistakes, Score, TimeRemaining
                FROM Results
                WHERE LessonId = @lessonId
                ORDER BY Score DESC
            """;

            AddParam(command, "@lessonId", lessonId);

            var results = new List<Result>();
            using DbDataReader reader = command.ExecuteReader();

            while (reader.Read())
                results.Add(Map(reader));

            CloseConnection();
            return results;
        }

        private static Result Map(DbDataReader reader)
        {
            return new Result
            {
                // Assuming the order of columns in SELECT matches the properties
                // and that all properties are settable
                // Id is not present in Result, so skip index 0
                StudentId = reader.GetInt32(1),
                LessonId = reader.GetInt32(2),
                StrokesPerMinute = reader.GetInt32(3),
                WordsPerMinute = reader.GetInt32(4),
                TotalMistakes = reader.GetInt32(5),
                Score = reader.GetInt32(6),
                // TimeRemaining is a double in the model, but int in the DB, so cast
                TimeRemaining = Convert.ToDouble(reader.GetInt32(7))
                // Add more property assignments here if needed and available in the Result class
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
