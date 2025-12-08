using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.IO;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class AssignmentRepository : DatabaseConnection, IAssignmentRepository
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

                // Insert 150 assignments (5 per lesson, 30 lessons)
                InsertDefaultAssignments();

                Debug.WriteLine("AssignmentRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AssignmentRepository initialization error: {ex.Message}");
                throw;
            }
        }

        private void InsertDefaultAssignments()
        {
            try
            {
                List<string> insertQueries = new();
                
                // Create 5 assignments per lesson (30 lessons x 5 = 150 assignments)
                for (int lessonId = 1; lessonId <= 30; lessonId++)
                {
                    for (int assignmentNum = 1; assignmentNum <= 5; assignmentNum++)
                    {
                        insertQueries.Add($@"INSERT OR IGNORE INTO Assignments(TimeLimit, LessonId) VALUES(60, {lessonId})");
                    }
                }

                InsertMultipleWithTransaction(insertQueries);
                Debug.WriteLine($"Inserted {insertQueries.Count} assignments");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inserting default assignments: {ex.Message}");
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
                        Debug.WriteLine($"Retrieved assignment: Id={id}, LessonId={lessonId}, TimeLimit={timeLimit}");
                    }
                }
                
                Debug.WriteLine($"Total assignments retrieved: {assignments.Count}");
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