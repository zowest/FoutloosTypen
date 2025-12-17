namespace FoutloosTypen.Core.Models
{
    public class SharePost : Model
    {
        public SharePost() : base(0) {}
        public SharePost(int id) : base(id) {}

        public int LessonId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
