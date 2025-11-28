using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Data.Repositories
{
    internal class AssignmentRepository : DatabaseConnection
    {
        private readonly List<Assignment> assignments = [];

        public AssignmentRepository()
        {
            try
            {
                CreateTable(@"
                    CREATE TABLE IF NOT EXISTS Assignments (
                        Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        Text NVARCHAR(300) NOT NULL,
                        LessonId INTEGER NOT NULL,
                        TimeLimit DOUBLE NOT NULL
                    );
                ");

                Debug.WriteLine("Assignments table ready.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AssignmentRepository init error: {ex.Message}");
                throw;
            }
        }

        // --------------------------------------------
        // IMPORT JSON
        // --------------------------------------------
        public async Task ImportFromJsonAsync(string path)
        {
            var json = await File.ReadAllTextAsync(path);

            var dtos = JsonSerializer.Deserialize<List<AssignmentDto>>(json)
                       ?? new List<AssignmentDto>();

            List<string> inserts = new();

            foreach (var dto in dtos)
            {
                string sql = $@"
                    INSERT INTO Assignments (Text, LessonId, TimeLimit)
                    VALUES ('{Escape(dto.Text)}', {dto.LessonId}, {dto.TimeLimit});
                ";

                inserts.Add(sql);
            }

            InsertMultipleWithTransaction(inserts);
        }

        private string Escape(string value) =>
            value.Replace("'", "''");

        private class AssignmentDto
        {
            public string Text { get; set; }
            public int LessonId { get; set; }
            public double TimeLimit { get; set; }
        }

        // --------------------------------------------
        // GET ALL
        // --------------------------------------------
        public List<Assignment> GetAll()
        {
            assignments.Clear();

            try
            {
                OpenConnection();

                using var cmd = new SqliteCommand("SELECT Id, Text, LessonId, TimeLimit FROM Assignments", Connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    assignments.Add(new Assignment(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetInt32(2),
                        reader.GetDouble(3)
                    ));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving assignments: {ex.Message}");
                throw;
            }
            finally
            {
                CloseConnection();
            }

            return assignments;
        }

        // --------------------------------------------
        // GET BY ID
        // --------------------------------------------
        public Assignment? Get(int id)
        {
            Assignment? assignment = null;

            try
            {
                OpenConnection();

                using var cmd = new SqliteCommand(
                    "SELECT Id, Text, LessonId, TimeLimit FROM Assignments WHERE Id = @Id", Connection);

                cmd.Parameters.AddWithValue("@Id", id);

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    assignment = new Assignment(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetInt32(2),
                        reader.GetDouble(3)
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving assignment with Id {id}: {ex.Message}");
                throw;
            }
            finally
            {
                CloseConnection();
            }

            return assignment;
        }
    }
}
