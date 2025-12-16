using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface ITimerService
    {
        void Initialize(double timeInSeconds);
        void Start();
        void Stop();
        void Restart();
        

        double TimeRemaining { get; }
        string TimeRemainingFormatted { get; }
        bool IsRunning { get; }
    }
}
