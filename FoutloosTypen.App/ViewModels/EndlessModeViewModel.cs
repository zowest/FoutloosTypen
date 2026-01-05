using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FoutloosTypen.ViewModels
{
    public partial class EndlessModeViewModel : BaseViewModel
    {
        private const double START_TIME = 60;
        private const int BASE_SCORE = 10;

        private const double COMBO_TIME_MAX = 3.0;
        private const double COMBO_TICK = 0.1;

        private readonly IEndlessModeService _endlessModeService;
        private readonly ITimerService _timerService;
        private readonly IResultRepository _resultRepository;
        private readonly GlobalViewModel _globalViewModel;
        private readonly Random _random = new();

        private readonly IDispatcherTimer _comboTimer;

        private List<EndlessMode> _words = new();

        private EndlessMode? _currentWord;
        private string _userInput = string.Empty;
        private string _previousUserInput = string.Empty;

        private int _combo;
        private int _score;
        private double _comboTimeRemaining;
        private bool _isGameOver;
        
        public EndlessModeViewModel(
            IEndlessModeService endlessModeService,
            ITimerService timerService,
            IResultRepository resultRepository,
            GlobalViewModel globalViewModel)
        {
            _endlessModeService = endlessModeService;
            _timerService = timerService;
            _resultRepository = resultRepository;
            _globalViewModel = globalViewModel;

            RefreshCommand = new RelayCommand(Restart);
            HomeCommand = new RelayCommand(() => RequestHome?.Invoke());

            _timerService.TimerExpired += OnTimerExpired;

            _comboTimer = Application.Current!.Dispatcher.CreateTimer();
            _comboTimer.Interval = TimeSpan.FromSeconds(COMBO_TICK);
            _comboTimer.Tick += OnComboTick;
        }

        public event Action? RequestHome;
        public ITimerService Timer => _timerService;

        public ICommand RefreshCommand { get; }
        public ICommand HomeCommand { get; }

        public bool IsGameOver
        {
            get => _isGameOver;
            private set
            {
                _isGameOver = value;
                OnPropertyChanged(nameof(IsGameOver));
            }
        }

        public EndlessMode? CurrentWord
        {
            get => _currentWord;
            private set
            {
                _currentWord = value;
                OnPropertyChanged(nameof(CurrentWord));
                ResetTyping();
            }
        }

        public string UserInput
        {
            get => _userInput;
            set
            {
                _userInput = value;
                OnPropertyChanged(nameof(UserInput));
            }
        }

        private FormattedString _formattedText = new();
        public FormattedString FormattedText
        {
            get => _formattedText;
            private set
            {
                _formattedText = value;
                OnPropertyChanged(nameof(FormattedText));
            }
        }

        public int Score
        {
            get => _score;
            private set
            {
                _score = value;
                OnPropertyChanged(nameof(Score));
            }
        }

        public string ComboText => _combo > 0 ? $"x{_combo}" : string.Empty;

        public double ComboTimeRemaining
        {
            get => _comboTimeRemaining;
            private set
            {
                _comboTimeRemaining = Math.Max(0, value);
                OnPropertyChanged(nameof(ComboTimeRemaining));
                OnPropertyChanged(nameof(ComboProgress));
            }
        }

        public double ComboProgress =>
            ComboTimeRemaining <= 0 ? 0 : ComboTimeRemaining / COMBO_TIME_MAX;

        public Color TimerColor => Colors.Black;

        public async Task OnAppearingAsync()
        {
            Start();
            await Task.CompletedTask;
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            _timerService.Stop();
            _comboTimer.Stop();
            _timerService.TimerExpired -= OnTimerExpired;
        }
        
        private void Start()
        {
            IsGameOver = false;
            Score = 0;
            ResetCombo();

            LoadWords();

            _timerService.Initialize(START_TIME);
            _timerService.Start();
        }

        private void Restart()
        {
            _timerService.Stop();
            _comboTimer.Stop();
            Start();
        }

        private void LoadWords()
        {
            _words = _endlessModeService
                .GetAll()
                .OrderBy(_ => _random.Next())
                .ToList();

            MoveNext();
        }

        public void UpdateTypedText(string typedText)
        {
            if (IsGameOver || CurrentWord == null)
                return;

            DetectMistake(typedText);

            UserInput = typedText;
            UpdateFormattedText();

            if (typedText == CurrentWord.Word)
            {
                HandleCorrectWord();
                MoveNext();
            }

            _previousUserInput = typedText;
        }

        private void HandleCorrectWord()
        {
            _combo++;

            StartComboTimer();
            _timerService.AddTime(GetTimeBonus());
            AddScoreForWord();

            OnPropertyChanged(nameof(ComboText));
            OnPropertyChanged(nameof(TimerColor));
        }

        private void DetectMistake(string typedText)
        {
            if (typedText.Length <= _previousUserInput.Length)
                return;

            int index = typedText.Length - 1;
            if (index >= CurrentWord!.Word.Length)
                return;

            if (typedText[index] != CurrentWord.Word[index])
            {
                _timerService.AddTime(-5);
                ResetCombo();
            }
        }

        private void ResetCombo()
        {
            _combo = 0;
            ComboTimeRemaining = 0;
            _comboTimer.Stop();

            OnPropertyChanged(nameof(ComboText));
            OnPropertyChanged(nameof(TimerColor));
        }

        private void MoveNext()
        {
            var (min, max) = GetAllowedWordLength();

            var pool = _words
                .Where(w => w.Word.Length >= min && w.Word.Length <= max)
                .ToList();

            if (pool.Count == 0)
                pool = _words;

            CurrentWord = pool[_random.Next(pool.Count)];
        }
        
        private void StartComboTimer()
        {
            ComboTimeRemaining = COMBO_TIME_MAX;

            if (!_comboTimer.IsRunning)
                _comboTimer.Start();
        }

        private void OnComboTick(object? sender, EventArgs e)
        {
            if (_combo <= 0)
            {
                _comboTimer.Stop();
                return;
            }

            ComboTimeRemaining -= COMBO_TICK;

            if (ComboTimeRemaining <= 0)
            {
                ComboTimeRemaining = 0;
                ResetCombo();
            }
        }

        private void AddScoreForWord()
        {
            double comboMultiplier = 1 + (_combo * 0.25);
            int gained = (int)Math.Round(BASE_SCORE * comboMultiplier);
            Score += gained;
        }

        private double GetTimeBonus()
        {
            if (_combo >= 10) return 4;
            if (_combo >= 5) return 2;
            return 1;
        }

        private (int min, int max) GetAllowedWordLength()
        {
            if (_combo >= 11) return (12, 15);
            if (_combo >= 10) return (11, 13);
            if (_combo >= 9) return (9, 11);
            if (_combo >= 7) return (7, 9);
            if (_combo >= 5) return (5, 7);
            if (_combo >= 3) return (4, 7);
            return (3, 4);
        }
        
        private void UpdateFormattedText()
        {
            var formatted = new FormattedString();
            string target = CurrentWord?.Word ?? string.Empty;
            string typed = UserInput;

            for (int i = 0; i < target.Length; i++)
            {
                var span = new Span
                {
                    Text = target[i].ToString(),
                    FontSize = 32
                };

                if (i < typed.Length)
                {
                    bool correct = typed[i] == target[i];
                    span.TextColor = correct ? Colors.Black : Colors.White;
                    span.BackgroundColor = correct ? Colors.Transparent : Colors.Red;
                }
                else if (i == typed.Length)
                {
                    span.TextColor = Colors.Gray;
                    span.BackgroundColor = Colors.LightGray;
                }
                else
                {
                    span.TextColor = Colors.LightGray;
                }

                formatted.Spans.Add(span);
            }

            FormattedText = formatted;
        }

        private void ResetTyping()
        {
            UserInput = string.Empty;
            _previousUserInput = string.Empty;
            UpdateFormattedText();
        }

        private void OnTimerExpired(object? sender, EventArgs e)
        {
            _timerService.Stop();
            _comboTimer.Stop();
            IsGameOver = true;
            
            SaveEndlessModeScore();
        }

        private void SaveEndlessModeScore()
        {

            if (_globalViewModel?.Student != null)
            {
                _resultRepository.SaveEndlessModeResult(_globalViewModel.Student.Id, Score);
            }
        }
    }
}
