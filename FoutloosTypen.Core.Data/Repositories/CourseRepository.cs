using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Diagnostics;


namespace FoutloosTypen.Core.Data.Repositories
{
    public class CourseRepository : DatabaseConnection, ICourseRepository
    {
        private readonly List<Course> courses = [];

        public CourseRepository()
        {
            try
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
                    @"INSERT OR IGNORE INTO Courses(Name, Description, Difficulty) VALUES('Course 3', 'Voor de echte pro''s', 3)"
                };

                InsertMultipleWithTransaction(insertQueries);
                Debug.WriteLine("CourseRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CourseRepository initialization error: {ex.Message}");
                throw;
            }
        }

        public List<Course> GetAll()
        {
            courses.Clear();

            try
            {
                string selectQuery = "SELECT Id, Name, Description, Difficulty FROM Courses";
                OpenConnection();

                using (SqliteCommand command = new(selectQuery, Connection))
                {
                    SqliteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        string description = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        int difficulty = reader.GetInt32(3);

                        courses.Add(new Course(id, name, description, difficulty));
                        Debug.WriteLine($"Loaded course: {id} - {name}");
                    }
                }

                CloseConnection();
                Debug.WriteLine($"Total courses loaded: {courses.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading courses: {ex.Message}");
                CloseConnection();
            }

            return courses;
        }

        public Course Get(int id)
        {
            string selectQuery = $"SELECT Id, Name, Description, Difficulty FROM Courses WHERE Id = {id}";
            Course tmpCourse = null;

            try
            {
                OpenConnection();

                using (SqliteCommand command = new(selectQuery, Connection))
                {
                    SqliteDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        int Id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        string description = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        int difficulty = reader.GetInt32(3);

                        tmpCourse = new Course(Id, name, description, difficulty);
                    }
                }

                CloseConnection();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading course {id}: {ex.Message}");
                CloseConnection();
            }

            return tmpCourse;
        }
    }
}