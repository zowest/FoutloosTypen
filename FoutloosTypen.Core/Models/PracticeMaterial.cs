namespace FoutloosTypen.Core.Models
{
    public partial class PracticeMaterial : Model
    {
        public string Sentence { get; set; }

        public int AssigmentId { get; set; }

        public PracticeMaterial(int id, string sentence, int assigmentId) : base(id)
        {
            sentence = Sentence;
            AssigmentId = assigmentId;
        }

        public PracticeMaterial() : base(0)
        {
        }
    }
}
