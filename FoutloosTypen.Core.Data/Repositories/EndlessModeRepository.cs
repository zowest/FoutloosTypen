using System.Data.Common;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class EndlessModeRepository : DatabaseConnection, IEndlessModeRepository
    {
        public EndlessModeRepository()
        {
            CreateTable("""
                CREATE TABLE IF NOT EXISTS EndlessWords (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    Word VARCHAR(50) NOT NULL
                );
            """);
        }

        public List<EndlessMode> GetAll()
        {
            var result = new List<EndlessMode>();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Word FROM EndlessWords";

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new EndlessMode
                {
                    Id = reader.GetInt32(0),
                    Word = reader.GetString(1)
                });
            }

            CloseConnection();
            return result;
        }

        public EndlessMode? Get(int id)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Word FROM EndlessWords WHERE Id = @id";

            var p = command.CreateParameter();
            p.ParameterName = "@id";
            p.Value = id;
            command.Parameters.Add(p);

            using DbDataReader reader = command.ExecuteReader();
            EndlessMode? result = null;

            if (reader.Read())
            {
                result = new EndlessMode
                {
                    Id = reader.GetInt32(0),
                    Word = reader.GetString(1)
                };
            }

            CloseConnection();
            return result;
        }
    }
}
