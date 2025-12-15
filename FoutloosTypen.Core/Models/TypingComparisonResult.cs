namespace FoutloosTypen.Core.Models
{
    public class TypingComparisonResult
    {
        public string ExpectedText { get; set; } = string.Empty;
        public string TypedText { get; set; } = string.Empty;
        public List<TypingCharacterResult> Characters { get; set; } = new();
        public int TotalMistakes { get; set; } = 0;
        public bool HasErrors => Characters.Any(c => !c.IsCorrect);
    }
}