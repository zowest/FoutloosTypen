namespace FoutloosTypen.Core.Models
{
    public class TtsRequest
    {
        public string Text { get; set; } = string.Empty;

        // Rate is optional; ignored on Windows
        public float Rate { get; set; } = 1.0f;
        public float Volume { get; set; } = 1.0f;
        public float Pitch { get; set; } = 1.0f;

        public string? Locale { get; set; }
    }
}
