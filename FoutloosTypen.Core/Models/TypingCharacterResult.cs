namespace FoutloosTypen.Core.Models
{
    public class TypingCharacterResult
    {
        public char? Expected { get; set; }
        public char? Typed { get; set; }
        public bool IsCorrect => Expected == Typed;
    }
}