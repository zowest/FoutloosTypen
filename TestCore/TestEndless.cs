using FoutloosTypen.Core.Helpers;
using NUnit.Framework;

namespace TestCore
{
    public class TestEndless
    {
        private EndlessModeHelper Rules;

        [SetUp]
        public void Setup()
        {
            Rules = new EndlessModeHelper();
            Rules.Reset();
        }

        // UT18-01: Correct woord verhoogt combo
        [Test]
        public void CorrectWordIncreasesCombo()
        {
            Rules.Process("abc", "abc");

            Assert.That(Rules.Combo, Is.EqualTo(1));
        }

        // UT18-02: Meerdere correcte woorden verhogen combo verder
        [Test]
        public void MultipleCorrectWordsIncreaseCombo()
        {
            Rules.Process("abc", "abc");
            Rules.Process("abc", "abc");

            Assert.That(Rules.Combo, Is.EqualTo(2));
        }

        // UT18-03: Correct woord verhoogt score
        [Test]
        public void CorrectWordIncreasesScore()
        {
            Rules.Process("abc", "abc");

            Assert.That(Rules.Score, Is.GreaterThan(0));
        }

        // UT18-04: Hogere combo geeft hogere score-toename
        [Test]
        public void HigherComboGivesHigherScoreGain()
        {
            Rules.Process("abc", "abc");
            int scoreAfter1 = Rules.Score;

            Rules.Process("abc", "abc");
            int scoreAfter2 = Rules.Score;

            int gained1 = scoreAfter1;
            int gained2 = scoreAfter2 - scoreAfter1;

            Assert.That(gained2, Is.GreaterThan(gained1));
        }

        // UT18-05: Fout reset combo
        [Test]
        public void MistakeResetsCombo()
        {
            Rules.Process("abc", "abc");
            Assert.That(Rules.Combo, Is.EqualTo(1));

            Rules.Process("abc", "x");

            Assert.That(Rules.Combo, Is.EqualTo(0));
        }

        // UT18-06: Fout trekt 5 seconden van de timer af
        [Test]
        public void MistakeSubtractsFiveSeconds()
        {
            var result = Rules.Process("abc", "x");

            Assert.That(result.TimeDelta, Is.EqualTo(-5));
        }

        // UT18-07: Combo lager dan 5 geeft 1 seconde tijdsbonus
        [Test]
        public void ComboBelowFiveAddsOneSecond()
        {
            var result = Rules.Process("abc", "abc");

            Assert.That(result.TimeDelta, Is.EqualTo(1));
        }

        // UT18-08: Combo 5 tot en met 9 geeft 2 seconden tijdsbonus
        [Test]
        public void ComboFiveToNineAddsTwoSeconds()
        {
            for (int i = 0; i < 5; i++)
                Rules.Process("abc", "abc");

            var result = Rules.Process("abc", "abc");

            Assert.That(result.TimeDelta, Is.EqualTo(2));
        }

        // UT18-09: Combo 10 of hoger geeft 4 seconden tijdsbonus
        [Test]
        public void ComboTenOrHigherAddsFourSeconds()
        {
            for (int i = 0; i < 10; i++)
                Rules.Process("abc", "abc");

            var result = Rules.Process("abc", "abc");

            Assert.That(result.TimeDelta, Is.EqualTo(4));
        }

        // UT18-10: Reset zet score en combo terug naar nul
        [Test]
        public void ResetResetsScoreAndCombo()
        {
            Rules.Process("abc", "abc");
            Assert.That(Rules.Score, Is.GreaterThan(0));
            Assert.That(Rules.Combo, Is.EqualTo(1));

            Rules.Reset();

            Assert.That(Rules.Score, Is.EqualTo(0));
            Assert.That(Rules.Combo, Is.EqualTo(0));
        }

        // UT18-11: Woordlengtebereik verandert bij hogere combo
        [Test]
        public void HigherComboChangesAllowedWordLengthRange()
        {
            var lowRange = Rules.GetAllowedWordLength();
            Assert.That(lowRange.min, Is.EqualTo(3));
            Assert.That(lowRange.max, Is.EqualTo(4));

            for (int i = 0; i < 11; i++)
                Rules.Process("abc", "abc");

            var highRange = Rules.GetAllowedWordLength();
            Assert.That(highRange.min, Is.EqualTo(12));
            Assert.That(highRange.max, Is.EqualTo(15));
        }

        // UT18-12: Lege of null invoer veroorzaakt geen exception
        [Test]
        public void NullOrEmptyInputDoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                Rules.Process(null, null);
                Rules.Process("", "");
            });
        }
    }
}
