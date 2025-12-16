using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Text.Json;
using System.Data.Common;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class LessonRepository : DatabaseConnection, ILessonRepository
    {
        private readonly List<Lesson> lessons = [];

        public LessonRepository()
        {
            CreateTable("""
                CREATE TABLE IF NOT EXISTS Lessons (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(80) NOT NULL,
                    Description VARCHAR(250),
                    IsTest TINYINT(1),
                    IsDone TINYINT(1),
                    CourseId INT NOT NULL,
                    UNIQUE(Name, CourseId)
                );
            """);

            LoadLessonsFromJsonSync();
        }

        private void LoadLessonsFromJsonSync()
        {
            var statements = new List<string>();

            try
            {
                using var stream = FileSystem.OpenAppPackageFileAsync("Lessons.json")
                    .GetAwaiter().GetResult();

                using var reader = new StreamReader(stream);
                using var doc = JsonDocument.Parse(reader.ReadToEnd());

                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty("Lessons", out var l))
                {
                    root = l;
                }

                if (root.ValueKind != JsonValueKind.Array)
                    return;

                foreach (var item in root.EnumerateArray())
                {
                    string name = item.GetProperty("Name").GetString() ?? "";
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    string description = item.TryGetProperty("Description", out var d)
                        ? d.GetString() ?? ""
                        : "";

                    bool isTest = item.TryGetProperty("IsTest", out var it) && it.GetBoolean();
                    bool isDone = item.TryGetProperty("IsDone", out var id) && id.GetBoolean();
                    int courseId = item.GetProperty("CourseId").GetInt32();

                    name = name.Replace("'", "''");
                    description = description.Replace("'", "''");

                    statements.Add($"""
                        INSERT IGNORE INTO Lessons
                        (Name, Description, IsTest, IsDone, CourseId)
                        VALUES
                        ('{name}', '{description}', {(isTest ? 1 : 0)}, {(isDone ? 1 : 0)}, {courseId});
                    """);
                }

                if (statements.Count > 0)
                    InsertMultipleWithTransaction(statements);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lesson seed error: {ex.Message}");
            }
        }

        public List<Lesson> GetAll()
        {
            lessons.Clear();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Name, Description, IsTest, IsDone, CourseId FROM Lessons";

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                lessons.Add(new Lesson(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.GetInt32(3) == 1,
                    reader.GetInt32(4) == 1,
                    reader.GetInt32(5),
                    totalTime: 60
                ));
            }

            CloseConnection();
            return lessons;
        }

        public Lesson? Get(int id)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Name, Description, IsTest, IsDone, CourseId FROM Lessons WHERE Id = @id";

            var p = command.CreateParameter();
            p.ParameterName = "@id";
            p.Value = id;
            command.Parameters.Add(p);

            using DbDataReader reader = command.ExecuteReader();
            Lesson? result = null;

            if (reader.Read())
            {
                result = new Lesson(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.GetInt32(3) == 1,
                    reader.GetInt32(4) == 1,
                    reader.GetInt32(5),
                    totalTime: 60
                );
            }

            CloseConnection();
            return result;
        }
    }
}
