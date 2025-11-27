namespace FoutloosTypen.Core.Models
{
    public partial class Lesson : Model
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsTest { get; set; }
        public bool IsDone { get; set; }
        public int CourseId { get; set; }

        public Lesson(int id, string name, string description, bool isTest, bool isDone, int courseId)
            : base(id, name)
        {
            Name = name;
            Description = description;
            IsTest = isTest;
            IsDone = isDone;
            CourseId = courseId;
        }

        public Lesson(int id, string name)
            : base(id, name)
        {
        }

        public Lesson() : base(0, "")
        {
        }
    }
}
