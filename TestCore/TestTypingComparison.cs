using FoutloosTypen.Core.Services;
using NUnit.Framework;
using System.Diagnostics;

namespace TestCore
{
    [TestFixture]
    public class TypingComparisonTests
    {
        private TypingComparisonService TypingComparisonService;

        [SetUp]
        public void SetUp()
        {
            TypingComparisonService = new TypingComparisonService();
        }

        [Test]
        public void CompareSameTextNoErrors()
        {
            var result = TypingComparisonService.Compare("hallo", "hallo");

            Assert.That(result.HasErrors, Is.False);
            Assert.That(result.Characters.Count, Is.EqualTo(5));

            foreach (var character in result.Characters)
            {
                Assert.That(character.IsCorrect, Is.True);
            }
        }

        [Test]
        public void CompareTypoDetectedAsError()
        {
            var result = TypingComparisonService.Compare("hallo", "hxllo");

            Assert.That(result.HasErrors, Is.True);
            Assert.That(result.Characters[1].IsCorrect, Is.False);
            Assert.That(result.Characters[1].Expected, Is.EqualTo('a'));
            Assert.That(result.Characters[1].Typed, Is.EqualTo('x'));
        }

        [Test]
        public void CompareTypedShorterThanExpectedDetectsMissingCharacters()
        {
            var result = TypingComparisonService.Compare("hallo", "hal");

            Assert.That(result.HasErrors, Is.True);
            Assert.That(result.Characters.Count, Is.EqualTo(5));
            Assert.That(result.Characters[3].Typed, Is.Null);
            Assert.That(result.Characters[3].Expected, Is.EqualTo('l'));
        }

        [Test]
        public void CompareTypedLongerThanExpectedDetectsExtraCharacters()
        {
            var result = TypingComparisonService.Compare("hal", "hallo");

            Assert.That(result.HasErrors, Is.True);
            Assert.That(result.Characters.Count, Is.EqualTo(5));
            Assert.That(result.Characters[3].Expected, Is.Null);
            Assert.That(result.Characters[3].Typed, Is.EqualTo('l'));
        }

        [Test]
        public void CompareCompletesUnder300ms()
        {
            var sw = Stopwatch.StartNew();

            TypingComparisonService.Compare("hallo", "hallo");

            sw.Stop();

            Assert.That(sw.ElapsedMilliseconds, Is.LessThanOrEqualTo(300));
        }

        [Test]
        public void TypingEngineHandlesHighLoad()
        {
            string expected = "testzin";
            int iterations = 600;

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                TypingComparisonService.Compare(expected, expected);
            }

            sw.Stop();

            Assert.That(sw.ElapsedMilliseconds, Is.LessThanOrEqualTo(60000));
        }

        [Test]
        public void ErrorMarkingIsAccurate()
        {
            int total = 10000;
            int failures = 0;

            for (int i = 0; i < total; i++)
            {
                var result = TypingComparisonService.Compare("hallo", "hxllo");

                if (!result.HasErrors)
                    failures++;
            }

            double failureRate = (double)failures / total;

            Assert.That(failureRate, Is.LessThanOrEqualTo(0.01));
        }

        [Test]
        public void ErrorMarkingCompletesUnder150ms()
        {
            var sw = Stopwatch.StartNew();

            TypingComparisonService.Compare("hallo", "hxllo");

            sw.Stop();

            Assert.That(sw.ElapsedMilliseconds, Is.LessThanOrEqualTo(150));
        }
    }
}
