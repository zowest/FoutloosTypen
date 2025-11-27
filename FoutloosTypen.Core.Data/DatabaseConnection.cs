using FoutloosTypen.Core.Data.Helpers;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using Microsoft.Maui.Storage;

namespace FoutloosTypen.Core.Data
{
    public abstract class DatabaseConnection : IDisposable
    {
        protected SqliteConnection Connection { get; }
        string databaseName;

        public DatabaseConnection()
        {
            databaseName = ConnectionHelper.ConnectionStringValue("FoutloosTypenDb");

            string dbDirectory = FileSystem.AppDataDirectory;
            string dbpath = Path.Combine(dbDirectory, databaseName);

            // Log the database path for debugging
            Debug.WriteLine($"Database path: {dbpath}");

            Connection = new SqliteConnection($"Data Source={dbpath}");
        }

        protected void OpenConnection()
        {
            if (Connection.State != System.Data.ConnectionState.Open) 
            {
                Connection.Open();
                Debug.WriteLine("Database connection opened");
            }
        }

        protected void CloseConnection()
        {
            if (Connection.State != System.Data.ConnectionState.Closed) 
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
                    Debug.WriteLine($"Table created: {commandText}");
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
                linesToInsert.ForEach(l => 
                {
                    Connection.ExecuteNonQuery(l);
                    Debug.WriteLine($"Executed: {l}");
                });
                transaction.Commit();
                Debug.WriteLine("Transaction committed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Transaction error: {ex.Message}");
                transaction.Rollback();
                throw;
            }
            finally
            {
                transaction.Dispose();
            }
        }

        public void Dispose()
        {
            CloseConnection();
        }
    }
}