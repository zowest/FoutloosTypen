using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface ITimerService
    {
        event EventHandler? TimerExpired;
        
        double TimeRemaining { get; }
        string TimeRemainingFormatted { get; }
        bool IsRunning { get; }
        
        void Initialize(double timeInSeconds);
        void Start();
        void Stop();
        void Restart();

        void AddTime(double seconds);
    }
}
