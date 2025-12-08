using FoutloosTypen.Core.Data.Helpers;
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
                string dbName = ConnectionHelper.ConnectionStringValue("FoutloosTypenDb");
                string dbDirectory = FileSystem.AppDataDirectory;
                string dbPath = Path.Combine(dbDirectory, dbName);

                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                    Debug.WriteLine($"[DEBUG] Deleted existing database at: {dbPath}");
                }
                else
                {
                    Debug.WriteLine($"[DEBUG] No database to delete at: {dbPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG] Failed to reset database: {ex.Message}");
            }
#endif
        }
    }
}
