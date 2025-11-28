using System.Linq;
using NUnit.Framework;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Services;

namespace FoutloosTypen.UnitTests
{
    public class TypingComparisonServiceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Compare_PracticeMaterial_WorksCorrectly()
        {
            // Arrange
            var material = new PracticeMaterial(1, "The quick brown fox", 5);
            var typed = "The quick brown fix";
            var service = new TypingComparisonService();

            // Act
            var result = service.Compare(material.Sentences, typed);

            // Assert
            Assert.That(result.Characters.Count, Is.EqualTo(material.Sentences.Length));

            // There should be exactly 1 wrong character
            var wrong = result.Characters.Where(c => !c.IsCorrect).ToList();
            Assert.That(wrong.Count, Is.EqualTo(1));

            var error = wrong.First();
            Assert.That(error.Expected, Is.EqualTo('o'));
            Assert.That(error.Typed, Is.EqualTo('i'));
        }
    }
}
