namespace FoutloosTypen.Core.Models;

public class TypingSentenceResult
{
    public string ExpectedText { get; set; } = string.Empty;
    public string TypedText { get; set; } = string.Empty;

    public List<TypingCharacterResult> Characters { get; set; } = new();

    public bool HasErrors => Characters.Any(c => !c.IsCorrect);
}
