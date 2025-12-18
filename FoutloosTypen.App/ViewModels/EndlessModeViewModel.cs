using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FoutloosTypen.ViewModels
{
    public partial class EndlessModeViewModel : BaseViewModel
    {
        private const double START_TIME = 60;
        private const int BASE_SCORE = 10;

        private readonly IEndlessModeService _endlessModeService;
        private readonly ITimerService _timerService;
        private readonly Random _random = new();

        private List<EndlessMode> _words = new();
        private string _previousUserInput = string.Empty;
        private int _combo;

        public ITimerService Timer => _timerService;

        // =====================
        // UI feedback
        // =====================
        public string ComboText => _combo > 0 ? $"🔥 x{_combo}" : string.Empty;

        public Color TimerColor
        {
            get
            {
                if (_combo >= 10) return Colors.Red;
                if (_combo >= 5) return Colors.Orange;
                return Colors.MediumPurple;
            }
        }

        // =====================
        // Adaptive difficulty
        // =====================
        private (int min, int max) GetAllowedWordLength()
        {
            if (_combo >= 10) return (7, 12);
            if (_combo >= 6) return (5, 9);
            if (_combo >= 3) return (4, 7);
            return (3, 5);
        }

        private double GetDrainMultiplier()
        {
            if (_combo >= 10) return 1.6;
            if (_combo >= 6) return 1.3;
            if (_combo >= 3) return 1.1;
            return 1.0;
        }

        // =====================
        // Current word
        // =====================
        private EndlessMode? _currentWord;
        public EndlessMode? CurrentWord
        {
            get => _currentWord;
            set
            {
                _currentWord = value;
                OnPropertyChanged(nameof(CurrentWord));
                ResetTyping();
            }
        }

        // =====================
        // Score
        // =====================
        private int _score;
        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged(nameof(Score));
            }
        }

        // =====================
        // Typing
        // =====================
        private string _userInput = string.Empty;
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
            set
            {
                _formattedText = value;
                OnPropertyChanged(nameof(FormattedText));
            }
        }

        public ICommand RefreshCommand { get; }

        public EndlessModeViewModel(
            IEndlessModeService endlessModeService,
            ITimerService timerService)
        {
            _endlessModeService = endlessModeService;
            _timerService = timerService;

            RefreshCommand = new RelayCommand(Restart);
            _timerService.TimerExpired += OnTimerExpired;
        }

        public async Task OnAppearingAsync()
        {
            Start();
            await Task.CompletedTask;
        }

        // =====================
        // Game flow
        // =====================
        private void Start()
        {
            Score = 0;
            _combo = 0;

            LoadWords();
            _timerService.Initialize(START_TIME);
            _timerService.Start();
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
            if (CurrentWord == null)
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

            double timeBonus =
                _combo >= 10 ? 4 :
                _combo >= 5 ? 3 : 2;

            _timerService.AddTime(timeBonus);

            AddScoreForWord();
            OnComboChanged();
        }

        private void AddScoreForWord()
        {
            double comboMultiplier = 1 + (_combo * 0.25);
            double drainMultiplier = GetDrainMultiplier();

            int gained = (int)Math.Round(
                BASE_SCORE * comboMultiplier * drainMultiplier
            );

            Score += gained;
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
                _timerService.AddTime(-2 * GetDrainMultiplier());
                ResetCombo();
            }
        }

        private void ResetCombo()
        {
            _combo = 0;
            OnComboChanged();
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

        // =====================
        // UI helpers
        // =====================
        private void OnComboChanged()
        {
            OnPropertyChanged(nameof(ComboText));
            OnPropertyChanged(nameof(TimerColor));
        }

        private void UpdateFormattedText()
        {
            var formatted = new FormattedString();
            string target = CurrentWord?.Word ?? string.Empty;
            string typed = UserInput ?? string.Empty;

            for (int i = 0; i < target.Length; i++)
            {
                var span = new Span
                {
                    Text = target[i].ToString(),
                    FontSize = 32
                };

                if (i < typed.Length)
                {
                    span.TextColor = typed[i] == target[i]
                        ? Colors.Black
                        : Colors.White;

                    span.BackgroundColor = typed[i] == target[i]
                        ? Colors.Transparent
                        : Colors.Red;
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

        private void Restart()
        {
            _timerService.Stop();
            Start();
        }

        private void OnTimerExpired(object? sender, EventArgs e)
        {
            Debug.WriteLine("Endless mode afgelopen");
            _timerService.Stop();
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            _timerService.Stop();
            _timerService.TimerExpired -= OnTimerExpired;
        }
    }
}
