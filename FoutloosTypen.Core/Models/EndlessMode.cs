using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Models
{
    public partial class EndlessMode : Model
    {
        public string Word { get; set; }

        public EndlessMode(int id, string word) : base(id)
        {
            Word = word;
        }

        public EndlessMode() : base(0)
        {
        }
    }
}
