using System;
using System.Collections.Generic;
using System.Data.Common;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class LeaderboardRepository : DatabaseConnection, ILeaderboardRepository
    {
        public List<Student> GetTopStudentsBySpeed(int limit = 10)
        {
            var students = new List<Student>();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = $"""
                SELECT id, username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore
                FROM students
                ORDER BY avgSpeed DESC
                LIMIT {limit}
            """;

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                students.Add(new Student(
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
            return students;
        }

        public List<Student> GetTopStudentsByPrecision(int limit = 10)
        {
            var students = new List<Student>();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = $"""
                SELECT id, username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore
                FROM students
                ORDER BY avgPrecision DESC
                LIMIT {limit}
            """;

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                students.Add(new Student(
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
            return students;
        }

        public List<Student> GetTopStudentsByScore(int limit = 10)
        {
            var students = new List<Student>();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = $"""
                SELECT id, username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore
                FROM students
                ORDER BY totalScore DESC
                LIMIT {limit}
            """;

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                students.Add(new Student(
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
            return students;
        }

        public List<Student> GetTopStudentsByCompletedLessons(int limit = 10)
        {
            var students = new List<Student>();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = $"""
                SELECT id, username, name, password, level, avgSpeed, avgPrecision, completedLessons, totalScore
                FROM students
                ORDER BY completedLessons DESC
                LIMIT {limit}
            """;

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                students.Add(new Student(
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
            return students;
        }

        public int GetStudentRankBySpeed(int studentId)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT COUNT(*) + 1 AS `rank`
                FROM students s1
                WHERE s1.avgSpeed > (SELECT avgSpeed FROM students WHERE id = @studentId)
            """;

            var p = command.CreateParameter();
            p.ParameterName = "@studentId";
            p.Value = studentId;
            command.Parameters.Add(p);

            var result = command.ExecuteScalar();
            CloseConnection();

            return result != null ? Convert.ToInt32(result) : 0;
        }

        public int GetStudentRankByPrecision(int studentId)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT COUNT(*) + 1 AS `rank`
                FROM students s1
                WHERE s1.avgPrecision > (SELECT avgPrecision FROM students WHERE id = @studentId)
            """;

            var p = command.CreateParameter();
            p.ParameterName = "@studentId";
            p.Value = studentId;
            command.Parameters.Add(p);

            var result = command.ExecuteScalar();
            CloseConnection();

            return result != null ? Convert.ToInt32(result) : 0;
        }

        public int GetStudentRankByScore(int studentId)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT COUNT(*) + 1 AS `rank`
                FROM students s1
                WHERE s1.totalScore > (SELECT totalScore FROM students WHERE id = @studentId)
            """;

            var p = command.CreateParameter();
            p.ParameterName = "@studentId";
            p.Value = studentId;
            command.Parameters.Add(p);

            var result = command.ExecuteScalar();
            CloseConnection();

            return result != null ? Convert.ToInt32(result) : 0;
        }

        public int GetStudentRankByCompletedLessons(int studentId)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT COUNT(*) + 1 AS `rank`
                FROM students s1
                WHERE s1.completedLessons > (SELECT completedLessons FROM students WHERE id = @studentId)
            """;

            var p = command.CreateParameter();
            p.ParameterName = "@studentId";
            p.Value = studentId;
            command.Parameters.Add(p);

            var result = command.ExecuteScalar();
            CloseConnection();

            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}
