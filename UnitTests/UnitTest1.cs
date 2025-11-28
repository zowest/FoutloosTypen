using System.Linq;
using Xunit;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Services;

namespace FoutloosTypen.UnitTests.Services
{
    public class TypingComparisonServiceTests
    {
        [Fact]
        public void Compare_PracticeMaterial_WorksCorrectly()
        {
            // Arrange
            var material = new PracticeMaterial(1, "The quick brown fox", 5);
            var typed = "The quick brown fix";
            var service = new TypingComparisonService();

            // Act
            var result = service.Compare(material.Sentences, typed);

            // Assert
            Assert.Equal(material.Sentences.Length, result.Characters.Count);

            // There should be exactly 1 wrong character
            var wrong = result.Characters.Where(c => !c.IsCorrect).ToList();
            Assert.Single(wrong);

            var error = wrong.First();
            Assert.Equal('o', error.Expected);
            Assert.Equal('i', error.Typed);
        }
    }
}