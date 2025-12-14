using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Models
{
    public partial class StudentSession : Model
    {
        public double Score { get; set; }
        public int SessionMistakes { get; set; }
        public double WordsPerMinute { get; set; }
        public double StrokesPerMinute { get; set; }

        public StudentSession(int id, double score, int sessionMistakes, double wordsPerMinute, double strokesPerMinute)
            : base(id)
        {
            Score = score;
            SessionMistakes = sessionMistakes;
            WordsPerMinute = wordsPerMinute;
            StrokesPerMinute = strokesPerMinute;
        }
    }
}
