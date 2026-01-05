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
                // First, create the main students table if it doesn't exist
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
            try
            {
                OpenConnection();

                using var command = Connection.CreateCommand();
                command.CommandText = """
                    SELECT id, username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore, UseTtsMode
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
                        reader.GetInt32("id"),
                        reader.GetString("username"),
                        reader.GetString("name"),
                        reader.GetString("password"),
                        reader.GetInt32("level"),
                        reader.GetDouble("avgSpeed"),
                        reader.GetDouble("avgPrecision"),
                        reader.GetInt32("completedLessons"),
                        reader.GetInt32("totalScore"),
                        reader.GetBoolean("UseTtsMode")
                    );
                }

                CloseConnection();
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StudentRepository] Error getting student by username: {ex.Message}");
                CloseConnection();
                return null;
            }
        }

        public Student? Get(int id)
        {
            try
            {
                OpenConnection();

                using var command = Connection.CreateCommand();
                command.CommandText = """
                    SELECT id, username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore, UseTtsMode
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
                        reader.GetInt32("id"),
                        reader.GetString("username"),
                        reader.GetString("name"),
                        reader.GetString("password"),
                        reader.GetInt32("level"),
                        reader.GetDouble("avgSpeed"),
                        reader.GetDouble("avgPrecision"),
                        reader.GetInt32("completedLessons"),
                        reader.GetInt32("totalScore"),
                        reader.GetBoolean("UseTtsMode")
                    );
                }

                CloseConnection();
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StudentRepository] Error getting student by id: {ex.Message}");
                CloseConnection();
                return null;
            }
        }

        public List<Student> GetAll()
        {
            var result = new List<Student>();
            
            try
            {
                OpenConnection();

                using var command = Connection.CreateCommand();
                command.CommandText = """
                    SELECT id, username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore, UseTtsMode
                    FROM students
                    ORDER BY username
                """;

                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new Student(
                        reader.GetInt32("id"),
                        reader.GetString("username"),
                        reader.GetString("name"),
                        reader.GetString("password"),
                        reader.GetInt32("level"),
                        reader.GetDouble("avgSpeed"),
                        reader.GetDouble("avgPrecision"),
                        reader.GetInt32("completedLessons"),
                        reader.GetInt32("totalScore"),
                        reader.GetBoolean("UseTtsMode")
                    ));
                }

                CloseConnection();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StudentRepository] Error getting all students: {ex.Message}");
                CloseConnection();
            }

            return result;
        }

        public void UpdateStatistics(int studentId, double avgSpeed, double avgPrecision)
        {
            try
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

                var rowsAffected = command.ExecuteNonQuery();
                Debug.WriteLine($"[StudentRepository] Updated statistics for student {studentId}, rows affected: {rowsAffected}");
                
                CloseConnection();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StudentRepository] Error updating statistics: {ex.Message}");
                CloseConnection();
            }
        }

        public void UpdateProgress(int studentId, int completedLessons, int totalScore)
        {
            try
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

                var rowsAffected = command.ExecuteNonQuery();
                Debug.WriteLine($"[StudentRepository] Updated progress for student {studentId}, rows affected: {rowsAffected}");
                
                CloseConnection();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StudentRepository] Error updating progress: {ex.Message}");
                CloseConnection();
            }
        }

        public void UpdateTtsMode(int studentId, bool useTtsMode)
        {
            try
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

                var rowsAffected = command.ExecuteNonQuery();
                Debug.WriteLine($"[StudentRepository] Updated TTS mode for student {studentId} to {useTtsMode}, rows affected: {rowsAffected}");
                
                CloseConnection();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StudentRepository] Error updating TTS mode: {ex.Message}");
                CloseConnection();
            }
        }

        public void Add(Student student)
        {
            try
            {
                OpenConnection();

                using var command = Connection.CreateCommand();
                command.CommandText = """
                    INSERT INTO students (username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore, UseTtsMode)
                    VALUES (@username, @name, @password, @level, @avgSpeed, @avgPrecision, @completedLessons, @totalScore, @useTtsMode)
                """;

                var pUsername = command.CreateParameter();
                pUsername.ParameterName = "@username";
                pUsername.Value = student.Username;
                command.Parameters.Add(pUsername);

                var pName = command.CreateParameter();
                pName.ParameterName = "@name";
                pName.Value = student.Name;
                command.Parameters.Add(pName);

                var pPassword = command.CreateParameter();
                pPassword.ParameterName = "@password";
                pPassword.Value = student.Password;
                command.Parameters.Add(pPassword);

                var pLevel = command.CreateParameter();
                pLevel.ParameterName = "@level";
                pLevel.Value = student.Level;
                command.Parameters.Add(pLevel);

                var pAvgSpeed = command.CreateParameter();
                pAvgSpeed.ParameterName = "@avgSpeed";
                pAvgSpeed.Value = student.AvgSpeed;
                command.Parameters.Add(pAvgSpeed);

                var pAvgPrecision = command.CreateParameter();
                pAvgPrecision.ParameterName = "@avgPrecision";
                pAvgPrecision.Value = student.AvgPrecision;
                command.Parameters.Add(pAvgPrecision);

                var pCompletedLessons = command.CreateParameter();
                pCompletedLessons.ParameterName = "@completedLessons";
                pCompletedLessons.Value = student.CompletedLessons;
                command.Parameters.Add(pCompletedLessons);

                var pTotalScore = command.CreateParameter();
                pTotalScore.ParameterName = "@totalScore";
                pTotalScore.Value = student.TotalScore;
                command.Parameters.Add(pTotalScore);

                var pUseTtsMode = command.CreateParameter();
                pUseTtsMode.ParameterName = "@useTtsMode";
                pUseTtsMode.Value = student.UseTtsMode ? 1 : 0;
                command.Parameters.Add(pUseTtsMode);

                command.ExecuteNonQuery();
                Debug.WriteLine($"[StudentRepository] Added new student: {student.Username}");
                
                CloseConnection();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StudentRepository] Error adding student: {ex.Message}");
                CloseConnection();
                throw;
            }
        }
    }
}
