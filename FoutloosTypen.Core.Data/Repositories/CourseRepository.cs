using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Maui.Storage;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Data.Common;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class CourseRepository : DatabaseConnection, ICourseRepository
    {
        private readonly List<Course> courses = [];

        public CourseRepository()
        {
            CreateTable("""
                CREATE TABLE IF NOT EXISTS Courses (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(80) UNIQUE NOT NULL,
                    Description VARCHAR(250),
                    Difficulty INT
                );
            """);
        }

        public List<Course> GetAll()
        {
            courses.Clear();
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Name, Description, Difficulty FROM Courses";

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                courses.Add(new Course(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.GetInt32(3)
                ));
            }

            CloseConnection();
            return courses;
        }

        public Course? Get(int id)
        {
            OpenConnection();

            using var command = Connection.CreateCommand();
            command.CommandText =
                "SELECT Id, Name, Description, Difficulty FROM Courses WHERE Id = @id";

            var p = command.CreateParameter();
            p.ParameterName = "@id";
            p.Value = id;
            command.Parameters.Add(p);

            using DbDataReader reader = command.ExecuteReader();
            Course? result = null;

            if (reader.Read())
            {
                result = new Course(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.GetInt32(3)
                );
            }

            CloseConnection();
            return result;
        }
    }
}
