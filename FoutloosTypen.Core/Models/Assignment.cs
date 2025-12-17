using FoutloosTypen.Core.Enums;

namespace FoutloosTypen.Core.Models
{
    public partial class Assignment : Model
    {
        public double TimeLimit { get; set; }
        public int LessonId { get; set; }

        // Use the shared enum and allow derived classes to set the value
        public AssignmentType AssignmentType { get; protected set; }

        public int AssignmentTypeId
        {
            get => (int)AssignmentType;
            set => AssignmentType = System.Enum.IsDefined(typeof(AssignmentType), value)
                ? (AssignmentType)value
                : AssignmentType.Normal;
        }

        public Assignment(int id, double timelimit, int lessonId) : base(id)
        {
            TimeLimit = timelimit;
            LessonId = lessonId;
            AssignmentType = AssignmentType.Normal;
        }

        public Assignment() : base(0)
        {
            AssignmentType = AssignmentType.Normal;
        }
    }
}