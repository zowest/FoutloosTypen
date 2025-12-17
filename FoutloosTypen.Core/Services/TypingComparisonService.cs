using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Services
{
    public class TypingComparisonService : ITypingComparisonService
    {
        public bool IsCharacterIncorrect(string expectedText, string previousInput, string currentInput)
        {
            // Only check if a character was added (not removed)
            if (currentInput.Length <= previousInput.Length)
                return false;

            // Get the newly typed character index
            int newCharIndex = previousInput.Length;
            
            // Check if we're still within expected text bounds
            if (newCharIndex >= expectedText.Length)
                return true; // Typing beyond expected length is incorrect

            char typedChar = currentInput[newCharIndex];
            char expectedChar = expectedText[newCharIndex];

            return typedChar != expectedChar;
        }

        public TypingComparisonResult Compare(string expectedText, string typedText)
        {
            var result = new TypingComparisonResult
            {
                ExpectedText = expectedText,
                TypedText = typedText
            };

            int maxLength = Math.Max(expectedText.Length, typedText.Length);

            for (int i = 0; i < maxLength; i++)
            {
                char? expected = i < expectedText.Length ? expectedText[i] : null;
                char? typed = i < typedText.Length ? typedText[i] : null;

                result.Characters.Add(new TypingCharacterResult
                {
                    Expected = expected,
                    Typed = typed
                });
            }

            return result;
        }

        public TypingDisplayState CalculateDisplayState(string targetText, string typedText)
        {
            var state = new TypingDisplayState();

            // Handle null or empty cases
            if (string.IsNullOrEmpty(targetText))
            {
                return state;
            }

            typedText ??= string.Empty;
            int typedLength = typedText.Length;

            // Handle empty input - show cursor on first character
            if (typedLength == 0)
            {
                state.CorrectText = string.Empty;
                state.ErrorText = string.Empty;
                state.CursorChar = targetText[0].ToString();
                state.RemainingText = targetText.Length > 1 ? targetText.Substring(1) : string.Empty;
                state.CorrectCharacterCount = 0;
                state.IsComplete = false;
                state.FirstErrorIndex = -1;
                return state;
            }

            // Find first error position
            int firstErrorIndex = FindFirstErrorIndex(targetText, typedText);
            state.FirstErrorIndex = firstErrorIndex;

            // Calculate correct character count
            state.CorrectCharacterCount = CountCorrectCharacters(targetText, typedText);

            // Calculate display parts
            if (firstErrorIndex == -1)
            {
                // No errors - all typed text is correct
                int correctLength = Math.Min(typedLength, targetText.Length);
                state.CorrectText = correctLength > 0 ? targetText.Substring(0, correctLength) : string.Empty;
                state.ErrorText = string.Empty;
            }
            else
            {
                // Has errors - split into correct and error parts
                state.CorrectText = firstErrorIndex > 0 ? targetText.Substring(0, firstErrorIndex) : string.Empty;
                
                // Error text from first error to end of typed (but not beyond target)
                int errorEndIndex = Math.Min(typedLength, targetText.Length);
                int errorLength = errorEndIndex - firstErrorIndex;
                state.ErrorText = errorLength > 0 ? targetText.Substring(firstErrorIndex, errorLength) : string.Empty;
            }

            // Cursor character (next character to type)
            if (typedLength < targetText.Length)
            {
                state.CursorChar = targetText[typedLength].ToString();
            }
            else
            {
                state.CursorChar = string.Empty;
            }

            // Remaining text (everything after cursor)
            int remainingStartIndex = typedLength + 1;
            if (remainingStartIndex < targetText.Length)
            {
                state.RemainingText = targetText.Substring(remainingStartIndex);
            }
            else
            {
                state.RemainingText = string.Empty;
            }

            // Check if complete
            state.IsComplete = typedText == targetText;

            return state;
        }

        public int CountCorrectCharacters(string targetText, string typedText)
        {
            if (string.IsNullOrEmpty(targetText) || string.IsNullOrEmpty(typedText))
                return 0;

            int correctCount = 0;
            int minLength = Math.Min(targetText.Length, typedText.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (typedText[i] == targetText[i])
                    correctCount++;
            }

            return correctCount;
        }

        private int FindFirstErrorIndex(string targetText, string typedText)
        {
            int compareLength = Math.Min(typedText.Length, targetText.Length);

            for (int i = 0; i < compareLength; i++)
            {
                if (typedText[i] != targetText[i])
                {
                    return i;
                }
            }

            // Check if user typed beyond target length
            if (typedText.Length > targetText.Length)
            {
                return targetText.Length;
            }

            return -1; // No errors
        }
    }
}