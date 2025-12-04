using NUnit.Framework;
using System.Text.Json;
using FoutloosTypen.Core.Models;

namespace TestCore
{
    [TestFixture]
    public class PracticeMaterialJsonTests
    {
        // ------------------------------------------
        // HAPPY PATH 1 — Single valid JSON entry
        // ------------------------------------------
        [Test]
        public void Deserialize_SingleValidEntry_ReturnsOnePracticeMaterial()
        {
            // Arrange
            string json = """
            [
                { "Sentence": "Type dit na", "AssignmentId": 5 }
            ]
            """;

            // Act
            var result = JsonSerializer.Deserialize<List<PracticeMaterial>>(json);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result![0].Sentence, Is.EqualTo("Type dit na"));
            Assert.That(result[0].AssignmentId, Is.EqualTo(5));
        }
    }
}
