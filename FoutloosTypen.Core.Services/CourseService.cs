using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using System.Collections.Generic;

namespace FoutloosTypen.Core.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public List<Course> GetAll()
        {
            return _courseRepository.GetAll();
        }

        public Course? Get(int id)
        {
            return _courseRepository.Get(id);
        }
    }
}