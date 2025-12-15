using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace FoutloosTypen.Core.Data.Helpers
{
    public static class DebugDatabaseReset
    {
        public static void Reset()
        {
#if DEBUG
            try
            {
                var provider = ConnectionHelper.GetProvider();

                if (provider == "Sqlite")
                {
                    ResetSqlite();
                }
                else if (provider == "MySql")
                {
                    ResetMySql();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG] Failed to reset database: {ex.Message}");
            }
#endif
        }

        private static void ResetSqlite()
        {
            string dbName = ConnectionHelper.GetConnectionString();
            string dbDirectory = FileSystem.AppDataDirectory;
            string dbPath = Path.Combine(dbDirectory, dbName);

            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
                Debug.WriteLine($"[DEBUG] Deleted SQLite database at: {dbPath}");
            }
            else
            {
                Debug.WriteLine($"[DEBUG] No SQLite database found at: {dbPath}");
            }
        }

        private static void ResetMySql()
        {
            using var connection = new MySqlConnector.MySqlConnection(
                ConnectionHelper.GetConnectionString());

            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DROP DATABASE IF EXISTS foutloostypen;";
            cmd.ExecuteNonQuery();

            Debug.WriteLine("[DEBUG] Dropped MySQL database 'foutloostypen'");
        }
    }
}
