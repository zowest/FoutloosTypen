namespace FoutloosTypen.Core.Models;

/// <summary>
/// Represents the comparison result of a single typed character.
/// </summary>
public class TypingCharacterResult
{
    public char? Typed { get; set; }
    public char? Expected { get; set; }
    public bool IsCorrect => Typed == Expected;
}
