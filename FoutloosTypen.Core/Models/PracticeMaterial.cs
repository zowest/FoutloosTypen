namespace FoutloosTypen.Core.Models
{
    public partial class PracticeMaterial : Model
    {
        public string Sentences { get; set; }

        public int AssignmentId { get; set; }

        public PracticeMaterial(int id, string sentences, int assignmentId) : base(id)
        {
            Sentences = sentences;
            AssignmentId = assignmentId;
        }

        public PracticeMaterial() : base(0)
        {
        }
    }
}
