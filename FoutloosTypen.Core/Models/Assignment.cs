using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FoutloosTypen.Core.Models
{
    public partial class Assignment : Model
    {
        public double TimeLimit { get; set; }

        public Assignment(int id, double timelimit) : base(id)
        {
            TimeLimit = timelimit;
        }

        public Assignment() : base(0)
        {
        }
    }
}
