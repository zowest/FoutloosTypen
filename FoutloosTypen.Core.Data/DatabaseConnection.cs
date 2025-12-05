using FoutloosTypen.Core.Data.Helpers;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Diagnostics;

namespace FoutloosTypen.Core.Data
{
    public abstract class DatabaseConnection : IDisposable
    {
        protected SqliteConnection Connection { get; }
        string databaseName;

        public DatabaseConnection() : this(null)
        {
        }

        /// <summary>
        /// Constructor met optionele database path voor testdoeleinden
        /// </summary>
        protected DatabaseConnection(string? customDatabasePath)
        {
            databaseName = ConnectionHelper.ConnectionStringValue("FoutloosTypenDb");

            string dbpath;
            
            if (!string.IsNullOrEmpty(customDatabasePath))
            {
                // Gebruik custom path voor tests
                dbpath = customDatabasePath;
                Debug.WriteLine($"Using custom database path: {dbpath}");
            }
            else
            {
                // Gebruik normale MAUI path voor productie
                try
                {
                    string dbDirectory = FileSystem.AppDataDirectory;
                    dbpath = Path.Combine(dbDirectory, databaseName);
                    Debug.WriteLine($"Using MAUI AppDataDirectory: {dbpath}");
                }
                catch (Exception ex)
                {
                    // Fallback voor test omgeving
                    Debug.WriteLine($"FileSystem.AppDataDirectory not available: {ex.Message}");
                    string tempDir = Path.Combine(Path.GetTempPath(), "FoutloosTypenTests");
                    Directory.CreateDirectory(tempDir);
                    dbpath = Path.Combine(tempDir, databaseName);
                    Debug.WriteLine($"Using fallback test directory: {dbpath}");
                }
            }

            Connection = new SqliteConnection($"Data Source={dbpath}");
        }

        protected void OpenConnection()
        {
            if (Connection.State != ConnectionState.Open) 
            {
                Connection.Open();
                Debug.WriteLine("Database connection opened");
            }
        }

        protected void CloseConnection()
        {
            if (Connection.State != ConnectionState.Closed) 
            {
                Connection.Close();
                Debug.WriteLine("Database connection closed");
            }
        }

        public void CreateTable(string commandText)
        {
            try
            {
                OpenConnection();
                using (var command = Connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                    Debug.WriteLine($"Table created successfully");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating table: {ex.Message}");
                throw;
            }
        }

        public void InsertMultipleWithTransaction(List<string> linesToInsert)
        {
            OpenConnection();
            var transaction = Connection.BeginTransaction();

            try
            {
                foreach (var line in linesToInsert)
                {
                    using var command = Connection.CreateCommand();
                    command.CommandText = line;
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
                Debug.WriteLine($"Transaction committed: {linesToInsert.Count} statements");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Transaction error: {ex.Message}");
                transaction.Rollback();
                throw;
            }
        }

        public void Dispose()
        {
            CloseConnection();
            Connection?.Dispose();
        }
    }
}