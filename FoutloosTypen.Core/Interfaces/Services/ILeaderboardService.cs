using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface ILeaderboardService
    {
        List<Student> GetTopStudentsBySpeed(int limit = 10);
        List<Student> GetTopStudentsByPrecision(int limit = 10);
        List<Student> GetTopStudentsByScore(int limit = 10);
        List<Student> GetTopStudentsByCompletedLessons(int limit = 10);
        int GetStudentRank(int studentId, LeaderboardCategory category);
        List<Student> GetLeaderboard(LeaderboardCategory category, int limit = 10);
    }

    public enum LeaderboardCategory
    {
        Speed,
        Precision,
        Score,
        CompletedLessons
    }
}