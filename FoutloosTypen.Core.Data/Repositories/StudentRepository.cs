using System.Data.Common;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using System.Diagnostics;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class StudentRepository : DatabaseConnection, IStudentRepository
    {
        public StudentRepository()
        {
            try
            {
                // Create the main students table if it doesn't exist
                CreateTable("""
                    CREATE TABLE IF NOT EXISTS students (
                        id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                        username VARCHAR(50) NOT NULL UNIQUE,
                        name VARCHAR(100) NOT NULL,
                        password VARCHAR(255) NOT NULL,
                        level INT DEFAULT 1,
                        avgSpeed DOUBLE DEFAULT 0.0,
                        avgPrecision DOUBLE DEFAULT 0.0,
                        completedLessons INT DEFAULT 0,
                        totalScore INT DEFAULT 0,
                        UseTtsMode TINYINT(1) DEFAULT 0
                    );
                """);

                // Check if UseTtsMode column exists and add it if it doesn't (for backward compatibility)
                if (!ColumnExists("students", "UseTtsMode"))
                {
                    Debug.WriteLine("[StudentRepository] Adding UseTtsMode column for backward compatibility");
                    CreateTable("""
                        ALTER TABLE students 
                        ADD COLUMN UseTtsMode TINYINT(1) DEFAULT 0
                    """);
                }

                Debug.WriteLine("[StudentRepository] Students table initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StudentRepository] Error initializing table: {ex.Message}");
            }
        }

        private bool ColumnExists(string tableName, string columnName)
        {
            try
            {
                OpenConnection();
                using var cmd = Connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @table AND COLUMN_NAME = @column";

                var tableParam = cmd.CreateParameter();
                tableParam.ParameterName = "@table";
                tableParam.Value = tableName;
                cmd.Parameters.Add(tableParam);

                var columnParam = cmd.CreateParameter();
                columnParam.ParameterName = "@column";
                columnParam.Value = columnName;
                cmd.Parameters.Add(columnParam);

                var result = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                CloseConnection();
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StudentRepository] Error checking column existence: {ex.Message}");
                CloseConnection();
                return false;
            }
        }

        public Student? Get(string username)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT id, username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore, COALESCE(UseTtsMode, 0) as UseTtsMode
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
                    reader.GetInt32(8),
                    Convert.ToBoolean(reader.GetInt32(9))  // UseTtsMode
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
                SELECT id, username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore, COALESCE(UseTtsMode, 0) as UseTtsMode
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
                    reader.GetInt32(8),
                    Convert.ToBoolean(reader.GetInt32(9))  // UseTtsMode
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
                SELECT id, username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore, COALESCE(UseTtsMode, 0) as UseTtsMode
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
                    reader.GetInt32(8),
                    Convert.ToBoolean(reader.GetInt32(9))  // UseTtsMode
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

        public void UpdateTtsMode(int studentId, bool useTtsMode)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                UPDATE students
                SET UseTtsMode = @useTtsMode
                WHERE id = @id
            """;

            var pId = command.CreateParameter();
            pId.ParameterName = "@id";
            pId.Value = studentId;
            command.Parameters.Add(pId);

            var pTtsMode = command.CreateParameter();
            pTtsMode.ParameterName = "@useTtsMode";
            pTtsMode.Value = useTtsMode ? 1 : 0;
            command.Parameters.Add(pTtsMode);

            command.ExecuteNonQuery();
            CloseConnection();
        }
    }
}