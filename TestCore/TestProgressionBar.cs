using NUnit.Framework;
using System.Diagnostics;
using FoutloosTypen.Core.Logic;

namespace TestCore
{
    [TestFixture]
    public class TestProgressBar
    {
        // UT3-01
        [Test]
        public void Progress_IncreasesOnFirstCorrectCharacter()
        {
            int correct = ProgressLogic.CountCorrectCharacters("h", "hallo");
            double progress = ProgressLogic.CalculateProgress(correct, 5);

            Assert.That(correct, Is.EqualTo(1));
            Assert.That(progress, Is.EqualTo(0.2));
        }

        // UT3-02
        [Test]
        public void Progress_UpdatesRealtimeWithMoreCharacters()
        {
            int c1 = ProgressLogic.CountCorrectCharacters("h", "hallo");
            int c2 = ProgressLogic.CountCorrectCharacters("ha", "hallo");

            Assert.That(c2, Is.GreaterThan(c1));
        }

        // UT3-03
        [Test]
        public void Progress_Reaches100Percent()
        {
            int correct = ProgressLogic.CountCorrectCharacters("hallo", "hallo");
            double progress = ProgressLogic.CalculateProgress(correct, 5);

            Assert.That(progress, Is.EqualTo(1.0));
        }

        // UT3-04
        [Test]
        public void Progress_RemainsAccurateWithFastTyping()
        {
            int correct = ProgressLogic.CountCorrectCharacters("hallo", "hallo");
            double progress = ProgressLogic.CalculateProgress(correct, 5);

            Assert.That(progress, Is.EqualTo(1.0));
        }

        // UT3-05
        [Test]
        public void Progress_DoesNotIncreaseOnIncorrectCharacters()
        {
            int correct = ProgressLogic.CountCorrectCharacters("hx", "hallo");

            Assert.That(correct, Is.EqualTo(1));
        }

        // UT3-06
        [Test]
        public void Progress_ContinuesAfterCorrectingError()
        {
            int before = ProgressLogic.CountCorrectCharacters("hx", "hallo");
            int after = ProgressLogic.CountCorrectCharacters("ha", "hallo");

            Assert.That(after, Is.GreaterThan(before));
        }

        // UT3-07
        [Test]
        public void Progress_ResetsToZero()
        {
            double progress = ProgressLogic.CalculateProgress(0, 10);
            Assert.That(progress, Is.EqualTo(0));
        }

        // UT3-08
        [Test]
        public void Progress_ShowsDifferenceBetweenCompletedAndRemaining()
        {
            int correct = ProgressLogic.CountCorrectCharacters("hal", "hallo");
            double progress = ProgressLogic.CalculateProgress(correct, 5);

            Assert.That(progress, Is.GreaterThan(0));
            Assert.That(progress, Is.LessThan(1));
        }

        // UT3-09
        [Test]
        public void Progress_CalculatesUnder100ms()
        {
            var sw = Stopwatch.StartNew();
            ProgressLogic.CalculateProgress(50, 100);
            sw.Stop();

            Assert.That(sw.ElapsedMilliseconds, Is.LessThanOrEqualTo(100));
        }
    }
}
