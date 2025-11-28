using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

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
                        TimeLimit DOUBLE NOT NULL,
                        LessonId INTEGER NOT NULL
                    );
                ");
                LoadAssignmentsFromJsonAsync().Wait();
                Debug.WriteLine("AssignmentRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AssignmentRepository initialization error: {ex.Message}");
                throw;
            }
        }
        private async Task LoadAssignmentsFromJsonAsync()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("Assignments.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;

                var items = root.GetProperty("Items");
                List<string> insertQueries = new();

                foreach (var item in items.EnumerateArray())
                {
                    int id = item.GetProperty("Id").GetInt32();
                    double timeLimit = item.GetProperty("TimeLimit").GetDouble();
                    int lessonId = item.GetProperty("LessonId").GetInt32();

                    insertQueries.Add($@"INSERT OR IGNORE INTO Assignments(Id, TimeLimit, LessonId) 
                                VALUES({id}, {timeLimit}, {lessonId})");
                }

                InsertMultipleWithTransaction(insertQueries);
                Debug.WriteLine($"Loaded {insertQueries.Count} assignments from JSON");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading assignments from JSON: {ex.Message}");
                throw;
            }
        }


        public List<Assignment> GetAll()
        {
            assignments.Clear();
            try
            {
                string query = "SELECT Id, TimeLimit, LessonId FROM Assignments";

                OpenConnection();
                using (SqliteCommand command = new(query, Connection))
                {
                    SqliteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        double timeLimit = reader.GetDouble(1);
                        int lessonId = reader.GetInt32(2);

                        assignments.Add(new Assignment(id, timeLimit, lessonId));
                    }
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

        public Assignment? Get(int id)
        {
            Assignment? assignment = null;

            try
            {
                string query = "SELECT Id, TimeLimit, LessonId FROM Assignments WHERE Id = @Id";

                OpenConnection();
                using (SqliteCommand command = new(query, Connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    SqliteDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        int assignmentId = reader.GetInt32(0);
                        double timeLimit = reader.GetDouble(1);
                        int lessonId = reader.GetInt32(2);

                        assignment = new Assignment(assignmentId, timeLimit, lessonId);
                    }
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