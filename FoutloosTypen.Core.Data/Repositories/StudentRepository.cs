using System.Data.Common;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class StudentRepository : DatabaseConnection, IStudentRepository
    {
        public Student? Get(string username)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT id, username, name, password, level, avgSpeed, avgPrecision
                FROM students
                WHERE username = @username
            """;

            var p = command.CreateParameter();
            p.ParameterName = "@username";
            p.Value = username;
            command.Parameters.Add(p);

            using DbDataReader reader = command.ExecuteReader();
            Student? result = null;

            if (reader.Read())
            {
                result = new Student(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetDouble(5),
                    reader.GetDouble(6)
                );
            }

            CloseConnection();
            return result;
        }

        public Student? Get(int id)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT id, username, name, password, level, avgSpeed, avgPrecision
                FROM students
                WHERE id = @id
            """;

            var p = command.CreateParameter();
            p.ParameterName = "@id";
            p.Value = id;
            command.Parameters.Add(p);

            using DbDataReader reader = command.ExecuteReader();
            Student? result = null;

            if (reader.Read())
            {
                result = new Student(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetDouble(5),
                    reader.GetDouble(6)
                );
            }

            CloseConnection();
            return result;
        }

        public List<Student> GetAll()
        {
            var result = new List<Student>();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText = """
                SELECT id, username, name, password, level, avgSpeed, avgPrecision
                FROM students
            """;

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Student(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetDouble(5),
                    reader.GetDouble(6)
                ));
            }

            CloseConnection();
            return result;
        }
    }
}
