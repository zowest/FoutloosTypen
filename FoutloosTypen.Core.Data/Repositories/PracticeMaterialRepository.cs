using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Maui.Storage;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Data.Repositories
{
    internal class PracticeMaterialRepository : DatabaseConnection
    {
        public PracticeMaterialRepository()
        {
            CreateTable(@"
                CREATE TABLE IF NOT EXISTS PracticeMaterials (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    Sentences NVARCHAR(300) NOT NULL,
                    AssignmentId INTEGER NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS idx_unique_practice 
                ON PracticeMaterials(Sentences, AssignmentId);
            ");

            LoadPracticeMaterialsFromJsonAsync().Wait();
        }

        private async Task LoadPracticeMaterialsFromJsonAsync()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("PracticeMaterials.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;
                var items = root.GetProperty("PracticeMaterial");

                OpenConnection();

                foreach (var item in items.EnumerateArray())
                {
                    int lessonId = item.GetProperty("LessonId").GetInt32();
                    double timeLimit = item.GetProperty("TimeLimit").GetDouble();
                    string sentence = item.GetProperty("Sentence").GetString() ?? "";

                    int assignmentId;

                    // Zoek bestaande assignment of voeg toe
                    using (var checkCmd = Connection.CreateCommand())
                    {
                        checkCmd.CommandText = @"
                            SELECT Id FROM Assignments 
                            WHERE LessonId = @LessonId AND TimeLimit = @TimeLimit LIMIT 1;";
                        checkCmd.Parameters.AddWithValue("@LessonId", lessonId);
                        checkCmd.Parameters.AddWithValue("@TimeLimit", timeLimit);
                        var result = checkCmd.ExecuteScalar();

                        if (result != null)
                        {
                            assignmentId = Convert.ToInt32(result);
                        }
                        else
                        {
                            using var insertCmd = Connection.CreateCommand();
                            insertCmd.CommandText = @"
                                INSERT INTO Assignments(TimeLimit, LessonId) 
                                VALUES(@TimeLimit, @LessonId);
                                SELECT last_insert_rowid();";
                            insertCmd.Parameters.AddWithValue("@TimeLimit", timeLimit);
                            insertCmd.Parameters.AddWithValue("@LessonId", lessonId);
                            assignmentId = Convert.ToInt32(insertCmd.ExecuteScalar());
                        }
                    }

                    // Check of de combinatie al bestaat
                    using (var checkPractice = Connection.CreateCommand())
                    {
                        checkPractice.CommandText = @"
                            SELECT COUNT(*) FROM PracticeMaterials 
                            WHERE Sentences = @Sentence AND AssignmentId = @AssignmentId";
                        checkPractice.Parameters.AddWithValue("@Sentence", sentence);
                        checkPractice.Parameters.AddWithValue("@AssignmentId", assignmentId);

                        long count = (long)checkPractice.ExecuteScalar();
                        if (count > 0)
                            continue; // skip duplicaat
                    }

                    // Voeg nieuwe regel toe
                    using var insertPractice = Connection.CreateCommand();
                    insertPractice.CommandText = @"
                        INSERT INTO PracticeMaterials(Sentences, AssignmentId) 
                        VALUES(@Sentence, @AssignmentId)";
                    insertPractice.Parameters.AddWithValue("@Sentence", sentence);
                    insertPractice.Parameters.AddWithValue("@AssignmentId", assignmentId);
                    insertPractice.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Fout bij het importeren van PracticeMaterials.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        public List<PracticeMaterial> GetAll()
        {
            List<PracticeMaterial> practiceMaterials = new();

            try
            {
                OpenConnection();

                using var command = Connection.CreateCommand();
                command.CommandText = "SELECT Id, Sentences, AssignmentId FROM PracticeMaterials";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string sentence = reader.GetString(1);
                    int assignmentId = reader.GetInt32(2);

                    practiceMaterials.Add(new PracticeMaterial(id, sentence, assignmentId));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Fout bij ophalen van PracticeMaterials.", ex);
            }
            finally
            {
                CloseConnection();
            }

            return practiceMaterials;
        }
    }
}
