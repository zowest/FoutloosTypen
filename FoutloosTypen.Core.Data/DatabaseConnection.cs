using MySqlConnector;
using System.Data;
using System.Diagnostics;

namespace FoutloosTypen.Core.Data
{
    public abstract class DatabaseConnection : IDisposable
    {
        protected MySqlConnection Connection { get; }

        protected DatabaseConnection()
        {
            var connectionString =
                "Server=localhost;" +
                "Port=3306;" +
                "Database=boltype;" +
                "User=bolType_user;" +
                "Password=boltype;" +
                "Pooling=true;";

            Connection = new MySqlConnection(connectionString);
            Debug.WriteLine("[DB] Using MySQL");
        }

        protected void OpenConnection()
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
        }

        protected void CloseConnection()
        {
            if (Connection.State != ConnectionState.Closed)
                Connection.Close();
        }

        protected void CreateTable(string sql)
        {
            OpenConnection();
            using var cmd = new MySqlCommand(sql, Connection);
            cmd.ExecuteNonQuery();
        }

        protected void InsertMultipleWithTransaction(IEnumerable<string> statements)
        {
            OpenConnection();
            using var tx = Connection.BeginTransaction();

            try
            {
                foreach (var sql in statements)
                {
                    using var cmd = new MySqlCommand(sql, Connection, tx);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void Dispose()
        {
            CloseConnection();
            Connection.Dispose();
        }
    }
}
