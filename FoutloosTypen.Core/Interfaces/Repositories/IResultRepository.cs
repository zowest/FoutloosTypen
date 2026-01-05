using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IResultRepository
    {
        void Save(Result result);
        Result? GetByStudentAndLesson(int studentId, int lessonId);
        List<Result> GetByStudent(int studentId);
        List<Result> GetByLesson(int lessonId);

        List<Result> GetTopByLesson(int lessonId, int limit = 10);
        List<Result> GetTopByScore(int limit = 10);

        List<Result> GetAllAttemptsByStudentAndLesson(int studentId, int lessonId);

        void SaveEndlessModeResult(int studentId, int score);
        List<(int StudentId, int BestScore)> GetEndlessModeLeaderboard(int limit = 10);
    }
}