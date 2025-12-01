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

namespace FoutloosTypen.Core.Data.Repositories
{
    public class PracticeMaterialRepository : DatabaseConnection, IPracticeMaterialRepository
    {
        public PracticeMaterialRepository()
        {
            try
            {
                CreateTable(@"
                    CREATE TABLE IF NOT EXISTS PracticeMaterials (
                        Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        Sentence NVARCHAR(300) NOT NULL,
                        AssignmentId INTEGER NOT NULL
                    );
                ");

                CreateTable(@"
                    CREATE UNIQUE INDEX IF NOT EXISTS idx_unique_practice 
                    ON PracticeMaterials(Sentence, AssignmentId);
                ");

                // Try to load from JSON, but don't crash if it fails
                try
                {
                    LoadPracticeMaterialsFromJsonAsync().Wait();
                }
                catch (Exception jsonEx)
                {
                    Debug.WriteLine($"Could not load practice materials from JSON (file may not exist yet): {jsonEx.Message}");
                    // Insert some default data instead
                    InsertDefaultPracticeMaterials();
                }

                Debug.WriteLine("PracticeMaterialRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PracticeMaterialRepository initialization error: {ex.Message}");
                throw;
            }
        }

        private void InsertDefaultPracticeMaterials()
        {
            try
            {
                List<string> insertQueries = new()
                {
                    @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De hond loopt naar het huis.', 1)",
                    @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De kat zit op de stoel.', 2)",
                    @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De vogel vliegt naar de boom.', 3)",
                    @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het weer is vandaag prachtig.', 4)",
                    @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Ik hou van programmeren.', 5)"
                };

                InsertMultipleWithTransaction(insertQueries);
                Debug.WriteLine("Inserted default practice materials");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inserting default practice materials: {ex.Message}");
            }
        }

        private async Task LoadPracticeMaterialsFromJsonAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("Raw/PracticeMaterial.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var jsonDoc = JsonDocument.Parse(json);
            var root = jsonDoc.RootElement;
            var items = root.GetProperty("PracticeMaterial");

            List<string> insertQueries = new();

            foreach (var item in items.EnumerateArray())
            {
                int id = item.GetProperty("Id").GetInt32();
                int assignmentId = item.GetProperty("AssignmentId").GetInt32();
                string sentence = item.GetProperty("Sentence").GetString() ?? "";

                // Escape single quotes for SQL
                sentence = sentence.Replace("'", "''");

                insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Id, Sentence, AssignmentId) 
                            VALUES({id}, '{sentence}', {assignmentId})");
            }

            InsertMultipleWithTransaction(insertQueries);
            Debug.WriteLine($"Loaded {insertQueries.Count} practice materials from JSON");
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

                Debug.WriteLine($"Retrieved {practiceMaterials.Count} practice materials");
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
