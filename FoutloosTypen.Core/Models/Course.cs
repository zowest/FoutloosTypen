using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Models
{
    public partial class Course : Model
    {
        // Removed duplicate Name property (Model already provides Name via ObservableProperty)
        public string Description { get; set; }
        public int Difficulty { get; set; }

        public Course(int id, string name, string description, int difficulty) : base(id, name)
        {
            Description = description;
            Difficulty = difficulty;
        }
        public Course(int id, string name)
            : base(id, name)
        {
        }

        public Course() : base(0, "")
        {
        }
    }
}
