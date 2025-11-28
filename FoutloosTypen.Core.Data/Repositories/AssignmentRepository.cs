using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Repositories;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace FoutloosTypen.Core.Data.Repositories
{
    internal class AssignmentRepository : DatabaseConnection
    {
        private readonly List<Assignment> assignments = [];
        public AssignmentRepository()
        {
            try 
            { 
            CreateTable(@"CREATE TABLE IF NOT EXISTS Assignments (
                        [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [TimeLimit] DOUBLE NOT NULL
                )");
            List<string> insertQueries = new()
                {
                    @"INSERT OR IGNORE INTO Courses(Name, Description, Difficulty) VALUES('1, 60.00')",
                };

            InsertMultipleWithTransaction(insertQueries);
            Debug.WriteLine("AssignmentRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AssignmentRepository initialization error: {ex.Message}");
                throw;
            }
        } 
        public List<Assignment> GetAll()
        {
            assignments.Clear();
            try
            {
                string selectQuery = "SELECT Id, TimeLimit FROM Assignments";
                OpenConnection();
                using (SqliteCommand command = new(selectQuery, Connection))
                {
                    SqliteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        double timeLimit = reader.GetDouble(1);
                        assignments.Add(new Assignment(id, timeLimit));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving assignments: {ex.Message}");
                throw;
            }
            finally
            {
                CloseConnection();
            }
            return assignments;
        }
        public Assignment? Get(int id)
        {
            Assignment? assignment = null;
            try
            {
                string selectQuery = "SELECT Id, TimeLimit FROM Assignments WHERE Id = @Id";
                OpenConnection();
                using (SqliteCommand command = new(selectQuery, Connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    SqliteDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        int assignmentId = reader.GetInt32(0);
                        double timeLimit = reader.GetDouble(1);
                        assignment = new Assignment(assignmentId, timeLimit);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving assignment with Id {id}: {ex.Message}");
                throw;
            }
            finally
            {
                CloseConnection();
            }
            return assignment;
        }
    }
}
