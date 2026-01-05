using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoutloosTypen.Core.Services
{
    public class ResultService : IResultService
    {
        private readonly IResultRepository _resultRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly Dictionary<int, LessonProgress> _activeProgress = new();

        public ResultService(IResultRepository resultRepository, IStudentRepository studentRepository)
        {
            _resultRepository = resultRepository;
            _studentRepository = studentRepository;
        }

        public void StartLesson(int lessonId, int totalAssignments)
        {
            _activeProgress[lessonId] = new LessonProgress
            {
                LessonId = lessonId,
                TotalAssignments = totalAssignments,
                StartTime = DateTime.Now,
                SentencesCompleted = 0,
                CharactersTyped = 0,
                TotalMistakes = 0,
                TimeSpent = 0,
                TimerExpired = false
            };
        }

        public void CompleteSentence(int lessonId, string sentence)
        {
            if (_activeProgress.TryGetValue(lessonId, out var progress))
            {
                progress.SentencesCompleted++;
                progress.CharactersTyped += sentence.Length;
            }
        }

        public void RecordMistake(int lessonId)
        {
            if (_activeProgress.TryGetValue(lessonId, out var progress))
            {
                progress.TotalMistakes++;
            }
        }

        public void UpdateCurrentProgress(int lessonId, int charactersTyped, string currentText)
        {
            if (_activeProgress.TryGetValue(lessonId, out var progress))
            {
                progress.CurrentText = currentText;
            }
        }

        public void EndLesson(int lessonId)
        {
            if (_activeProgress.TryGetValue(lessonId, out var progress))
            {
                progress.TimeSpent = (DateTime.Now - progress.StartTime).TotalSeconds;
            }
        }

        public void MarkTimerExpired(int lessonId)
        {
            if (_activeProgress.TryGetValue(lessonId, out var progress))
            {
                progress.TimerExpired = true;
            }
        }

        public LessonProgress? GetProgress(int lessonId)
        {
            return _activeProgress.TryGetValue(lessonId, out var progress) ? progress : null;
        }

        public void SaveResult(int lessonId, int studentId, LessonProgress progress)
        {
            var result = new Result
            {
                StudentId = studentId,
                LessonId = lessonId,
                WordsPerMinute = (int)progress.Speed,
                StrokesPerMinute = progress.StrokesPerMinute,
                AccuracyPercent = progress.Accuracy,
                Score = progress.Score,
                TotalMistakes = progress.TotalMistakes,
                SentencesCompleted = progress.SentencesCompleted,
                TotalCharactersTyped = progress.CharactersTyped,
                StartTime = progress.StartTime,
                EndTime = DateTime.Now,
                TimeRemaining = 0, // Calculated if needed
                TimerExpired = progress.TimerExpired
            };

            _resultRepository.Save(result);

            // Update student overall statistics
            UpdateStudentStatistics(studentId);
        }

        public List<Result> GetLessonLeaderboard(int lessonId, int limit = 10)
        {
            return _resultRepository.GetTopByLesson(lessonId, limit);
        }

        public List<Result> GetOverallLeaderboard(int limit = 10)
        {
            return _resultRepository.GetTopByScore(limit);
        }

        public Result? GetStudentBestResult(int lessonId, int studentId)
        {
            return _resultRepository.GetByStudentAndLesson(studentId, lessonId);
        }

        public Result? GetStudentPreviousResult(int lessonId, int studentId)
        {
            var allAttempts = _resultRepository.GetAllAttemptsByStudentAndLesson(studentId, lessonId);
            
            // Return the second most recent (previous best before current attempt)
            return allAttempts.Skip(1).FirstOrDefault();
        }

        public ScoreComparison CompareWithPrevious(int lessonId, int studentId, Result currentResult)
        {
            var previousBest = GetStudentBestResult(lessonId, studentId);
            var allAttempts = _resultRepository.GetAllAttemptsByStudentAndLesson(studentId, lessonId);
            
            var comparison = new ScoreComparison
            {
                PreviousBest = previousBest,
                IsFirstAttempt = allAttempts.Count <= 1
            };

            if (previousBest != null && !comparison.IsFirstAttempt)
            {
                comparison.IsNewPersonalBest = currentResult.Score >= previousBest.Score;
                comparison.ScoreDifference = currentResult.Score - previousBest.Score;
                comparison.SpeedDifference = currentResult.WordsPerMinute - previousBest.WordsPerMinute;
                comparison.AccuracyDifference = currentResult.AccuracyPercent - previousBest.AccuracyPercent;
            }
            else
            {
                comparison.IsNewPersonalBest = true; // First attempt is always "best"
            }

            return comparison;
        }

        private void UpdateStudentStatistics(int studentId)
        {
            var studentResults = _resultRepository.GetByStudent(studentId);
            
            if (!studentResults.Any())
                return;

            double avgSpeed = studentResults.Average(r => r.WordsPerMinute);
            double avgAccuracy = studentResults.Average(r => r.AccuracyPercent);
            int completedLessons = studentResults.Select(r => r.LessonId).Distinct().Count();
            int totalScore = studentResults.Sum(r => r.Score);

            _studentRepository.UpdateStatistics(studentId, avgSpeed, avgAccuracy);
            _studentRepository.UpdateProgress(studentId, completedLessons, totalScore);
        }
    }
}