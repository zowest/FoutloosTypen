using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Services
{
    public class TypingComparisonService : ITypingComparisonService
    {
        private readonly TypingDisplayState _cachedState = new();

        public bool IsCharacterIncorrect(string expectedText, string previousInput, string currentInput)
        {
            if (currentInput.Length <= previousInput.Length)
                return false;

            int newCharIndex = previousInput.Length;
            
            if (newCharIndex >= expectedText.Length)
                return true;

            return currentInput[newCharIndex] != expectedText[newCharIndex];
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

        public TypingDisplayState CalculateDisplayState(string expectedText, string typedText)
        {
            int typedLength = typedText.Length;
            int expectedLength = expectedText.Length;

            // Vind het eerste foutieve karakter via directe index-toegang
            int correctCount = 0;
            int minLength = Math.Min(typedLength, expectedLength);

            for (int i = 0; i < minLength; i++)
            {
                if (typedText[i] != expectedText[i])
                    break;
                correctCount++;
            }

            // Gebruik Span<char> voor substring-operaties (zero-allocation waar mogelijk)
            ReadOnlySpan<char> expectedSpan = expectedText.AsSpan();

            // Hergebruik cached state object
            _cachedState.CorrectCharacterCount = correctCount;
            _cachedState.IsComplete = correctCount == expectedLength && typedLength == expectedLength;

            // Correct text: alles tot eerste fout
            _cachedState.CorrectText = correctCount > 0 
                ? expectedSpan[..correctCount].ToString() 
                : string.Empty;

            // Error text: verkeerd getypte karakters
            if (typedLength > correctCount && correctCount < expectedLength)
            {
                int errorEnd = Math.Min(typedLength, expectedLength);
                _cachedState.ErrorText = expectedSpan[correctCount..errorEnd].ToString();
            }
            else
            {
                _cachedState.ErrorText = string.Empty;
            }

            // Cursor: volgende te typen karakter
            int cursorPos = Math.Max(correctCount, typedLength);
            _cachedState.CursorChar = cursorPos < expectedLength 
                ? expectedSpan[cursorPos].ToString() 
                : string.Empty;

            // Remaining: rest van de tekst na cursor
            int remainingStart = cursorPos + 1;
            _cachedState.RemainingText = remainingStart < expectedLength 
                ? expectedSpan[remainingStart..].ToString() 
                : string.Empty;

            return _cachedState;
        }
    }
}