using System.Diagnostics;
using System.Text.Json;
using Microsoft.Maui.Storage;
using System.Data.Common;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Repositories;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class PracticeMaterialRepository : DatabaseConnection, IPracticeMaterialRepository
    {
        public PracticeMaterialRepository()
        {
            CreateTable("""
                CREATE TABLE IF NOT EXISTS PracticeMaterials (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    Sentence VARCHAR(500) NOT NULL,
                    AssignmentId INT NOT NULL
                );
            """);

            //LoadPracticeMaterialsFromJsonSync();
        }

        //private void LoadPracticeMaterialsFromJsonSync()
        //{
        //    var stopwatch = Stopwatch.StartNew();
        //    var statements = new List<string>();

        //    try
        //    {
        //        using var stream = FileSystem
        //            .OpenAppPackageFileAsync("PracticeMaterial.json")
        //            .GetAwaiter()
        //            .GetResult();

        //        using var reader = new StreamReader(stream);
        //        using var doc = JsonDocument.Parse(reader.ReadToEnd());
        //        var root = doc.RootElement;

        //        if (root.ValueKind == JsonValueKind.Object &&
        //            root.TryGetProperty("PracticeMaterials", out var p))
        //        {
        //            root = p;
        //        }

        //        if (root.ValueKind != JsonValueKind.Array)
        //            return;

        //        foreach (var item in root.EnumerateArray())
        //        {
        //            int assignmentId = item.GetProperty("AssignmentId").GetInt32();
        //            string sentence = item.GetProperty("Sentence").GetString() ?? "";

        //            if (assignmentId == 0 || string.IsNullOrWhiteSpace(sentence))
        //                continue;

        //            sentence = sentence.Replace("'", "''");

        //            statements.Add($"""
        //                INSERT IGNORE INTO PracticeMaterials
        //                (Sentence, AssignmentId)
        //                VALUES('{sentence}', {assignmentId});
        //            """);
        //        }

        //        if (statements.Count > 0)
        //        {
        //            InsertMultipleWithTransaction(statements);
        //            stopwatch.Stop();
        //            Debug.WriteLine($"Seeded {statements.Count} practice materials in {stopwatch.ElapsedMilliseconds}ms");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"PracticeMaterial seed error: {ex.Message}");
        //    }
        //}

        public List<PracticeMaterial> GetAll()
        {
            var result = new List<PracticeMaterial>();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Sentence, AssignmentId FROM PracticeMaterials";

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PracticeMaterial(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetInt32(2)
                ));
            }

            CloseConnection();
            return result;
        }

        public PracticeMaterial? Get(int id)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Sentence, AssignmentId FROM PracticeMaterials WHERE Id = @id";

            var p = command.CreateParameter();
            p.ParameterName = "@id";
            p.Value = id;
            command.Parameters.Add(p);

            using DbDataReader reader = command.ExecuteReader();
            PracticeMaterial? result = null;

            if (reader.Read())
            {
                result = new PracticeMaterial(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetInt32(2)
                );
            }

            CloseConnection();
            return result;
        }
    }
}
