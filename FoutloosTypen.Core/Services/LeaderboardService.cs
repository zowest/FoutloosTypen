using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
   
namespace FoutloosTypen.Core.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ILeaderboardRepository _leaderboardRepository;

        public LeaderboardService(ILeaderboardRepository leaderboardRepository)
        {
            _leaderboardRepository = leaderboardRepository;
        }

        public List<Student> GetTopStudentsBySpeed(int limit = 10)
        {
            return _leaderboardRepository.GetTopStudentsBySpeed(limit);
        }

        public List<Student> GetTopStudentsByPrecision(int limit = 10)
        {
            return _leaderboardRepository.GetTopStudentsByPrecision(limit);
        }

        public List<Student> GetTopStudentsByScore(int limit = 10)
        {
            return _leaderboardRepository.GetTopStudentsByScore(limit);
        }

        public List<Student> GetTopStudentsByCompletedLessons(int limit = 10)
        {
            return _leaderboardRepository.GetTopStudentsByCompletedLessons(limit);
        }

        public int GetStudentRank(int studentId, LeaderboardCategory category)
        {
            return category switch
            {
                LeaderboardCategory.Speed => _leaderboardRepository.GetStudentRankBySpeed(studentId),
                LeaderboardCategory.Precision => _leaderboardRepository.GetStudentRankByPrecision(studentId),
                LeaderboardCategory.Score => _leaderboardRepository.GetStudentRankByScore(studentId),
                LeaderboardCategory.CompletedLessons => _leaderboardRepository.GetStudentRankByCompletedLessons(studentId),
                _ => 0
            };
        }

        public List<Student> GetLeaderboard(LeaderboardCategory category, int limit = 10)
        {
            return category switch
            {
                LeaderboardCategory.Speed => GetTopStudentsBySpeed(limit),
                LeaderboardCategory.Precision => GetTopStudentsByPrecision(limit),
                LeaderboardCategory.Score => GetTopStudentsByScore(limit),
                LeaderboardCategory.CompletedLessons => GetTopStudentsByCompletedLessons(limit),
                _ => new List<Student>()
            };
        }
    }
}
