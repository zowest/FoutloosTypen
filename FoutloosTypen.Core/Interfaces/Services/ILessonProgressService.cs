using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface ILessonProgressService
    {
        void StartLesson(int lessonId);
        void RecordMistake(int lessonId);
        void CompleteSentence(int lessonId, int charactersTyped);
        void EndLesson(int lessonId);
        void MarkTimerExpired(int lessonId);
        LessonProgress? GetProgress(int lessonId);
        void CalculateResults(int lessonId);
    }
}