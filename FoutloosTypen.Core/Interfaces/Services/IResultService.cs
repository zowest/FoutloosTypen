using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IResultService
    {
        void StartLesson(int lessonId, int numberOfAssignments);
        void RecordMistake(int lessonId);
        void CompleteSentence(int lessonId, string typedSentence);
        void UpdateCurrentProgress(int lessonId, int charactersTypedSoFar, string currentText);
        void EndLesson(int lessonId);
        void MarkTimerExpired(int lessonId);
        Result? GetProgress(int lessonId);
        void CalculateResults(int lessonId);
    }
}