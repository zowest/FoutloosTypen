using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface ITypingComparisonService
    {
        /// <summary>
        /// Detects if a newly typed character is incorrect
        /// </summary>
        bool IsCharacterIncorrect(string expectedText, string previousInput, string currentInput);
        
        /// <summary>
        /// Compares the full typed text with expected text
        /// </summary>
        TypingComparisonResult Compare(string expectedText, string typedText);

        /// <summary>
        /// Calculates the display state for the typing UI
        /// </summary>
        TypingDisplayState CalculateDisplayState(string targetText, string typedText);
    }
}
