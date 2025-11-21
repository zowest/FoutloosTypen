using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class CourseRepository : DatabaseConnection, ICourseRepository
    {
        private readonly List<Course> courses = new();

        public CourseRepository()
        {
            CreateTable(@"
                CREATE TABLE IF NOT EXISTS Courses (
                    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    [Name] NVARCHAR(80) UNIQUE NOT NULL,
                    [Description] NVARCHAR(250),
                    [Difficulty] INTEGER
                )");

            // Insert demo courses
            List<string> insertQueries = new()
            {
                @"INSERT OR IGNORE INTO Courses(Name, Description, Difficulty) VALUES('Easy', 'Leer de basics van typen', 1)",
                @"INSERT OR IGNORE INTO Courses(Name, Description, Difficulty) VALUES('Medium', 'Voor snelle typers', 2)",
                @"INSERT OR IGNORE INTO Courses(Name, Description, Difficulty) VALUES('Hard', 'Voor de echte pro's', 3)"
            };

            InsertMultipleWithTransaction(insertQueries);
            GetAll();
        }
        public List<Course> GetAll()
        {
            courses.Clear();

            string query = "SELECT Id, Name, Description, Difficulty FROM Courses";

            OpenConnection();

            using (SqliteCommand cmd = new SqliteCommand(query, Connection))
            using (SqliteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    courses.Add(new Course(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetInt32(3)
                    ));
                }
            }

            CloseConnection();
            return courses;
        }

        public Course Get(int id)
        {
            Course course = null;

            string query = $"SELECT Id, Name, Description, Difficulty FROM Courses WHERE Id = {id}";

            OpenConnection();

            using (SqliteCommand cmd = new SqliteCommand(query, Connection))
            using (SqliteDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    course = new Course(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetInt32(3)
                    );
                }
            }

            CloseConnection();
            return course;
        }
    }
}