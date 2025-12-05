using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FoutloosTypen.Core.Interfaces.Services;

namespace FoutloosTypen.Core.Services
{
    public class TimerService : ObservableObject, ITimerService
    {
        private System.Timers.Timer? _timer;
        private double _initialTime;
        private double _timeRemaining;

        public double TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                if (SetProperty(ref _timeRemaining, value))
                {
                    OnPropertyChanged(nameof(TimeRemainingFormatted));
                }
            }
        }

        public string TimeRemainingFormatted
        {
            get
            {
                var timeSpan = TimeSpan.FromSeconds(TimeRemaining);
                return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
        }

        public bool IsRunning => _timer != null;

        public void Initialize(double timeInSeconds)
        {
            _initialTime = timeInSeconds;
            TimeRemaining = timeInSeconds;
            Debug.WriteLine($"Timer initialized: {timeInSeconds} seconds");
        }

        public void Start()
        {
            if (_timer != null)
            {
                Debug.WriteLine("Timer already running");
                return;
            }

            _timer = new System.Timers.Timer(1000); // Tick every second
            _timer.Elapsed += OnTimerTick;
            _timer.AutoReset = true;
            _timer.Start();
            OnPropertyChanged(nameof(IsRunning));
            Debug.WriteLine("Timer started");
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= OnTimerTick;
                _timer.Dispose();
                _timer = null;
                OnPropertyChanged(nameof(IsRunning));
                Debug.WriteLine("Timer stopped");
            }
        }

        public void Restart()
        {
            Stop();
            TimeRemaining = _initialTime;
            Start();
            Debug.WriteLine("Timer restarted");
        }

        private void OnTimerTick(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current?.Dispatcher.Dispatch(() =>
            {
                TimeRemaining--;

                Debug.WriteLine($"Time remaining: {TimeRemainingFormatted}");

                if (TimeRemaining <= 0)
                {
                    TimeRemaining = 0;
                    Stop();
                    _ = OnTimerExpiredAsync();
                }
            });
        }

        private async Task OnTimerExpiredAsync()
        {
            Debug.WriteLine("Timer expired!");

            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Tijd Voorbij!",
                    "De tijd is afgelopen.",
                    "OK");

                await Shell.Current.GoToAsync("..");
            }
        }
    }
}