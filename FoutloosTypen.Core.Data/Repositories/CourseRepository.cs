using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Maui.Storage;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Data.Common;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class CourseRepository : DatabaseConnection, ICourseRepository
    {
        private readonly List<Course> courses = [];

        public CourseRepository()
        {
            CreateTable("""
                CREATE TABLE IF NOT EXISTS Courses (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(80) UNIQUE NOT NULL,
                    Description VARCHAR(250),
                    Difficulty INT
                );
            """);

            //LoadCoursesFromJsonSync();
        }

        //private void LoadCoursesFromJsonSync()
        //{
        //    try
        //    {
        //        using var stream = FileSystem
        //            .OpenAppPackageFileAsync("Courses.json")
        //            .GetAwaiter()
        //            .GetResult();

        //        using var reader = new StreamReader(stream);
        //        using var doc = JsonDocument.Parse(reader.ReadToEnd());

        //        var root = doc.RootElement;
        //        if (root.ValueKind != JsonValueKind.Array &&
        //            !(root.ValueKind == JsonValueKind.Object &&
        //              root.TryGetProperty("Courses", out root)))
        //            return;

        //        var statements = new List<string>();

        //        foreach (var item in root.EnumerateArray())
        //        {
        //            string name = item.GetProperty("Name").GetString() ?? "";
        //            if (string.IsNullOrWhiteSpace(name))
        //                continue;

        //            string description = item.TryGetProperty("Description", out var d)
        //                ? d.GetString() ?? ""
        //                : "";

        //            int difficulty = item.TryGetProperty("Difficulty", out var diff)
        //                ? diff.GetInt32()
        //                : 0;

        //            name = name.Replace("'", "''");
        //            description = description.Replace("'", "''");

        //            statements.Add(
        //                $"INSERT IGNORE INTO Courses(Name, Description, Difficulty) " +
        //                $"VALUES('{name}', '{description}', {difficulty});"
        //            );
        //        }

        //        if (statements.Count > 0)
        //        {
        //            InsertMultipleWithTransaction(statements);
        //            Debug.WriteLine($"Seeded {statements.Count} courses");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Course seed error: {ex.Message}");
        //    }
        //}

        public List<Course> GetAll()
        {
            courses.Clear();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Name, Description, Difficulty FROM Courses";

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                courses.Add(new Course(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.GetInt32(3)
                ));
            }

            CloseConnection();
            return courses;
        }

        public Course? Get(int id)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Name, Description, Difficulty FROM Courses WHERE Id = @id";

            var p = command.CreateParameter();
            p.ParameterName = "@id";
            p.Value = id;
            command.Parameters.Add(p);

            using DbDataReader reader = command.ExecuteReader();
            Course? result = null;

            if (reader.Read())
            {
                result = new Course(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.GetInt32(3)
                );
            }

            CloseConnection();
            return result;
        }
    }
}
