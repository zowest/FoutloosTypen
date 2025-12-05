using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [Test]
        public void Initialize_SetsTimeRemaining()
        {
            _timerService.Initialize(60);

            Assert.That(_timerService.TimeRemaining, Is.EqualTo(60));
        }

        [Test]
        public void Initialize_FormatsTimeCorrectly()
        {
            _timerService.Initialize(125);

            Assert.That(_timerService.TimeRemainingFormatted, Is.EqualTo("02:05"));
        }

        [Test]
        public void Initialize_WithZero_SetsTimeToZero()
        {
            _timerService.Initialize(0);

            Assert.That(_timerService.TimeRemaining, Is.EqualTo(0));
            Assert.That(_timerService.TimeRemainingFormatted, Is.EqualTo("00:00"));
        }

        [Test]
        public void Start_ChangesIsRunningToTrue()
        {
            _timerService.Initialize(60);

            _timerService.Start();

            Assert.That(_timerService.IsRunning, Is.True);
        }

        [Test]
        public void Start_WhenAlreadyRunning_DoesNotStartAgain()
        {
            _timerService.Initialize(60);
            _timerService.Start();
            var firstRunningState = _timerService.IsRunning;

            _timerService.Start();

            Assert.That(_timerService.IsRunning, Is.EqualTo(firstRunningState));
        }

        [Test]
        public void Stop_ChangesIsRunningToFalse()
        {
            _timerService.Initialize(60);
            _timerService.Start();

            _timerService.Stop();

            Assert.That(_timerService.IsRunning, Is.False);
        }

        [Test]
        public void Stop_WhenNotRunning_DoesNotThrow()
        {
            _timerService.Initialize(60);

            Assert.DoesNotThrow(() => _timerService.Stop());
        }

        [Test]
        [Ignore("MAUI application needs to run in order for ticks to occur, keeping incase a fix or workaround is found")] //works for the wrong reasons
        public void Stop_PreservesTimeRemaining()
        {
            _timerService.Initialize(60);
            _timerService.Start();
            Thread.Sleep(100);

            _timerService.Stop();
            var stoppedTime = _timerService.TimeRemaining;

            Assert.That(stoppedTime, Is.LessThanOrEqualTo(60));
        }

        [Test]
        public void Restart_ResetsTimeToInitialValue()
        {
            _timerService.Initialize(60);
            _timerService.Start();
            _timerService.Restart();
            _timerService.Stop();

            Assert.That(_timerService.TimeRemaining, Is.EqualTo(60));
        }

        [Test]
        public void Restart_StartsTimerAgain()
        {
            _timerService.Initialize(60);

            _timerService.Restart();

            Assert.That(_timerService.IsRunning, Is.True);
        }

        [Test]
        [Ignore("MAUI application needs to run in order for ticks to occur, keeping incase a fix or workaround is found")]
        public async Task Timer_DecrementsTimeRemaining()
        {
            _timerService.Initialize(5);
            var initialTime = _timerService.TimeRemaining;

            _timerService.Start();
            await Task.Delay(2200);
            _timerService.Stop();

            Assert.That(_timerService.TimeRemaining, Is.LessThan(initialTime));
        }

        [Test]
        [Ignore("MAUI application needs to run in order for ticks to occur, keeping incase a fix or workaround is found")] //works for the wrong reasons
        public async Task Timer_FormattingUpdatesCorrectly()
        {
            _timerService.Initialize(61);
            Assert.That(_timerService.TimeRemainingFormatted, Is.EqualTo("01:01"));

            _timerService.Start();
            await Task.Delay(1200);
            _timerService.Stop();

            Assert.That(_timerService.TimeRemainingFormatted, Does.Match(@"\d{2}:\d{2}"));
        }

        [Test]
        [Ignore("MAUI application needs to run in order for ticks to occur, keeping incase a fix or workaround is found")]
        public async Task Timer_StopsAtZero()
        {
            _timerService.Initialize(2);

            _timerService.Start();
            await Task.Delay(3000);

            Assert.That(_timerService.TimeRemaining, Is.EqualTo(0));
            Assert.That(_timerService.IsRunning, Is.False);
        }

        [Test]
        public void TimeRemainingFormatted_ShowsCorrectMinutesAndSeconds()
        {
            _timerService.Initialize(90);

            Assert.That(_timerService.TimeRemainingFormatted, Is.EqualTo("01:30"));
        }

        [Test]
        public void TimeRemainingFormatted_PadsWithZeros()
        {
            _timerService.Initialize(5);

            Assert.That(_timerService.TimeRemainingFormatted, Is.EqualTo("00:05"));
        }

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

        [Test]
        public void Initialize_WithLargeValue_FormatsCorrectly()
        {
            _timerService.Initialize(3661);

            Assert.That(_timerService.TimeRemainingFormatted, Is.EqualTo("60:00"));
        }

        [Test]
        public void TimeRemainingFormatted_HandlesHoursCorrectly()
        {
            _timerService.Initialize(3600);
            
            Assert.That(_timerService.TimeRemainingFormatted, Is.EqualTo("60:00"));  //not sure whether or not to keep these 3, maybe useful for endless mode testing later on
        }

        [Test]
        public void TimeRemainingFormatted_ShowsOnlyMinutesAndSeconds()
        {
            _timerService.Initialize(7325);
            
            Assert.That(_timerService.TimeRemainingFormatted, Is.EqualTo("60:00"));
        }
    }
}
