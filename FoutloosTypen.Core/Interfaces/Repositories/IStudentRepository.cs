using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IStudentRepository
    {
        Student? Get(string username);
        Student? Get(int id);
        List<Student> GetAll();
        void UpdateStatistics(int studentId, double avgSpeed, double avgPrecision);
        void UpdateProgress(int studentId, int completedLessons, int totalScore);
    }
}
