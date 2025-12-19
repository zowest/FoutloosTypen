using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using FoutloosTypen.Core.Interfaces.Services;

namespace FoutloosTypen.Core.Services
{
    public class TimerService : ObservableObject, ITimerService
    {
        private IDispatcherTimer? _timer;
        private double _initialTime;
        private double _timeRemaining;

        public event EventHandler? TimerExpired;

        public double TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                if (SetProperty(ref _timeRemaining, value))
                    OnPropertyChanged(nameof(TimeRemainingFormatted));
            }
        }

        public string TimeRemainingFormatted
            => $"{(int)(TimeRemaining / 60):D2}:{(int)(TimeRemaining % 60):D2}";

        public bool IsRunning => _timer?.IsRunning == true;

        public void Initialize(double timeInSeconds)
        {
            _initialTime = Math.Min(timeInSeconds, 3600);
            TimeRemaining = _initialTime;
        }

        public void Start()
        {
            if (_timer != null && _timer.IsRunning) return;
            if (Application.Current?.Dispatcher == null) return;

            _timer ??= Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick -= OnTimerTick;
            _timer.Tick += OnTimerTick;
            _timer.Start();
            OnPropertyChanged(nameof(IsRunning));
        }

        public void Stop()
        {
            if (_timer == null) return;
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
            OnPropertyChanged(nameof(IsRunning));
        }

        public void Restart()
        {
            Stop();
            TimeRemaining = _initialTime;
            Start();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (TimeRemaining > 0) 
                TimeRemaining--;
            
            if (TimeRemaining <= 0)
            {
                TimeRemaining = 0;
                Stop();
                TimerExpired?.Invoke(this, EventArgs.Empty);
            }
        }
        public void AddTime(double seconds)
        {
            TimeRemaining = Math.Max(0, TimeRemaining + seconds);
            OnPropertyChanged(nameof(TimeRemaining));
            OnPropertyChanged(nameof(TimeRemainingFormatted));
        }




    }
}