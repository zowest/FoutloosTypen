namespace FoutloosTypen.Core.Models
{
    public partial class PracticeMaterial : Model
    {
        public string Sentence { get; set; }

        public int AssignmentId { get; set; }

        public PracticeMaterial(int id, string sentence, int assignmentId) : base(id)
        {
            Sentence = sentence;
            AssignmentId = assignmentId;
        }

        public PracticeMaterial() : base(0)
        {
        }
    }
}
