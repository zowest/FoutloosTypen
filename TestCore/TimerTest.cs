using System;
using System.Threading;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Services;
using NUnit.Framework;

namespace TestCore
{
    [TestFixture]
    public class TimerTest
    {
        private ITimerService _timerService;

        [SetUp]
        public void SetUp()
        {
            _timerService = new TimerService();
        }

        [TearDown]
        public void TearDown()
        {
            _timerService.Stop();
        }

        // UT4-01
        [Test]
        public void Initialize_SetsTimeRemaining()
        {
            _timerService.Initialize(60);
            Assert.That(_timerService.TimeRemaining, Is.EqualTo(60));
        }

        // UT4-02
        [Test]
        public void Initialize_FormatsTimeCorrectly()
        {
            _timerService.Initialize(125);
            Assert.That(_timerService.TimeRemainingFormatted, Is.EqualTo("02:05"));
        }

        // UT4-03
        [Test]
        public void Initialize_WithZero_SetsTimeToZero()
        {
            _timerService.Initialize(0);
            Assert.That(_timerService.TimeRemaining, Is.EqualTo(0));
            Assert.That(_timerService.TimeRemainingFormatted, Is.EqualTo("00:00"));
        }

        // UT4-04
        [Test]
        public void Start_ChangesIsRunningToTrue()
        {
            _timerService.Initialize(60);
            _timerService.Start();
            Assert.That(_timerService.IsRunning, Is.True);
        }

        // UT4-05
        [Test]
        public void Start_WhenAlreadyRunning_DoesNotStartAgain()
        {
            _timerService.Initialize(60);
            _timerService.Start();
            var initial = _timerService.IsRunning;

            _timerService.Start();

            Assert.That(_timerService.IsRunning, Is.EqualTo(initial));
        }

        // UT4-06
        [Test]
        public void Stop_ChangesIsRunningToFalse()
        {
            _timerService.Initialize(60);
            _timerService.Start();
            _timerService.Stop();
            Assert.That(_timerService.IsRunning, Is.False);
        }

        // UT4-07
        [Test]
        public void Stop_WhenNotRunning_DoesNotThrow()
        {
            _timerService.Initialize(60);
            Assert.DoesNotThrow(() => _timerService.Stop());
        }

        // UT4-08
        [Test]
        public void Restart_ResetsTimeToInitialValue()
        {
            _timerService.Initialize(60);
            _timerService.Start();
            _timerService.Restart();
            _timerService.Stop();

            Assert.That(_timerService.TimeRemaining, Is.EqualTo(60));
        }

        // UT4-09
        [Test]
        public void Restart_StartsTimerAgain()
        {
            _timerService.Initialize(60);
            _timerService.Restart();
            Assert.That(_timerService.IsRunning, Is.True);
        }

        // UT4-10
        [Test]
        public void TimeRemainingFormatted_ShowsCorrectMinutesAndSeconds()
        {
            _timerService.Initialize(90);
            Assert.That(_timerService.TimeRemainingFormatted, Is.EqualTo("01:30"));
        }

        // UT4-11
        [Test]
        public void TimeRemainingFormatted_PadsWithZeros()
        {
            _timerService.Initialize(5);
            Assert.That(_timerService.TimeRemainingFormatted, Is.EqualTo("00:05"));
        }

        // UT4-12
        [Test]
        public void MultipleStartStop_MaintainsCorrectState()
        {
            _timerService.Initialize(60);

            _timerService.Start();
            Assert.That(_timerService.IsRunning, Is.True);

            _timerService.Stop();
            Assert.That(_timerService.IsRunning, Is.False);

            _timerService.Start();
            Assert.That(_timerService.IsRunning, Is.True);

            _timerService.Stop();
            Assert.That(_timerService.IsRunning, Is.False);
        }

        // UT4-13
        [Test]
        public void Initialize_WithLargeValue_FormatsCorrectly()
        {
            _timerService.Initialize(3661);
            Assert.That(_timerService.TimeRemainingFormatted, Is.EqualTo("60:00"));
        }
    }
}
