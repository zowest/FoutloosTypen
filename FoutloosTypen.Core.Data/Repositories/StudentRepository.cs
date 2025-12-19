using System.Data.Common;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class StudentRepository : DatabaseConnection, IStudentRepository
    {
        public Student? Get(string username)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT id, username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore
                FROM students
                WHERE username = @username
            """;

            var p = command.CreateParameter();
            p.ParameterName = "@username";
            p.Value = username;
            command.Parameters.Add(p);

            using DbDataReader reader = command.ExecuteReader();
            Student? result = null;

            if (reader.Read())
            {
                result = new Student(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetDouble(5),
                    reader.GetDouble(6),
                    reader.GetInt32(7),
                    reader.GetInt32(8)
                );
            }

            CloseConnection();
            return result;
        }

        public Student? Get(int id)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT id, username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore
                FROM students
                WHERE id = @id
            """;

            var p = command.CreateParameter();
            p.ParameterName = "@id";
            p.Value = id;
            command.Parameters.Add(p);

            using DbDataReader reader = command.ExecuteReader();
            Student? result = null;

            if (reader.Read())
            {
                result = new Student(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetDouble(5),
                    reader.GetDouble(6),
                    reader.GetInt32(7),
                    reader.GetInt32(8)
                );
            }

            CloseConnection();
            return result;
        }

        public List<Student> GetAll()
        {
            var result = new List<Student>();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT id, username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore
                FROM students
            """;

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Student(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetDouble(5),
                    reader.GetDouble(6),
                    reader.GetInt32(7),
                    reader.GetInt32(8)
                ));
            }

            CloseConnection();
            return result;
        }

        public void UpdateStatistics(int studentId, double avgSpeed, double avgPrecision)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                UPDATE students
                SET avgSpeed = @avgSpeed, avgPrecision = @avgPrecision
                WHERE id = @id
            """;

            var pId = command.CreateParameter();
            pId.ParameterName = "@id";
            pId.Value = studentId;
            command.Parameters.Add(pId);

            var pSpeed = command.CreateParameter();
            pSpeed.ParameterName = "@avgSpeed";
            pSpeed.Value = avgSpeed;
            command.Parameters.Add(pSpeed);

            var pPrecision = command.CreateParameter();
            pPrecision.ParameterName = "@avgPrecision";
            pPrecision.Value = avgPrecision;
            command.Parameters.Add(pPrecision);

            command.ExecuteNonQuery();
            CloseConnection();
        }

        public void UpdateProgress(int studentId, int completedLessons, int totalScore)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                UPDATE students
                SET completedLessons = @completedLessons, totalScore = @totalScore
                WHERE id = @id
            """;

            var pId = command.CreateParameter();
            pId.ParameterName = "@id";
            pId.Value = studentId;
            command.Parameters.Add(pId);

            var pLessons = command.CreateParameter();
            pLessons.ParameterName = "@completedLessons";
            pLessons.Value = completedLessons;
            command.Parameters.Add(pLessons);

            var pScore = command.CreateParameter();
            pScore.ParameterName = "@totalScore";
            pScore.Value = totalScore;
            command.Parameters.Add(pScore);

            command.ExecuteNonQuery();
            CloseConnection();
        }
    }
}
