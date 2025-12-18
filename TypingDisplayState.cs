namespace FoutloosTypen.Core.Models
{

    public class TypingDisplayState
    {
        public string CorrectText { get; set; } = string.Empty;

        public string ErrorText { get; set; } = string.Empty;

        public string CursorChar { get; set; } = string.Empty;

        public string RemainingText { get; set; } = string.Empty;

        public bool HasError => !string.IsNullOrEmpty(ErrorText);

        public int CorrectCharacterCount { get; set; }

        public bool IsComplete { get; set; }

        public int FirstErrorIndex { get; set; } = -1;
    }
}