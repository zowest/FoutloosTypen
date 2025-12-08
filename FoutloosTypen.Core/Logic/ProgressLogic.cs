namespace FoutloosTypen.Core.Logic
{
    public static class ProgressLogic
    {
        public static int CountCorrectCharacters(string typed, string expected)
        {
            if (string.IsNullOrEmpty(typed) || string.IsNullOrEmpty(expected))
                return 0;

            int count = 0;

            for (int i = 0; i < typed.Length && i < expected.Length; i++)
            {
                if (typed[i] == expected[i])
                    count++;
            }

            return count;
        }

        public static double CalculateProgress(int correctCharacters, int totalCharacters)
        {
            if (totalCharacters <= 0)
                return 0;

            return (double)correctCharacters / totalCharacters;
        }
    }
}
