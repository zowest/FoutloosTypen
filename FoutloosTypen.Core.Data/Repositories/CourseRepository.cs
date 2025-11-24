using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Data.Sqlite;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class CourseRepository : DatabaseConnection, ICourseRepository
    {
        private readonly List<Course> courses = [];

        public CourseRepository()
        {
            CreateTable(@"
            CREATE TABLE IF NOT EXISTS Courses (
                [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                [Name] NVARCHAR(80) UNIQUE NOT NULL,
                [Description] NVARCHAR(250),
                [Difficulty] INTEGER
            )");

            List<string> insertQueries = new()
        {
            @"INSERT OR IGNORE INTO Courses(Name, Description, Difficulty) VALUES('Course 1', 'Leer de basics van typen', 1)",
            @"INSERT OR IGNORE INTO Courses(Name, Description, Difficulty) VALUES('Course 2', 'Voor snelle typers', 2)",
            @"INSERT OR IGNORE INTO Courses(Name, Description, Difficulty) VALUES('Course 3', 'Voor de echte pro's', 3)"
        };

            InsertMultipleWithTransaction(insertQueries);
        }

        public List<Course> GetAll()
        {
            courses.Clear();

            string selectQuery = "SELECT Id, Name, Description, Difficulty FROM Courses";
            OpenConnection();

            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    string description = reader.GetString(2);
                    int difficulty = reader.GetInt32(3);

                    courses.Add(new Course(id, name, description, difficulty));
                }
            }

            CloseConnection();
            return courses;
        }

        public Course Get(int id)
        {
            string selectQuery = $"SELECT Id, Name, Description, Difficulty FROM Courses WHERE Id = {id}";
            Course tmpCourse = null;

            OpenConnection();

            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    int Id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    string description = reader.GetString(2);
                    int difficulty = reader.GetInt32(3);

                    tmpCourse = new Course(Id, name, description, difficulty);
                }
            }

            CloseConnection();
            return tmpCourse;
        }
    }
}