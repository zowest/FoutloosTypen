namespace FoutloosTypen.Core.Models
{
    public enum AssignmentTypeEnum
    {
        Text = 1,
        Audio = 2
    }

    public partial class Assignment : Model
    {
        public double TimeLimit { get; set; }
        public int LessonId { get; set; }

        public AssignmentTypeEnum AssignmentType { get; set; }

        public int AssignmentTypeId
        {
            get => (int)AssignmentType;
            set
            {
                AssignmentType = System.Enum.IsDefined(typeof(AssignmentTypeEnum), value)
                    ? (AssignmentTypeEnum)value
                    : AssignmentTypeEnum.Text;
            }
        }

        public Assignment(int id, double timelimit, int lessonId) : base(id)
        {
            TimeLimit = timelimit;
            LessonId = lessonId;
            AssignmentType = AssignmentTypeEnum.Text;
        }

        public Assignment() : base(0)
        {
            AssignmentType = AssignmentTypeEnum.Text;
        }
    }
}