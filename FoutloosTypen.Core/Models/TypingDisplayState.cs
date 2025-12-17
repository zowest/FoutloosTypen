namespace FoutloosTypen.Core.Models
{
    /// <summary>
    /// Represents the display state for the typing exercise UI
    /// </summary>
    public class TypingDisplayState
    {
        /// <summary>
        /// Text that has been typed correctly (shown in black)
        /// </summary>
        public string CorrectText { get; set; } = string.Empty;

        /// <summary>
        /// Text that has been typed incorrectly (shown in red)
        /// </summary>
        public string ErrorText { get; set; } = string.Empty;

        /// <summary>
        /// The current character to type (shown with underline)
        /// </summary>
        public string CursorChar { get; set; } = string.Empty;

        /// <summary>
        /// Remaining text to type (shown in gray)
        /// </summary>
        public string RemainingText { get; set; } = string.Empty;

        /// <summary>
        /// Whether there are any errors in the current input
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorText);

        /// <summary>
        /// Number of correctly typed characters
        /// </summary>
        public int CorrectCharacterCount { get; set; }

        /// <summary>
        /// Whether the sentence is complete and correct
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Index of the first error (-1 if no errors)
        /// </summary>
        public int FirstErrorIndex { get; set; } = -1;
    }
}