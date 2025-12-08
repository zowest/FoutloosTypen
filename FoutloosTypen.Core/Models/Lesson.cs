namespace FoutloosTypen.Core.Models
{
    public partial class Lesson : Model
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsTest { get; set; }
        public bool IsDone { get; set; }
        public int CourseId { get; set; }
        public double TotalTime { get; set; }

        public Lesson(int id, string name, string description, bool isTest, bool isDone, int courseId, double totalTime)
            : base(id)
        {
            Name = name;
            Description = description;
            IsTest = isTest;
            IsDone = isDone;
            CourseId = courseId;
            TotalTime = totalTime;
        }

        public Lesson(int id, string name)
            : base(id)
        {
        }

        public Lesson() : base(0)
        {
        }
    }
}
