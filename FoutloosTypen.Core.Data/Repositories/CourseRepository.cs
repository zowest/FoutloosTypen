using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace FoutloosTypen.Core.Data.Repositories
{
    internal class CourseRepository : ICourseRepository
    {
        // Simple in-memory data so the UI can show courses.
        // Replace with real data source as needed.
        private readonly List<Course> _courses = new()
        {
            new Course(1, "Cursus 1", "Beschrijving cursus 1", 1),
            new Course(2, "Cursus 2", "Beschrijving cursus 2", 2),
            new Course(3, "Cursus 3", "Beschrijving cursus 3", 3),
        };

        public List<Course> GetAll()
        {
            return _courses.ToList();
        }

        public Course Get(int id)
        {
            return _courses.FirstOrDefault(c => c.Id == id) ?? new Course();
        }
    }
}