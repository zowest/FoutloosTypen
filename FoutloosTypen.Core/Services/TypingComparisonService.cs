using FoutloosTypen.Core.Interfaces.Repositories;
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
    }
}