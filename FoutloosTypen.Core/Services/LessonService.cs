using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Services;

namespace FoutloosTypen.Core.Services
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        public LessonService(ILessonRepository lessonRepository, IAssignmentRepository assignmentRepository)
        {
            _lessonRepository = lessonRepository;
            _assignmentRepository = assignmentRepository;
        }
        public IEnumerable<Lesson> GetAll()
        {
            var lessons = _lessonRepository.GetAll();
            foreach (var lesson in lessons)
            {
                lesson.TotalTime = CalculateTotalTime(lesson.Id);
            }
            return lessons;
        }
        public Lesson Get(int id)
        {
            try
            {
                var lesson = _lessonRepository.Get(id);

                lesson.TotalTime = CalculateTotalTime(lesson.Id);
                return lesson;
            }
            catch (Exception)
            {
                throw new KeyNotFoundException($"Lesson with id {id} not found.");

            }
        }
        public double CalculateTotalTime(int lessonId)
        {
            var assignments = _assignmentRepository.GetAll().Where(a => a.LessonId == lessonId);
            return assignments.Sum(a => a.TimeLimit);
        }
    }
}