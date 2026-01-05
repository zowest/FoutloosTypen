using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface ILeaderboardRepository
    {
        List<Student> GetTopStudentsBySpeed(int limit = 10);
        List<Student> GetTopStudentsByPrecision(int limit = 10);
        List<Student> GetTopStudentsByScore(int limit = 10);
        List<Student> GetTopStudentsByCompletedLessons(int limit = 10);
        int GetStudentRankBySpeed(int studentId);
        int GetStudentRankByPrecision(int studentId);
        int GetStudentRankByScore(int studentId);
        int GetStudentRankByCompletedLessons(int studentId);
    }
}
