using System.Diagnostics;
using System.Text.Json;
using Microsoft.Maui.Storage;
using System.Data.Common;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Repositories;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class PracticeMaterialRepository : DatabaseConnection, IPracticeMaterialRepository
    {
        public PracticeMaterialRepository()
        {
            CreateTable("""
                CREATE TABLE IF NOT EXISTS PracticeMaterials (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    Sentence VARCHAR(500) NOT NULL,
                    AssignmentId INT NOT NULL
                );
            """);
        }

        public List<PracticeMaterial> GetAll()
        {
            var result = new List<PracticeMaterial>();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Sentence, AssignmentId FROM PracticeMaterials";

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PracticeMaterial(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetInt32(2)
                ));
            }

            CloseConnection();
            return result;
        }

        public PracticeMaterial? Get(int id)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Sentence, AssignmentId FROM PracticeMaterials WHERE Id = @id";

            var p = command.CreateParameter();
            p.ParameterName = "@id";
            p.Value = id;
            command.Parameters.Add(p);

            using DbDataReader reader = command.ExecuteReader();
            PracticeMaterial? result = null;

            if (reader.Read())
            {
                result = new PracticeMaterial(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetInt32(2)
                );
            }

            CloseConnection();
            return result;
        }
    }
}
