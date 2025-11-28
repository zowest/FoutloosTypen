namespace FoutloosTypen.Core.Models
{
    public partial class Assignment : Model
    {
        public double TimeLimit { get; set; }
        public int LessonId { get; set; }

        public Assignment(int id, double timelimit, int lessonId) : base(id)
        {
            TimeLimit = timelimit;
            LessonId = lessonId;
        }

        public Assignment() : base(0)
        {
        }
    }
}