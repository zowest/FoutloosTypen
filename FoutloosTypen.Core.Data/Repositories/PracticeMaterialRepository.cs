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
                CreateTable(@"
                    CREATE TABLE IF NOT EXISTS PracticeMaterials (
                        Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        Sentence NVARCHAR(500) NOT NULL,
                        AssignmentId INTEGER NOT NULL
                    );
                ");

                // Probeer data uit JSON te laden
                try
                {
                    LoadPracticeMaterialsFromJsonAsync().Wait();
                    Debug.WriteLine("PracticeMaterialRepository: Loaded from JSON");
                }
                catch (Exception jsonEx)
                {
                    Debug.WriteLine($"Could not load from JSON: {jsonEx.Message}");
                    Debug.WriteLine("Using fallback hardcoded data");
                    InsertFallbackData();
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
            var stopwatch = Stopwatch.StartNew();
            List<string> insertQueries = new();

            try
            {
                // Probeer het JSON bestand te laden
                using var stream = await FileSystem.OpenAppPackageFileAsync("PracticeMaterial.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                Debug.WriteLine($"JSON file loaded in {stopwatch.ElapsedMilliseconds}ms");

                // Parse JSON
                var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;
                
                // Check of de property "PracticeMaterials" bestaat
                if (!root.TryGetProperty("PracticeMaterials", out var materials))
                {
                    Debug.WriteLine("ERROR: JSON property 'PracticeMaterials' not found");
                    throw new Exception("Invalid JSON structure: 'PracticeMaterials' property missing");
                }

                // Loop door alle materials
                foreach (var item in materials.EnumerateArray())
                {
                    int assignmentId = item.GetProperty("AssignmentId").GetInt32();
                    string sentence = item.GetProperty("Sentence").GetString() ?? "";
                    
                    // Escape single quotes voor SQL
                    sentence = sentence.Replace("'", "''");

                    insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) 
                                VALUES('{sentence}', {assignmentId})");
                }

                Debug.WriteLine($"Parsed {insertQueries.Count} materials in {stopwatch.ElapsedMilliseconds}ms");

                if (insertQueries.Any())
                {
                    InsertMultipleWithTransaction(insertQueries);
                    stopwatch.Stop();
                    Debug.WriteLine($"SUCCESS: Loaded {insertQueries.Count} practice materials from JSON in {stopwatch.ElapsedMilliseconds}ms");
                }
                else
                {
                    Debug.WriteLine("WARNING: No materials found in JSON");
                    throw new Exception("JSON file contains no materials");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR loading PracticeMaterial.json: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void InsertFallbackData()
        {
            // Fallback: alleen de eerste 10 zinnen als voorbeeld
            Debug.WriteLine("Inserting fallback practice materials...");
            
            List<string> insertQueries = new()
            {
                @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De kat speelt met een bal. De hond rent door de tuin. Het is een mooie zonnige dag vandaag. Ik zie een vogel in de boom zitten.', 1)",
                @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het huis heeft een rode deur. De tuin is vol met bloemen. Kinderen spelen buiten op straat. De zon schijnt helder aan de hemel.', 2)",
                @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Mijn auto staat voor het huis. De fiets is nieuw en blauw. We gaan wandelen in het park. Het weer is vandaag erg mooi weer.', 3)",
                @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De school begint om acht uur. Leerlingen lopen naar binnen. De leraar staat voor het bord. Iedereen is klaar voor de les.', 4)",
                @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het boek ligt op tafel. De lamp geeft veel licht. Ik lees graag in de avond. Het is stil en rustig hier.', 5)",
                @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Mijn vriend heet Jan en woont in Amsterdam. We gaan vaak samen wandelen in het park. Het weer is vandaag erg mooi en warm buiten.', 6)",
                @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De leraar schrijft op het bord. Leerlingen maken aantekeningen. Het is stil in de klas nu. Iedereen luistert goed naar de uitleg.', 7)",
                @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De winkel verkoopt verse groente. Ik koop appels en peren. De kassière is vriendelijk. Het brood ruikt lekker vandaag hier.', 8)",
                @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De bus komt om half negen. We stappen in bij het station. Het is druk in de ochtend. Veel mensen gaan naar hun werk.', 9)",
                @"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Mijn zus studeert medicijnen. Ze werkt hard elke dag. De universiteit is ver weg. Maar ze vindt het heel leuk.', 10)"
            };

            InsertMultipleWithTransaction(insertQueries);
            Debug.WriteLine($"Inserted {insertQueries.Count} fallback practice materials");
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
                
                Debug.WriteLine($"Retrieved practice material with Id {id}: {(practiceMaterial != null ? "Found" : "Not found")}");
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
