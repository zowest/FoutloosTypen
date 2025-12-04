using FoutloosTypen.Core.Services;
using NUnit.Framework;

namespace TestCore
{
    [TestFixture]
    public class TestTypingComparison
    {
        private TypingComparisonService _service;

        [SetUp]
        public void Setup()
        {
            _service = new TypingComparisonService();
        }

        [Test]
        public void Compare_GelijkeZinnen_GeenFouten()
        {
            var result = _service.Compare("hallo", "hallo");

            Assert.That(result.HasErrors, Is.False);
            Assert.That(result.Characters.Count, Is.EqualTo(5));

            foreach (var character in result.Characters)
            {
                Assert.That(character.IsCorrect, Is.True);
            }
        }

        [Test]
        public void Compare_Typfout_WordtGedetecteerd()
        {
            var result = _service.Compare("hallo", "hxllo");

            Assert.That(result.HasErrors, Is.True);
            Assert.That(result.Characters[1].IsCorrect, Is.False);
            Assert.That(result.Characters[1].Expected, Is.EqualTo('a'));
            Assert.That(result.Characters[1].Typed, Is.EqualTo('x'));
        }

        [Test]
        public void Compare_TypedKorterDanExpected_DetecteertFouten()
        {
            var result = _service.Compare("hallo", "hal");

            Assert.That(result.HasErrors, Is.True);
            Assert.That(result.Characters.Count, Is.EqualTo(5));

            Assert.That(result.Characters[3].Typed, Is.Null);
            Assert.That(result.Characters[3].Expected, Is.EqualTo('l'));
        }

        [Test]
        public void Compare_TypedLangerDanExpected_DetecteertExtraTekens()
        {
            var result = _service.Compare("hal", "hallo");

            Assert.That(result.HasErrors, Is.True);
            Assert.That(result.Characters.Count, Is.EqualTo(5));

            Assert.That(result.Characters[3].Expected, Is.Null);
            Assert.That(result.Characters[3].Typed, Is.EqualTo('l'));
        }
    }
}
