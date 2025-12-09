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

                // Seed from packaged JSON if available
                LoadAssignmentsFromJsonSync();

                Debug.WriteLine("AssignmentRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AssignmentRepository initialization error: {ex.Message}");
                throw;
            }
        }

        private void LoadAssignmentsFromJsonSync()
        {
            try
            {
                using var stream = FileSystem.OpenAppPackageFileAsync("Assignment.json").GetAwaiter().GetResult();
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Accept either array root or object with common property names
                JsonElement items = root;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("Assignments", out var aPlural)) items = aPlural;
                    else if (root.TryGetProperty("Assignment", out var aSingular)) items = aSingular;
                    else if (root.TryGetProperty("Items", out var itemsProp)) items = itemsProp;
                    else if (root.TryGetProperty("Data", out var dataProp)) items = dataProp;
                }

                if (items.ValueKind != JsonValueKind.Array)
                {
                    Debug.WriteLine("Assignment JSON has unexpected shape; expected array or object with 'Assignments'/'Assignment'.");
                    return;
                }

                var insertQueries = new List<string>();
                foreach (var item in items.EnumerateArray())
                {
                    int id = item.TryGetProperty("Id", out var idProp) ? idProp.GetInt32() : 0;
                    double timeLimit = item.TryGetProperty("TimeLimit", out var tlProp) ? tlProp.GetDouble() :
                                       item.TryGetProperty("timeLimit", out var tlProp2) ? tlProp2.GetDouble() : 60;
                    int lessonId = item.TryGetProperty("LessonId", out var lidProp) ? lidProp.GetInt32() :
                                   item.TryGetProperty("lessonId", out var lidProp2) ? lidProp2.GetInt32() : 0;

                    if (lessonId == 0) continue;

                    if (id > 0)
                    {
                        insertQueries.Add($@"INSERT OR IGNORE INTO Assignments(Id, TimeLimit, LessonId) VALUES({id}, {timeLimit}, {lessonId})");
                    }
                    else
                    {
                        insertQueries.Add($@"INSERT OR IGNORE INTO Assignments(TimeLimit, LessonId) VALUES({timeLimit}, {lessonId})");
                    }
                }

                if (insertQueries.Count > 0)
                {
                    InsertMultipleWithTransaction(insertQueries);
                    Debug.WriteLine($"Seeded {insertQueries.Count} assignments from JSON");
                }
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("Assignment.json not found; skipping JSON seed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading assignments from JSON: {ex.Message}");
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