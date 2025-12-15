using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using System;
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

        public void CompleteSentence(int lessonId, int charactersTyped)
        {
            if (_activeLessons.TryGetValue(lessonId, out var progress))
            {
                progress.SentencesCompleted++;
                progress.TotalCharactersTyped += charactersTyped;
            }
        }

        public void EndLesson(int lessonId)
        {
            if (_activeLessons.TryGetValue(lessonId, out var progress))
            {
                progress.EndTime = DateTime.Now;
                CalculateResults(lessonId);
            }
        }

        public void MarkTimerExpired(int lessonId)
        {
            if (_activeLessons.TryGetValue(lessonId, out var progress))
            {
                progress.TimerExpired = true;
            }
        }

        public void CalculateResults(int lessonId)
        {
            if (!_activeLessons.TryGetValue(lessonId, out var progress))
                return;

            // Calculate Accuracy
            if (progress.TotalCharactersTyped > 0)
            {
                progress.AccuracyPercent = ((double)(progress.TotalCharactersTyped - progress.TotalMistakes) 
                    / progress.TotalCharactersTyped) * 100;
                progress.AccuracyPercent = Math.Max(0, progress.AccuracyPercent);
            }
            else
            {
                progress.AccuracyPercent = 0;
            }

            // Calculate Strokes Per Minute (APM)
            if (progress.TimeSpent > 0)
            {
                double minutes = progress.TimeSpent / 60.0;
                progress.StrokesPerMinute = progress.TotalCharactersTyped > 0 
                    ? (int)(progress.TotalCharactersTyped / minutes) 
                    : 0;
            }
            else
            {
                progress.StrokesPerMinute = 0;
            }

            // Calculate Words Per Minute (WPM)
            // Typically 5 characters = 1 word
            progress.WordsPerMinute = progress.StrokesPerMinute / 5;

            // Calculate Score
            if (progress.TimerExpired)
            {
                progress.Score = 0;
            }
            else
            {
                double accuracyScore = progress.AccuracyPercent * 10; // Max 1000 points for 100% accuracy
                double timeBonus = progress.TimeRemaining * 7; // Bonus for time remaining
                progress.Score = (int)((accuracyScore + timeBonus) * 10);
            }
        }

        public LessonProgress? GetProgress(int lessonId)
        {
            return _activeLessons.TryGetValue(lessonId, out var progress) ? progress : null;
        }
    }
}