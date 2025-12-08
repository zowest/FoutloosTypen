using NUnit.Framework;
using System.Text.Json;
using FoutloosTypen.Core.Models;

namespace TestCore
{
    [TestFixture]
    public class PracticeMaterialJsonTests
    {
        // UT2-09: JSON → 1 item correct ingeladen
        [Test]
        public void Deserialize_SingleValidEntry_ReturnsOnePracticeMaterial()
        {
            string json = """
            [
                { "Sentence": "Type dit na", "AssignmentId": 5 }
            ]
            """;

            var result = JsonSerializer.Deserialize<List<PracticeMaterial>>(json);

            Assert.That(result, Is.Not.Null);
            Assert.That(result![0].Sentence, Is.EqualTo("Type dit na"));
        }

        // UT2-10: JSON → meerdere items correct ingeladen
        [Test]
        public void Deserialize_MultipleValidEntries_ReturnsAllPracticeMaterials()
        {
            string json = """
            [
                { "Sentence": "Eerste zin", "AssignmentId": 1 },
                { "Sentence": "Tweede zin", "AssignmentId": 2 }
            ]
            """;

            var result = JsonSerializer.Deserialize<List<PracticeMaterial>>(json);

            Assert.That(result, Has.Count.EqualTo(2));
        }

        // UT2-11: Corrupt JSON moet JsonException geven
        [Test]
        public void Deserialize_CorruptJson_ThrowsJsonException()
        {
            string invalidJson = """
            [
                { "Sentence": "Dit is fout, "AssignmentId": 3 }
            ]
            """;

            Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<List<PracticeMaterial>>(invalidJson));
        }

        // UT2-12: AssignmentId ontbreekt → default = 0
        [Test]
        public void Deserialize_MissingAssignmentId_ReturnsDefaultValue()
        {
            string json = """
            [
                { "Sentence": "Alleen een zin" }
            ]
            """;

            var result = JsonSerializer.Deserialize<List<PracticeMaterial>>(json);

            Assert.That(result![0].AssignmentId, Is.EqualTo(0));
        }
    }
}
