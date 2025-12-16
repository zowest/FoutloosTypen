using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Maui.Storage;
using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class AssignmentRepository : DatabaseConnection, IAssignmentRepository
    {
        private readonly List<Assignment> assignments = [];

        public AssignmentRepository()
        {
            CreateTable("""
                CREATE TABLE IF NOT EXISTS Assignments (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    TimeLimit DOUBLE NOT NULL,
                    LessonId INT NOT NULL
                );
            """);

            //LoadAssignmentsFromJsonSync();
        }

        //private void LoadAssignmentsFromJsonSync()
        //{
        //    try
        //    {
        //        using var stream = FileSystem
        //            .OpenAppPackageFileAsync("Assignment.json")
        //            .GetAwaiter()
        //            .GetResult();

        //        using var reader = new StreamReader(stream);
        //        using var doc = JsonDocument.Parse(reader.ReadToEnd());

        //        var root = doc.RootElement;

        //        JsonElement items = root;
        //        if (root.ValueKind == JsonValueKind.Object)
        //        {
        //            if (root.TryGetProperty("Assignment", out var a))
        //                items = a;
        //            else if (root.TryGetProperty("Assignments", out var ap))
        //                items = ap;
        //            else
        //                return;
        //        }

        //        if (items.ValueKind != JsonValueKind.Array)
        //            return;

        //        var statements = new List<string>();

        //        foreach (var item in items.EnumerateArray())
        //        {
        //            double timeLimit = item.GetProperty("TimeLimit").GetDouble();
        //            int lessonId = item.GetProperty("LessonId").GetInt32();

        //            statements.Add(
        //                $"INSERT INTO Assignments(TimeLimit, LessonId) VALUES({timeLimit}, {lessonId});"
        //            );
        //        }

        //        if (statements.Count > 0)
        //            InsertMultipleWithTransaction(statements);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Assignment seed error: {ex.Message}");
        //        throw;
        //    }
        //}


        public List<Assignment> GetAll()
        {
            assignments.Clear();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, TimeLimit, LessonId FROM Assignments";

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                assignments.Add(new Assignment(
                    reader.GetInt32(0),
                    reader.GetDouble(1),
                    reader.GetInt32(2)
                ));
            }

            CloseConnection();
            return assignments;
        }

        public Assignment? Get(int id)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, TimeLimit, LessonId FROM Assignments WHERE Id = @id";

            var p = command.CreateParameter();
            p.ParameterName = "@id";
            p.Value = id;
            command.Parameters.Add(p);

            using DbDataReader reader = command.ExecuteReader();
            Assignment? result = null;

            if (reader.Read())
            {
                result = new Assignment(
                    reader.GetInt32(0),
                    reader.GetDouble(1),
                    reader.GetInt32(2)
                );
            }

            CloseConnection();
            return result;
        }
    }
}
