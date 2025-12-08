namespace FoutloosTypen.Core.Data.Helpers
{
    public static class ConnectionHelper
    {
        public static string ConnectionStringValue(string databaseName)
        {
            // Return the database filename with .db extension
            return $"{databaseName}.db";
        }
    }
}