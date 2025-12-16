using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Text.Json;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class LessonRepository : DatabaseConnection, ILessonRepository
    {
        private readonly List<Lesson> lessons = [];

        public LessonRepository()
        {
            Debug.WriteLine("LessonRepository: Starting initialization...");
            
            CreateTable(@"CREATE TABLE IF NOT EXISTS Lessons (
                        [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [Name] NVARCHAR(80) NOT NULL,
                        [Description] NVARCHAR(250),
                        [IsTest] BOOL,
                        [IsDone] BOOL,
                        [CourseId] INTEGER NOT NULL,
                        UNIQUE(Name, CourseId)
                )");

            Debug.WriteLine("LessonRepository: Table created");

            LoadLessonsFromJsonSync();

            GetAll();
            
            Debug.WriteLine("LessonRepository: Initialization complete");
        }

        private void LoadLessonsFromJsonSync()
        {
            var stopwatch = Stopwatch.StartNew();
            List<string> insertQueries = new();

            try
            {
                Debug.WriteLine("Loading Lessons.json...");

                Task.Run(async () =>
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync("Lessons.json");
                    using var reader = new StreamReader(stream);
                    var json = await reader.ReadToEndAsync();

                    using var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;

                    JsonElement lessonsElement = root;
                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("Lessons", out var le))
                        lessonsElement = le;

                    if (lessonsElement.ValueKind != JsonValueKind.Array)
                        throw new Exception("Invalid JSON structure for lessons");

                    foreach (var item in lessonsElement.EnumerateArray())
                    {
                        int id = item.TryGetProperty("Id", out var idProp) ? idProp.GetInt32() : 0;
                        string name = item.GetProperty("Name").GetString() ?? string.Empty;
                        string description = item.TryGetProperty("Description", out var descProp) ? (descProp.GetString() ?? string.Empty) : string.Empty;
                        bool isTest = item.TryGetProperty("IsTest", out var isTestProp) && isTestProp.GetBoolean();
                        bool isDone = item.TryGetProperty("IsDone", out var isDoneProp) && isDoneProp.GetBoolean();
                        int courseId = item.GetProperty("CourseId").GetInt32();

                        name = name.Replace("'", "''");
                        description = description.Replace("'", "''");

                        if (id > 0)
                        {
                            insertQueries.Add($@"INSERT OR IGNORE INTO Lessons(Id, Name, Description, IsTest, IsDone, CourseId)
                            VALUES({id}, '{name}', '{description}', {(isTest ? 1 : 0)}, {(isDone ? 1 : 0)}, {courseId})");
                        }
                        else
                        {
                            insertQueries.Add($@"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('{name}', '{description}', {(isTest ? 1 : 0)}, {(isDone ? 1 : 0)}, {courseId})");
                        }
                    }
                }).GetAwaiter().GetResult();

                if (insertQueries.Any())
                {
                    InsertMultipleWithTransaction(insertQueries);
                    stopwatch.Stop();
                    Debug.WriteLine($"SUCCESS: Loaded {insertQueries.Count} lessons from JSON in {stopwatch.ElapsedMilliseconds}ms");
                }
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("Lessons.json not found; skipping JSON seed for lessons");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR loading Lessons.json: {ex.Message}");
            }
        }

        public List<Lesson> GetAll()
        {
            lessons.Clear();

            string selectQuery = "SELECT Id, Name, Description, IsTest, IsDone, CourseId FROM Lessons";
            OpenConnection();

            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    string description = reader.GetString(2);
                    bool isTest = reader.GetBoolean(3);
                    bool isDone = reader.GetBoolean(4);
                    int courseId = reader.GetInt32(5);
                    double totalTime = 60;

                    lessons.Add(new Lesson(id, name, description, isTest, isDone, courseId, totalTime));
                }
            }

            CloseConnection();
            Debug.WriteLine($"LessonRepository: Retrieved {lessons.Count} lessons");
            return lessons;
        }

        public Lesson Get(int id)
        {
            string selectQuery = $"SELECT Id, Name, Description, IsTest, IsDone, CourseId FROM Lessons WHERE Id = {id}";
            Lesson tmpLesson = null;

            OpenConnection();

            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    int Id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    string description = reader.GetString(2);
                    bool isTest = reader.GetBoolean(3);
                    bool isDone = reader.GetBoolean(4);
                    int courseId = reader.GetInt32(5);
                    double totalTime = 60;

                    tmpLesson = new Lesson(Id, name, description, isTest, isDone, courseId, totalTime);
                }
            }

            CloseConnection();
            return tmpLesson;
        }
    }
}