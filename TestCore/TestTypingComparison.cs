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

        // UT2-01: Correcte invoer bevat geen fouten
        [Test]
        public void CompareSameTextNoErrors()
        {
            var result = TypingComparisonService.Compare("hallo", "hallo");

            Assert.That(result.HasErrors, Is.False);
            Assert.That(result.Characters.Count, Is.EqualTo(5));

            foreach (var character in result.Characters)
                Assert.That(character.IsCorrect, Is.True);
        }

        // UT2-02: Typfout wordt gedetecteerd
        [Test]
        public void CompareTypoDetectedAsError()
        {
            var result = TypingComparisonService.Compare("hallo", "hxllo");

            Assert.That(result.HasErrors, Is.True);
            Assert.That(result.Characters[1].IsCorrect, Is.False);
        }

        // UT2-03: Ontbrekende tekens worden gedetecteerd
        [Test]
        public void CompareTypedShorterThanExpectedDetectsMissingCharacters()
        {
            var result = TypingComparisonService.Compare("hallo", "hal");

            Assert.That(result.Characters.Count, Is.EqualTo(5));
            Assert.That(result.Characters[3].Expected, Is.EqualTo('l'));
            Assert.That(result.Characters[3].Typed, Is.Null);
        }

        // UT2-04: Extra tekens worden gedetecteerd
        [Test]
        public void CompareTypedLongerThanExpectedDetectsExtraCharacters()
        {
            var result = TypingComparisonService.Compare("hal", "hallo");

            Assert.That(result.Characters.Count, Is.EqualTo(5));
            Assert.That(result.Characters[3].Expected, Is.Null);
            Assert.That(result.Characters[3].Typed, Is.EqualTo('l'));
        }

        // UT2-05: Vergelijking onder 300ms
        [Test]
        public void CompareCompletesUnder300ms()
        {
            var sw = Stopwatch.StartNew();

            TypingComparisonService.Compare("hallo", "hallo");

            sw.Stop();
            Assert.That(sw.ElapsedMilliseconds, Is.LessThanOrEqualTo(300));
        }

        // UT2-06: Engine kan 100WPM simulatie aan
        [Test]
        public void TypingEngineHandlesHighLoad()
        {
            int iterations = 600;
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
                TypingComparisonService.Compare("testzin", "testzin");

            sw.Stop();
            Assert.That(sw.ElapsedMilliseconds, Is.LessThanOrEqualTo(60000));
        }

        // UT2-07: Foutmarkering moet in minstens 99% kloppen
        [Test]
        public void ErrorMarkingIsAccurate()
        {
            int total = 10000;
            int failures = 0;

            for (int i = 0; i < total; i++)
            {
                var r = TypingComparisonService.Compare("hallo", "hxllo");
                if (!r.HasErrors) failures++;
            }

            double failureRate = failures / (double)total;
            Assert.That(failureRate, Is.LessThanOrEqualTo(0.01));
        }

        // UT2-08: Foutmarkering onder 150ms
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
