using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface ILessonProgressService
    {
        void StartLesson(int lessonId);
        void RecordMistake(int lessonId);
        void CompleteSentence(int lessonId);
        void EndLesson(int lessonId);
        LessonProgress? GetProgress(int lessonId);
    }
}