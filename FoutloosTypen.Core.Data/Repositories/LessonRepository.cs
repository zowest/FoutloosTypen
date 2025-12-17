using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using System.Diagnostics;
using System.Data.Common;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class LessonRepository : DatabaseConnection, ILessonRepository
    {
        private readonly List<Lesson> lessons = [];

        public LessonRepository()
        {
            CreateTable("""
                CREATE TABLE IF NOT EXISTS Lessons (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(80) NOT NULL,
                    Description VARCHAR(250),
                    IsTest TINYINT(1),
                    IsDone TINYINT(1),
                    CourseId INT NOT NULL,
                    UNIQUE(Name, CourseId)
                );
            """);
            
            // JSON loading removed - data komt nu uit de database
            Debug.WriteLine("[LessonRepository] Table created, using database data");
        }

        public List<Lesson> GetAll()
        {
            lessons.Clear();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Name, Description, IsTest, IsDone, CourseId FROM Lessons";

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                lessons.Add(new Lesson(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.GetInt32(3) == 1,
                    reader.GetInt32(4) == 1,
                    reader.GetInt32(5),
                    totalTime: 60
                ));
            }

            CloseConnection();
            return lessons;
        }

        public Lesson? Get(int id)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Name, Description, IsTest, IsDone, CourseId FROM Lessons WHERE Id = @id";

            var p = command.CreateParameter();
            p.ParameterName = "@id";
            p.Value = id;
            command.Parameters.Add(p);

            using DbDataReader reader = command.ExecuteReader();
            Lesson? result = null;

            if (reader.Read())
            {
                result = new Lesson(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.GetInt32(3) == 1,
                    reader.GetInt32(4) == 1,
                    reader.GetInt32(5),
                    totalTime: 60
                );
            }

            CloseConnection();
            return result;
        }
    }
}
