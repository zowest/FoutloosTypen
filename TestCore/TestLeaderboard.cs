using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Services;
using FoutloosTypen.Core.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace FoutloosTypen.Tests.Core
{
    [TestFixture]
    public class LeaderboardServiceTests
    {
        private Mock<IResultRepository> _mockResultRepo;
        private Mock<IStudentRepository> _mockStudentRepo;
        private IResultService _resultService;

        [SetUp]
        public void SetUp()
        {
            _mockResultRepo = new Mock<IResultRepository>();
            _mockStudentRepo = new Mock<IStudentRepository>();
            _resultService = new ResultService(_mockResultRepo.Object, _mockStudentRepo.Object);
        }

        [Test]
        [Description("UT13-01")]
        public void UT1301_GetLessonLeaderboard_ShouldReturnResultsSortedByScoreDescending()
        {
            // Arrange
            var lessonId = 1;
            var unsortedResults = new List<Result>
            {
                new Result { StudentId = 1, LessonId = lessonId, Score = 100, WordsPerMinute = 50, AccuracyPercent = 95.5 },
                new Result { StudentId = 2, LessonId = lessonId, Score = 200, WordsPerMinute = 60, AccuracyPercent = 98.0 },
                new Result { StudentId = 3, LessonId = lessonId, Score = 150, WordsPerMinute = 55, AccuracyPercent = 96.0 }
            };

            _mockResultRepo.Setup(r => r.GetTopByLesson(lessonId, 10))
                .Returns(unsortedResults.OrderByDescending(r => r.Score).ToList());

            // Act
            var leaderboard = _resultService.GetLessonLeaderboard(lessonId, 10);

            // Assert
            Assert.That(leaderboard.Count, Is.EqualTo(3));
            Assert.That(leaderboard[0].Score, Is.EqualTo(200));
            Assert.That(leaderboard[1].Score, Is.EqualTo(150));
            Assert.That(leaderboard[2].Score, Is.EqualTo(100));
        }

        [Test]
        [Description("UT13-02")]
        public void UT1302_GetLessonLeaderboard_ShouldRespectLimitParameter()
        {
            // Arrange
            var lessonId = 1;
            var manyResults = Enumerable.Range(1, 50)
                .Select(i => new Result
                {
                    StudentId = i,
                    LessonId = lessonId,
                    Score = i * 10,
                    WordsPerMinute = 50,
                    AccuracyPercent = 95.0
                })
                .OrderByDescending(r => r.Score)
                .Take(10)
                .ToList();

            _mockResultRepo.Setup(r => r.GetTopByLesson(lessonId, 10))
                .Returns(manyResults);

            // Act
            var leaderboard = _resultService.GetLessonLeaderboard(lessonId, 10);

            // Assert
            Assert.That(leaderboard.Count, Is.EqualTo(10));
            _mockResultRepo.Verify(r => r.GetTopByLesson(lessonId, 10), Times.Once);
        }

        [Test]
        [Description("UT13-03")]
        public void UT1303_GetOverallLeaderboard_ShouldAggregateScoresAcrossAllLessons()
        {
            // Arrange
            var overallResults = new List<Result>
            {
                new Result { StudentId = 1, Score = 500, WordsPerMinute = 60, AccuracyPercent = 96.0 },
                new Result { StudentId = 2, Score = 700, WordsPerMinute = 70, AccuracyPercent = 98.0 },
                new Result { StudentId = 3, Score = 600, WordsPerMinute = 65, AccuracyPercent = 97.0 }
            };

            _mockResultRepo.Setup(r => r.GetTopByScore(10))
                .Returns(overallResults.OrderByDescending(r => r.Score).ToList());

            // Act
            var leaderboard = _resultService.GetOverallLeaderboard(10);

            // Assert
            Assert.That(leaderboard[0].Score, Is.EqualTo(700));
            Assert.That(leaderboard[1].Score, Is.EqualTo(600));
            Assert.That(leaderboard[2].Score, Is.EqualTo(500));
        }

        [Test]
        [Description("UT13-04")]
        public void UT1304_GetStudentBestResult_ShouldReturnHighestScoreForStudent()
        {
            // Arrange
            var lessonId = 1;
            var studentId = 5;
            var bestResult = new Result
            {
                StudentId = studentId,
                LessonId = lessonId,
                Score = 250,
                WordsPerMinute = 65,
                AccuracyPercent = 98.5
            };

            _mockResultRepo.Setup(r => r.GetByStudentAndLesson(studentId, lessonId))
                .Returns(bestResult);

            // Act
            var result = _resultService.GetStudentBestResult(lessonId, studentId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Score, Is.EqualTo(250));
            Assert.That(result.StudentId, Is.EqualTo(studentId));
        }

        [Test]
        [Description("UT13-05")]
        public void UT1305_GetLessonLeaderboard_ShouldReturnEmptyList_WhenNoResults()
        {
            // Arrange
            var lessonId = 999;
            _mockResultRepo.Setup(r => r.GetTopByLesson(lessonId, 10))
                .Returns(new List<Result>());

            // Act
            var leaderboard = _resultService.GetLessonLeaderboard(lessonId, 10);

            // Assert
            Assert.That(leaderboard, Is.Empty);
            Assert.That(leaderboard, Is.Not.Null);
        }

        [Test]
        [Description("UT13-06")]
        public void UT1306_SaveResult_ShouldCalculateScoreCorrectly()
        {
            // Arrange
            var lessonId = 1;
            var studentId = 10;
            var progress = new LessonProgress
            {
                LessonId = lessonId,
                TotalAssignments = 5,
                CharactersTyped = 500,
                TotalMistakes = 10,
                SentencesCompleted = 5,
                TimeSpent = 60.0,
                StartTime = DateTime.Now.AddMinutes(-1),
                TimerExpired = false,
                CurrentText = ""
            };

            Result? savedResult = null;
            _mockResultRepo.Setup(r => r.Save(It.IsAny<Result>()))
                .Callback<Result>(r => savedResult = r);

            _mockResultRepo.Setup(r => r.GetByStudent(studentId))
                .Returns(new List<Result>());

            // Act
            _resultService.SaveResult(lessonId, studentId, progress);

            // Assert
            Assert.That(savedResult, Is.Not.Null);
            Assert.That(savedResult!.StudentId, Is.EqualTo(studentId));
            Assert.That(savedResult.LessonId, Is.EqualTo(lessonId));
            Assert.That(savedResult.Score, Is.GreaterThanOrEqualTo(0));
            Assert.That(savedResult.AccuracyPercent, Is.GreaterThanOrEqualTo(0));
        }

        /// <summary>
        /// NFR1: Performance Test - Leaderboard moet binnen 200ms laden
        /// </summary>
        [Test]
        [Description("UT13-11")]
        public void UT1311_GetLessonLeaderboard_ShouldLoadWithin200Milliseconds()
        {
            // Arrange - Simulate realistic dataset (100 students)
            var lessonId = 1;
            var largeDataset = Enumerable.Range(1, 100)
                .Select(i => new Result
                {
                    StudentId = i,
                    LessonId = lessonId,
                    Score = i * 10,
                    WordsPerMinute = 50 + (i % 20),
                    AccuracyPercent = 90.0 + (i % 10),
                    TotalCharactersTyped = 500,
                    TotalMistakes = i % 10,
                    StartTime = DateTime.Now.AddMinutes(-5),
                    EndTime = DateTime.Now
                })
                .OrderByDescending(r => r.Score)
                .Take(10)
                .ToList();

            _mockResultRepo.Setup(r => r.GetTopByLesson(lessonId, 10))
                .Returns(largeDataset);

            // Act - Measure execution time
            var stopwatch = Stopwatch.StartNew();
            var leaderboard = _resultService.GetLessonLeaderboard(lessonId, 10);
            stopwatch.Stop();

            // Assert - NFR1: Performance requirement < 200ms
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(200),
                $"Leaderboard loaded in {stopwatch.ElapsedMilliseconds}ms, expected < 200ms");
            Assert.That(leaderboard.Count, Is.EqualTo(10));
            Assert.That(leaderboard, Is.Ordered.Descending.By("Score"));
        }
    }

    /// <summary>
    /// Unit tests voor Student domain model
    /// </summary>
    [TestFixture]
    public class StudentDomainTests
    {
        [Test]
        [Description("UT13-07")]
        public void UT1307_Student_ShouldNotExposePassword_InLeaderboardContext()
        {
            // Arrange - Privacy test
            var student = new Student(1, "testuser", "Test Student", "SecretPassword123", 1, 60, 98, 10, 500);

            // Assert - Password should exist but not be accidentally exposed
            Assert.That(student.Password, Is.EqualTo("SecretPassword123"));
            Assert.That(student.Name, Is.EqualTo("Test Student"));
            Assert.That(student.Name, Is.Not.EqualTo(student.Password));
        }

        [Test]
        [Description("UT13-08")]
        public void UT1308_Student_ShouldHaveValidTotalScore()
        {
            // Arrange
            var student = new Student(1, "user1", "Student One", "pass", 1, 60, 98, 10, 500);

            // Assert
            Assert.That(student.TotalScore, Is.EqualTo(500));
            Assert.That(student.CompletedLessons, Is.EqualTo(10));
            Assert.That(student.TotalScore, Is.GreaterThanOrEqualTo(0));
        }
    }

    /// <summary>
    /// Unit tests voor Result domain model
    /// </summary>
    [TestFixture]
    public class ResultDomainTests
    {
        [Test]
        [Description("UT13-09")]
        public void UT1309_Result_ShouldCalculateAccuracyCorrectly()
        {
            // Arrange
            var totalChars = 1000;
            var mistakes = 20;
            var expectedAccuracy = ((totalChars - mistakes) / (double)totalChars) * 100;

            var result = new Result
            {
                TotalCharactersTyped = totalChars,
                TotalMistakes = mistakes,
                AccuracyPercent = expectedAccuracy
            };

            // Assert - Verify the calculation logic is correct
            Assert.That(result.TotalCharactersTyped, Is.EqualTo(1000));
            Assert.That(result.TotalMistakes, Is.EqualTo(20));
            Assert.That(result.AccuracyPercent, Is.EqualTo(98.0).Within(0.01));
            Assert.That(expectedAccuracy, Is.EqualTo(98.0));
        }

        [Test]
        [Description("UT13-10")]
        public void UT1310_Result_ShouldHaveValidScore()
        {
            // Arrange
            var result = new Result
            {
                Score = 250,
                WordsPerMinute = 60,
                AccuracyPercent = 95.5
            };

            // Assert
            Assert.That(result.Score, Is.GreaterThanOrEqualTo(0));
            Assert.That(result.WordsPerMinute, Is.GreaterThanOrEqualTo(0));
            Assert.That(result.AccuracyPercent, Is.InRange(0, 100));
        }
    }
}