using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IStudentRepository
    {
        Student? Get(string username);
        Student? Get(int id);
        List<Student> GetAll();
        void UpdateTtsMode(int studentId, bool useTtsMode);
        void UpdateStatistics(int studentId, double avgSpeed, double avgPrecision);
        void UpdateProgress(int studentId, int completedLessons, int totalScore);
    }
}
