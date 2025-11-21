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

        }
    }
}
