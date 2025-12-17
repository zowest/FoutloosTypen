using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FoutloosTypen.Core.Services
{
    public class ResultService : IResultService
    {
        private readonly Dictionary<int, Result> _activeLessons = new();
        private const int TIME_PER_ASSIGNMENT = 60; // 60 seconds per assignment

        public void StartLesson(int lessonId, int numberOfAssignments)
        {
            _activeLessons[lessonId] = new Result
            {
                LessonId = lessonId,
                StartTime = DateTime.Now,
                ExpectedTime = numberOfAssignments * TIME_PER_ASSIGNMENT
            };
            Debug.WriteLine($"Lesson {lessonId} started at {DateTime.Now} with {numberOfAssignments} assignments (Total time: {numberOfAssignments * TIME_PER_ASSIGNMENT} seconds)");
        }

        public void RecordMistake(int lessonId)
        {
            if (_activeLessons.TryGetValue(lessonId, out var progress))
            {
                progress.TotalMistakes++;
            }
        }

        public void CompleteSentence(int lessonId, string typedSentence)
        {
            if (_activeLessons.TryGetValue(lessonId, out var progress))
            {
                progress.SentencesCompleted++;
                progress.TotalCharactersTyped += typedSentence.Length;
                progress.CompletedSentences.Add(typedSentence);
                Debug.WriteLine($"Sentence completed. Total chars: {progress.TotalCharactersTyped}");
            }
        }

        // New method to track current typing progress
        public void UpdateCurrentProgress(int lessonId, int charactersTypedSoFar, string currentText)
        {
            if (_activeLessons.TryGetValue(lessonId, out var progress))
            {
                // Store current incomplete text
                progress.CurrentIncompleteText = currentText ?? string.Empty;
                
                // Store the maximum characters typed (including incomplete sentences)
                int totalIncludingCurrent = progress.CompletedSentences.Sum(s => s.Length) + charactersTypedSoFar;
                if (totalIncludingCurrent > progress.TotalCharactersTyped)
                {
                    progress.TotalCharactersTyped = totalIncludingCurrent;
                }
            }
        }

        public void EndLesson(int lessonId)
        {
            if (_activeLessons.TryGetValue(lessonId, out var progress))
            {
                progress.EndTime = DateTime.Now;
                Debug.WriteLine($"Lesson ended. TimeSpent: {progress.TimeSpent} seconds, Chars: {progress.TotalCharactersTyped}");
                CalculateResults(lessonId);
            }
        }

        public void MarkTimerExpired(int lessonId)
        {
            if (_activeLessons.TryGetValue(lessonId, out var progress))
            {
                progress.TimerExpired = true;
                Debug.WriteLine($"Timer expired marked for lesson {lessonId}");
            }
        }

        public void CalculateResults(int lessonId)
        {
            if (!_activeLessons.TryGetValue(lessonId, out var progress))
                return;
            
            Debug.WriteLine($"=== Calculating Results ===");
            Debug.WriteLine($"Characters: {progress.TotalCharactersTyped}");
            Debug.WriteLine($"Mistakes: {progress.TotalMistakes}");
            Debug.WriteLine($"Time Spent: {progress.TimeSpent}");
            
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
            Debug.WriteLine($"Accuracy: {progress.AccuracyPercent}%");

            // Calculate Strokes Per Minute (APM)
            if (progress.TimeSpent > 0)
            {
                double minutes = progress.TimeSpent / 60.0;
                progress.StrokesPerMinute = (int)(progress.TotalCharactersTyped / minutes);
                Debug.WriteLine($"Total minutes: {minutes}, APM: {progress.StrokesPerMinute}");
            }
            else
            {
                progress.StrokesPerMinute = 0;
            }

            // Calculate Words Per Minute (WPM)
            // Combine all completed sentences + current incomplete text
            List<string> allText = new List<string>(progress.CompletedSentences);
            if (!string.IsNullOrWhiteSpace(progress.CurrentIncompleteText))
            {
                allText.Add(progress.CurrentIncompleteText);
            }

            string allTypedText = string.Join(" ", allText);

            Debug.WriteLine($"All typed text: '{allTypedText}' (Length: {allTypedText.Length})");

            int totalWords = 0;
            if (!string.IsNullOrWhiteSpace(allTypedText))
            {
                var words = allTypedText.Split(new[] { ' ', '\t', '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries);
                totalWords = words.Length;
            }

            Debug.WriteLine($"Total words typed: {totalWords}");

            if (progress.TimeSpent > 0)
            {
                double minutes = progress.TimeSpent / 60.0;
                progress.WordsPerMinute = (int)(totalWords / minutes);
                Debug.WriteLine($"WPM: {progress.WordsPerMinute}");
            }
            else
            {
                progress.WordsPerMinute = 0;
            }

            // Calculate Score
            if (progress.TimerExpired)
            {
                progress.Score = 0;
                Debug.WriteLine("Timer expired - Score = 0");
            }
            else
            {
                double accuracyScore = progress.AccuracyPercent * 10;
                double timeBonus = progress.TimeRemaining * 7;
                progress.Score = (int)((accuracyScore + timeBonus) * 10);
                Debug.WriteLine($"Score: {progress.Score}");
            }
            progress.TimeRemaining = Math.Max(0, progress.ExpectedTime - progress.TimeSpent);
            Debug.WriteLine("=========================");
        }

        public Result? GetProgress(int lessonId)
        {
            return _activeLessons.TryGetValue(lessonId, out var progress) ? progress : null;
        }
    }
}