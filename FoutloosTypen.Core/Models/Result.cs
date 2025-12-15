using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Models
{
    public partial class Result : Model
    {
        public double StrokesPerMinute { get; set; }
        public int TotalMistakes { get; set; }
        public int Score { get; set; }
        public int TimeRemaining { get; set; }
        public double WordsPerMinute { get; set; }

        public Result(int id, double strokesPerMinute, int totalMistakes, int score, int timeRemaining, double wordsPerMinute)
            : base(id)
        {
            StrokesPerMinute = strokesPerMinute;
            TotalMistakes = totalMistakes;
            Score = score;
            TimeRemaining = timeRemaining;
            WordsPerMinute = wordsPerMinute;
        }
    }
}
