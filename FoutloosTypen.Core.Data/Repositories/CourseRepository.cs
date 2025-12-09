using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;


namespace FoutloosTypen.Core.Data.Repositories
{
    public class CourseRepository : DatabaseConnection, ICourseRepository
    {
        private readonly List<Course> courses = [];

        public CourseRepository()
        {
            try
            {
                CreateTable(@"
                CREATE TABLE IF NOT EXISTS Courses (
                    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    [Name] NVARCHAR(80) UNIQUE NOT NULL,
                    [Description] NVARCHAR(250),
                    [Difficulty] INTEGER
                )");

                // Seed from JSON if available
                LoadCoursesFromJsonSync();

                Debug.WriteLine("CourseRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CourseRepository initialization error: {ex.Message}");
                throw;
            }
        }

        private void LoadCoursesFromJsonSync()
        {
            try
            {
                using var stream = FileSystem.OpenAppPackageFileAsync("Courses.json").GetAwaiter().GetResult();
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Accept array root or object with common property names
                JsonElement items = root;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("Courses", out var plural)) items = plural;
                    else if (root.TryGetProperty("Course", out var singular)) items = singular;
                    else if (root.TryGetProperty("Items", out var itemsProp)) items = itemsProp;
                    else if (root.TryGetProperty("Data", out var dataProp)) items = dataProp;
                }

                if (items.ValueKind != JsonValueKind.Array)
                {
                    Debug.WriteLine("Courses JSON has unexpected shape; expected array or object with 'Courses'/'Course'.");
                    return;
                }

                var insertQueries = new List<string>();
                foreach (var item in items.EnumerateArray())
                {
                    int id = item.TryGetProperty("Id", out var idProp) ? idProp.GetInt32() : 0;
                    string name = item.TryGetProperty("Name", out var nameProp) ? (nameProp.GetString() ?? string.Empty) :
                                  item.TryGetProperty("name", out var nameProp2) ? (nameProp2.GetString() ?? string.Empty) : string.Empty;
                    string description = item.TryGetProperty("Description", out var descProp) ? (descProp.GetString() ?? string.Empty) :
                                        item.TryGetProperty("description", out var descProp2) ? (descProp2.GetString() ?? string.Empty) : string.Empty;
                    int difficulty = item.TryGetProperty("Difficulty", out var diffProp) ? diffProp.GetInt32() :
                                     item.TryGetProperty("difficulty", out var diffProp2) ? diffProp2.GetInt32() : 0;

                    if (string.IsNullOrWhiteSpace(name)) continue;

                    // escape single quotes for raw SQL
                    name = name.Replace("'", "''");
                    description = description.Replace("'", "''");

                    if (id > 0)
                    {
                        insertQueries.Add($@"INSERT OR IGNORE INTO Courses(Id, Name, Description, Difficulty) VALUES({id}, '{name}', '{description}', {difficulty})");
                    }
                    else
                    {
                        insertQueries.Add($@"INSERT OR IGNORE INTO Courses(Name, Description, Difficulty) VALUES('{name}', '{description}', {difficulty})");
                    }
                }

                if (insertQueries.Count > 0)
                {
                    InsertMultipleWithTransaction(insertQueries);
                    Debug.WriteLine($"Seeded {insertQueries.Count} courses from JSON");
                }
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("Courses.json not found; skipping JSON seed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading courses from JSON: {ex.Message}");
            }
        }

        public List<Course> GetAll()
        {
            courses.Clear();

            try
            {
                string selectQuery = "SELECT Id, Name, Description, Difficulty FROM Courses";
                OpenConnection();

                using (SqliteCommand command = new(selectQuery, Connection))
                {
                    SqliteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        string description = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        int difficulty = reader.GetInt32(3);

                        courses.Add(new Course(id, name, description, difficulty));
                        Debug.WriteLine($"Loaded course: {id} - {name}");
                    }
                }

                CloseConnection();
                Debug.WriteLine($"Total courses loaded: {courses.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading courses: {ex.Message}");
                CloseConnection();
            }

            return courses;
        }

        public Course Get(int id)
        {
            string selectQuery = $"SELECT Id, Name, Description, Difficulty FROM Courses WHERE Id = {id}";
            Course tmpCourse = null;

            try
            {
                OpenConnection();

                using (SqliteCommand command = new(selectQuery, Connection))
                {
                    SqliteDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        int Id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        string description = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        int difficulty = reader.GetInt32(3);

                        tmpCourse = new Course(Id, name, description, difficulty);
                    }
                }

                CloseConnection();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading course {id}: {ex.Message}");
                CloseConnection();
            }

            return tmpCourse;
        }
    }
}