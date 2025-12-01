using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using Microsoft.Data.Sqlite;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class LessonRepository : DatabaseConnection, ILessonRepository
    {
        private readonly List<Lesson> lessons = [];

        public LessonRepository()
        {
            CreateTable(@"CREATE TABLE IF NOT EXISTS Lessons (
                        [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [Name] NVARCHAR(80) UNIQUE NOT NULL,
                        [Description] NVARCHAR(250),
                        [IsTest] BOOL,
                        [IsDone] BOOL,
                        [CourseId] INTEGER NOT NULL
                )");

            List<string> insertQueries = new()
        {
            @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 1', 'Dit is de allereerste les', False, False, 1)",
            @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 2', 'Dit is de tweede les', False, False, 1)",
            @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 3', 'Dit is de derde les', False, False, 1)",
            @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 4', 'Dit is de vierde les', False, False, 1)",
            @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 5', 'Dit is de vijfde les', False, False, 1)",
            @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Test Les', 'Dit is een test les', False, False, 2)"
        };

            InsertMultipleWithTransaction(insertQueries);
            GetAll();
        }


        public List<Lesson> GetAll()
        {
            lessons.Clear();

            string selectQuery = "SELECT Id, Name, Description, IsTest, IsDone, CourseId FROM Lessons";
            OpenConnection();

            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    string description = reader.GetString(2);
                    bool isTest = reader.GetBoolean(3);
                    bool isDone = reader.GetBoolean(4);
                    int courseId = reader.GetInt32(5);

                    lessons.Add(new Lesson(id, name, description, isTest, isDone, courseId));
                }
            }

            CloseConnection();
            return lessons;
        }


        public Lesson Get(int id)
        {
            string selectQuery = $"SELECT Id, Name, Description, IsTest, IsDone, CourseId FROM Lessons WHERE Id = {id}";
            Lesson tmpLesson = null;

            OpenConnection();

            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    int Id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    string description = reader.GetString(2);
                    bool isTest = reader.GetBoolean(3);
                    bool isDone = reader.GetBoolean(4);
                    int courseId = reader.GetInt32(5);

                    tmpLesson = new Lesson(Id, name, description, isTest, isDone, courseId);
                }
            }

            CloseConnection();
            return tmpLesson;
        }
    }
}