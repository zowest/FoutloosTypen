using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Services;

public class TypingComparisonService : ITypingComparisonService
{
    public TypingSentenceResult Compare(string expectedText, string typedText)
    {
        var result = new TypingSentenceResult
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