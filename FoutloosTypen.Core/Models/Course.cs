using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Models
{
    public partial class Course : Model
    {
        public string Description { get; set; }
        public int Difficulty { get; set; }
        public string Name { get; set; }

        public Course(int id, string name, string description, int difficulty) : base(id)
        {
            Description = description;
            Difficulty = difficulty;
            Name = name;
        }
        public Course(int id, string name)
            : base(id)
        {
        }

        public Course() : base(0)
        {
        }
    }
}
