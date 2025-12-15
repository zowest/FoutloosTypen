using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using System.Collections.Generic;

namespace FoutloosTypen.Core.Services
{
    public class LessonProgressService : ILessonProgressService
    {
        private readonly Dictionary<int, LessonProgress> _activeLessons = new();

        public void StartLesson(int lessonId)
        {
            _activeLessons[lessonId] = new LessonProgress
            {
                LessonId = lessonId,
                StartTime = DateTime.Now
            };
        }

        public void RecordMistake(int lessonId)
        {
            if (_activeLessons.TryGetValue(lessonId, out var progress))
            {
                progress.TotalMistakes++;
            }
        }

        public void CompleteSentence(int lessonId)
        {
            if (_activeLessons.TryGetValue(lessonId, out var progress))
            {
                progress.SentencesCompleted++;
            }
        }

        public void EndLesson(int lessonId)
        {
            if (_activeLessons.TryGetValue(lessonId, out var progress))
            {
                progress.EndTime = DateTime.Now;
            }
        }

        public LessonProgress? GetProgress(int lessonId)
        {
            return _activeLessons.TryGetValue(lessonId, out var progress) ? progress : null;
        }
    }
}