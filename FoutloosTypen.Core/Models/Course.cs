using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Models
{
    public partial class Course : Model
    {
        public String Name { get; set; }
        public String Description { get; set; }
        public int Difficulty { get; set; }

        public Course(int id, string name, string description, int difficulty) : base(id, "")
        {
            Name = name;
            Description = description;
            Difficulty = difficulty;
        }
        public Lesson Lesson { get; set; } = new Lesson(0, "None");
    }
}
