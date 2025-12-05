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
                        [Name] NVARCHAR(80) NOT NULL,
                        [Description] NVARCHAR(250),
                        [IsTest] BOOL,
                        [IsDone] BOOL,
                        [CourseId] INTEGER NOT NULL,
                        UNIQUE(Name, CourseId)
                )");

            List<string> insertQueries = new()
            {
                // Cursus 1 - Beginners (10 lessen)
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 1', 'Leer typen met eenvoudige korte zinnen', 0, 0, 1)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 2', 'Oefen met veelgebruikte woorden', 0, 0, 1)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 3', 'Type eenvoudige vraagzinnen', 0, 0, 1)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 4', 'Oefen met korte mededelingen', 0, 0, 1)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 5', 'Type eenvoudige instructies', 0, 0, 1)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 6', 'Oefen met begroetingen en afscheid', 0, 0, 1)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 7', 'Type zinnen met cijfers en tijden', 0, 0, 1)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 8', 'Oefen met winkeldialogen', 0, 0, 1)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 9', 'Type over het weer', 0, 0, 1)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 10', 'Test je basiskennis', 1, 0, 1)",
                
                // Cursus 2 - Gevorderden (10 lessen)
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 1', 'Oefen met uitgebreidere zinnen', 0, 0, 2)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 2', 'Type professionele berichten', 0, 0, 2)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 3', 'Oefen met korte verhalen', 0, 0, 2)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 4', 'Type technische beschrijvingen', 0, 0, 2)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 5', 'Oefen met nieuwsberichten', 0, 0, 2)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 6', 'Type complexere zinsstructuren', 0, 0, 2)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 7', 'Oefen met gesprekken', 0, 0, 2)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 8', 'Type officiële correspondentie', 0, 0, 2)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 9', 'Oefen met beoordelingen schrijven', 0, 0, 2)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 10', 'Test je gevorderde vaardigheden', 1, 0, 2)",
                
                // Cursus 3 - Expert (10 lessen)
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 1', 'Type literaire fragmenten', 0, 0, 3)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 2', 'Oefen met academische content', 0, 0, 3)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 3', 'Type juridische teksten', 0, 0, 3)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 4', 'Oefen met diepe onderwerpen', 0, 0, 3)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 5', 'Type gedetailleerde instructies', 0, 0, 3)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 6', 'Oefen met gedichten en rijm', 0, 0, 3)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 7', 'Type historische beschrijvingen', 0, 0, 3)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 8', 'Oefen met beargumenteerde teksten', 0, 0, 3)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 9', 'Type creatieve verhalen', 0, 0, 3)",
                @"INSERT OR IGNORE INTO Lessons(Name, Description, IsTest, IsDone, CourseId) VALUES('Les 10', 'Test je expertise', 1, 0, 3)"
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
                    double totalTime = 60; // Elke les is 60 seconden

                    lessons.Add(new Lesson(id, name, description, isTest, isDone, courseId, totalTime));
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
                    double totalTime = 60; // Elke les is 60 seconden

                    tmpLesson = new Lesson(Id, name, description, isTest, isDone, courseId, totalTime);
                }
            }

            CloseConnection();
            return tmpLesson;
        }
    }
}