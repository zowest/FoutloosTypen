using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Maui.Storage;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Repositories;
using System.Diagnostics;
using System.Linq;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class PracticeMaterialRepository : DatabaseConnection, IPracticeMaterialRepository
    {
        public PracticeMaterialRepository()
        {
            try
            {
                Debug.WriteLine("PracticeMaterialRepository: Starting initialization...");

                CreateTable(@"
                    CREATE TABLE IF NOT EXISTS PracticeMaterials (
                        Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        Sentence NVARCHAR(500) NOT NULL,
                        AssignmentId INTEGER NOT NULL
                    );
                ");

                Debug.WriteLine("PracticeMaterialRepository: Table created");

                LoadPracticeMaterialsFromJsonSync();

                Debug.WriteLine("PracticeMaterialRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PracticeMaterialRepository initialization error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void LoadPracticeMaterialsFromJsonSync()
        {
            var stopwatch = Stopwatch.StartNew();
            List<string> insertQueries = new();

            try
            {
                Debug.WriteLine("Loading PracticeMaterial.json...");

                using var stream = FileSystem.OpenAppPackageFileAsync("PracticeMaterial.json").GetAwaiter().GetResult();
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();

                using var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;

                // Accept multiple shapes: array root, or object with common property names
                JsonElement materialsElement = root;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("PracticeMaterials", out var pmPlural))
                        materialsElement = pmPlural;
                    else if (root.TryGetProperty("PracticeMaterial", out var pmSingular))
                        materialsElement = pmSingular;
                    else if (root.TryGetProperty("Items", out var items))
                        materialsElement = items;
                    else if (root.TryGetProperty("Data", out var data))
                        materialsElement = data;
                }

                if (materialsElement.ValueKind != JsonValueKind.Array)
                    throw new Exception("Invalid JSON structure: expected array or object with 'PracticeMaterials' or 'PracticeMaterial'");

                foreach (var item in materialsElement.EnumerateArray())
                {
                    // Support different casing/keys
                    int assignmentId = item.TryGetProperty("AssignmentId", out var aid) ? aid.GetInt32() :
                                       item.TryGetProperty("assignmentId", out var aid2) ? aid2.GetInt32() : 0;
                    string sentence = item.TryGetProperty("Sentence", out var s) ? (s.GetString() ?? "") :
                                      item.TryGetProperty("sentence", out var s2) ? (s2.GetString() ?? "") : "";

                    if (assignmentId == 0 || string.IsNullOrWhiteSpace(sentence))
                        continue;

                    sentence = sentence.Replace("'", "''");

                    insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('{sentence}', {assignmentId})");
                }

                if (insertQueries.Any())
                {
                    InsertMultipleWithTransaction(insertQueries);
                    stopwatch.Stop();
                    Debug.WriteLine($"SUCCESS: Loaded {insertQueries.Count} practice materials from JSON in {stopwatch.ElapsedMilliseconds}ms");
                }
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("PracticeMaterial.json not found; skipping JSON seed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR loading PracticeMaterial.json: {ex.Message}");
                Debug.WriteLine("Using fallback data");
            }
        }

        public List<PracticeMaterial> GetAll()
        {
            List<PracticeMaterial> practiceMaterials = new();

            try
            {
                OpenConnection();

                using var command = Connection.CreateCommand();
                command.CommandText = "SELECT Id, Sentence, AssignmentId FROM PracticeMaterials";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string sentence = reader.GetString(1);
                    int assignmentId = reader.GetInt32(2);

                    practiceMaterials.Add(new PracticeMaterial(id, sentence, assignmentId));
                }

                Debug.WriteLine($"Retrieved {practiceMaterials.Count} practice materials from database");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving practice materials: {ex.Message}");
                throw;
            }
            finally
            {
                CloseConnection();
            }

            return practiceMaterials;
        }

        public PracticeMaterial? Get(int id)
        {
            PracticeMaterial? practiceMaterial = null;

            try
            {
                string query = "SELECT Id, Sentence, AssignmentId FROM PracticeMaterials WHERE Id = @Id";

                OpenConnection();
                using (SqliteCommand command = new(query, Connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    SqliteDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string sentence = reader.GetString(1);
                        int assignmentId = reader.GetInt32(2);

                        practiceMaterial = new PracticeMaterial(id, sentence, assignmentId);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving practice material with Id {id}: {ex.Message}");
                throw;
            }
            finally
            {
                CloseConnection();
            }

            return practiceMaterial;
        }
    }
}