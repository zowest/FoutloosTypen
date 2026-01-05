using System;

namespace FoutloosTypen.Core.Helpers
{
    public class EndlessModeRules
    {
        private const int BASE_SCORE = 10;

        public int Combo { get; private set; }
        public int Score { get; private set; }

        private string _previousUserInput = string.Empty;

        public void Reset()
        {
            Combo = 0;
            Score = 0;
            _previousUserInput = string.Empty;
        }

        public EndlessModeRulesResult Process(string targetWord, string typedText)
        {
            targetWord ??= string.Empty;
            typedText ??= string.Empty;

            if (typedText.Length > _previousUserInput.Length)
            {
                int index = typedText.Length - 1;

                if (index < targetWord.Length)
                {
                    if (typedText[index] != targetWord[index])
                    {
                        Combo = 0;
                        _previousUserInput = typedText;

                        return new EndlessModeRulesResult(false, true, -5, 0);
                    }
                }
            }

            if (typedText == targetWord)
            {
                Combo++;

                double timeBonus = GetTimeBonus(Combo);
                int gained = GetScoreGain(Combo);

                Score += gained;
                _previousUserInput = string.Empty;

                return new EndlessModeRulesResult(true, false, timeBonus, gained);
            }

            _previousUserInput = typedText;
            return new EndlessModeRulesResult(false, false, 0, 0);
        }

        public double GetTimeBonus(int combo)
        {
            if (combo >= 10) return 4;
            if (combo >= 5) return 2;
            return 1;
        }

        public int GetScoreGain(int combo)
        {
            double comboMultiplier = 1 + (combo * 0.25);
            return (int)Math.Round(BASE_SCORE * comboMultiplier);
        }

        public (int min, int max) GetAllowedWordLength()
        {
            if (Combo >= 11) return (12, 15);
            if (Combo >= 10) return (11, 13);
            if (Combo >= 9) return (9, 11);
            if (Combo >= 7) return (7, 9);
            if (Combo >= 5) return (5, 7);
            if (Combo >= 3) return (4, 7);
            return (3, 4);
        }
    }

    public class EndlessModeRulesResult
    {
        public bool CorrectWord { get; }
        public bool Mistake { get; }
        public double TimeDelta { get; }
        public int ScoreGained { get; }

        public EndlessModeRulesResult(bool correctWord, bool mistake, double timeDelta, int scoreGained)
        {
            CorrectWord = correctWord;
            Mistake = mistake;
            TimeDelta = timeDelta;
            ScoreGained = scoreGained;
        }
    }
}
