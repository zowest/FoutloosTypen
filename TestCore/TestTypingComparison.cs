using FoutloosTypen.Core.Services;
using NUnit.Framework;

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
    }
}
