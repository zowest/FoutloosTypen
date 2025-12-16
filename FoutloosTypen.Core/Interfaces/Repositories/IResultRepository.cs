using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IResultRepository
    {
        void Save(Result result);

        Result? GetByStudentAndLesson(int studentId, int lessonId);

        List<Result> GetByStudent(int studentId);

        List<Result> GetByLesson(int lessonId);
    }
}
