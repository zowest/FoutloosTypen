using FoutloosTypen.Core.Services;
using NUnit.Framework;

namespace TestCore
{
    [TestFixture]
    public class TypingComparisonTests
    {
        private TypingComparisonService typingComparisonService;

        [SetUp]
        public void SetUp()
        {
            typingComparisonService = new TypingComparisonService();
        }

        [Test]
        public void Compare_sameText_noErrors()
        {
            // Arrange & Act
            var result = typingComparisonService.Compare("hallo", "hallo");

            // Assert
            Assert.That(result.HasErrors, Is.False);
            Assert.That(result.Characters.Count, Is.EqualTo(5));

            foreach (var character in result.Characters)
            {
                Assert.That(character.IsCorrect, Is.True);
            }
        }

        [Test]
        public void Compare_typo_detectedAsError()
        {
            // Arrange & Act
            var result = typingComparisonService.Compare("hallo", "hxllo");

            // Assert
            Assert.That(result.HasErrors, Is.True);
            Assert.That(result.Characters[1].IsCorrect, Is.False);
            Assert.That(result.Characters[1].Expected, Is.EqualTo('a'));
            Assert.That(result.Characters[1].Typed, Is.EqualTo('x'));
        }

        [Test]
        public void Compare_typedShorterThanExpected_detectsMissingCharacters()
        {
            // Arrange & Act
            var result = typingComparisonService.Compare("hallo", "hal");

            // Assert
            Assert.That(result.HasErrors, Is.True);
            Assert.That(result.Characters.Count, Is.EqualTo(5));
            Assert.That(result.Characters[3].Typed, Is.Null);
            Assert.That(result.Characters[3].Expected, Is.EqualTo('l'));
        }

        [Test]
        public void Compare_typedLongerThanExpected_detectsExtraCharacters()
        {
            // Arrange & Act
            var result = typingComparisonService.Compare("hal", "hallo");

            // Assert
            Assert.That(result.HasErrors, Is.True);
            Assert.That(result.Characters.Count, Is.EqualTo(5));
            Assert.That(result.Characters[3].Expected, Is.Null);
            Assert.That(result.Characters[3].Typed, Is.EqualTo('l'));
        }
    }
}
