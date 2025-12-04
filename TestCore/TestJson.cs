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

        // ------------------------------------------
        // HAPPY PATH 2 — Multiple valid JSON entries
        // ------------------------------------------
        [Test]
        public void Deserialize_MultipleValidEntries_ReturnsAllPracticeMaterials()
        {
            // Arrange
            string json = """
            [
                { "Sentence": "Eerste zin", "AssignmentId": 1 },
                { "Sentence": "Tweede zin", "AssignmentId": 2 },
                { "Sentence": "Derde zin", "AssignmentId": 3 }
            ]
            """;

            // Act
            var result = JsonSerializer.Deserialize<List<PracticeMaterial>>(json);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result![0].Sentence, Is.EqualTo("Eerste zin"));
            Assert.That(result[1].Sentence, Is.EqualTo("Tweede zin"));
            Assert.That(result[2].Sentence, Is.EqualTo("Derde zin"));
        }

        // ------------------------------------------
        // UNHAPPY PATH — Corrupt JSON throws exception
        // ------------------------------------------
        [Test]
        public void Deserialize_CorruptJson_ThrowsJsonException()
        {
            // Arrange - Missing closing quote causes invalid JSON
            string invalidJson = """
            [
                { "Sentence": "Dit is fout, "AssignmentId": 3 }
            ]
            """;

            // Act & Assert
            Assert.Throws<JsonException>(() =>
            {
                JsonSerializer.Deserialize<List<PracticeMaterial>>(invalidJson);
            });
        }
    }
}
