namespace FoutloosTypen.Core.Models;


public class TypingCharacterResult
{
    public char? Typed { get; set; }
    public char? Expected { get; set; }
    public bool IsCorrect => Typed == Expected;
}
