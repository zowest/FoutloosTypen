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

                // Try to load from JSON, but don't crash if it fails
                try
                {
                    LoadPracticeMaterialsFromJsonAsync().GetAwaiter().GetResult();
                }
                catch (Exception jsonEx)
                {
                    Debug.WriteLine($"Could not load practice materials from JSON (file may not exist yet): {jsonEx.Message}");
                }

                Debug.WriteLine("PracticeMaterialRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PracticeMaterialRepository initialization error: {ex.Message}");
                throw;
            }
        }

        private async Task LoadPracticeMaterialsFromJsonAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("PracticeMaterial.json").ConfigureAwait(false);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync().ConfigureAwait(false);

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
