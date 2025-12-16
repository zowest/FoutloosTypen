using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services;

public interface ITypingComparisonService
{
    TypingSentenceResult Compare(string expectedText, string typedText);
}
