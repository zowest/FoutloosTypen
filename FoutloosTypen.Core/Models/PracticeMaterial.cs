namespace FoutloosTypen.Core.Models
{
    public partial class PracticeMaterial : Model
    {
        public string Sentences { get; set; }

        public int AssigmentId { get; set; }

        public PracticeMaterial(int id, string sentences, int assigmentId) : base(id)
        {
            Sentences = sentences;
            AssigmentId = assigmentId;
        }

        public PracticeMaterial() : base(0)
        {
        }
    }
}
