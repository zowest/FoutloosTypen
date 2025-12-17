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
        private const double BONUS_TIME = 2;

        private readonly IPracticeMaterialService _practiceMaterialService;
        private readonly ITimerService _timerService;
        private readonly Random _random = new();

        private List<PracticeMaterial> _materials = new();
        private int _materialIndex;
        private int _lastCheckedWordIndex;
        private string _previousUserInput = string.Empty;

        public ITimerService Timer => _timerService;

        private PracticeMaterial? _currentMaterial;
        public PracticeMaterial? CurrentMaterial
        {
            get => _currentMaterial;
            set
            {
                _currentMaterial = value;
                OnPropertyChanged(nameof(CurrentMaterial));
                UpdateTotalCharacters();
                ResetTyping();
            }
        }

        private string _userInput = string.Empty;
        public string UserInput
        {
            get => _userInput;
            set
            {
                _userInput = value;
                OnPropertyChanged(nameof(UserInput));
                UpdateTypedCharacters();
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

        private int _typedCharacters;
        public int TypedCharacters
        {
            get => _typedCharacters;
            set
            {
                _typedCharacters = value;
                OnPropertyChanged(nameof(TypedCharacters));
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        private int _totalCharacters;
        public int TotalCharacters
        {
            get => _totalCharacters;
            set
            {
                _totalCharacters = value;
                OnPropertyChanged(nameof(TotalCharacters));
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public double Progress =>
            TotalCharacters == 0 ? 0 : (double)TypedCharacters / TotalCharacters;

        public string ProgressText =>
            $"{Math.Round(Progress * 100)}%";

        public ICommand RefreshCommand { get; }

        public EndlessModeViewModel(
            IPracticeMaterialService practiceMaterialService,
            ITimerService timerService)
        {
            _practiceMaterialService = practiceMaterialService;
            _timerService = timerService;

            RefreshCommand = new RelayCommand(Restart);
            _timerService.TimerExpired += OnTimerExpired;
        }

        public async Task OnAppearingAsync()
        {
            Start();
            await Task.CompletedTask;
        }

        private void Start()
        {
            LoadMaterials();
            _timerService.Initialize(START_TIME);
            _timerService.Start();
        }

        private void LoadMaterials()
        {
            _materials = _practiceMaterialService
                .GetAll()
                .OrderBy(_ => _random.Next())
                .ToList();

            _materialIndex = 0;
            CurrentMaterial = _materials.FirstOrDefault();
        }

        public void UpdateTypedText(string typedText)
        {
            if (CurrentMaterial == null)
                return;

            DetectMistake(typedText);

            UserInput = typedText;
            UpdateFormattedText();
            CheckCompletedWords();

            if (typedText == CurrentMaterial.Sentence)
            {
                MoveNext();
            }

            _previousUserInput = typedText;
        }

        private void DetectMistake(string typedText)
        {
            if (typedText.Length <= _previousUserInput.Length)
                return;

            int index = typedText.Length - 1;

            if (index >= CurrentMaterial!.Sentence.Length)
                return;

            if (typedText[index] != CurrentMaterial.Sentence[index])
            {
                _timerService.AddTime(-BONUS_TIME);
                Debug.WriteLine($"-{BONUS_TIME}s fout karakter op positie {index + 1}");
            }
        }

        private void CheckCompletedWords()
        {
            if (CurrentMaterial == null)
                return;

            var targetWords = CurrentMaterial.Sentence.Split(' ');
            var typedWords = UserInput.Split(' ');

            int completedWordCount = typedWords.Length - 1;

            while (_lastCheckedWordIndex < completedWordCount &&
                   _lastCheckedWordIndex < targetWords.Length)
            {
                if (typedWords[_lastCheckedWordIndex] ==
                    targetWords[_lastCheckedWordIndex])
                {
                    _timerService.AddTime(BONUS_TIME);
                    Debug.WriteLine($"+{BONUS_TIME}s voor woord {_lastCheckedWordIndex + 1}");
                }

                _lastCheckedWordIndex++;
            }
        }

        private void MoveNext()
        {
            _materialIndex++;

            if (_materialIndex >= _materials.Count)
            {
                _materials = _materials
                    .OrderBy(_ => _random.Next())
                    .ToList();
                _materialIndex = 0;
            }

            CurrentMaterial = _materials[_materialIndex];
        }

        private void UpdateFormattedText()
        {
            var formatted = new FormattedString();
            var target = CurrentMaterial?.Sentence ?? string.Empty;

            for (int i = 0; i < target.Length; i++)
            {
                var span = new Span
                {
                    Text = target[i].ToString(),
                    FontSize = 32
                };

                if (i < UserInput.Length)
                {
                    span.TextColor =
                        UserInput[i] == target[i]
                        ? Colors.Black
                        : Colors.Red;
                }
                else
                {
                    span.TextColor = Colors.LightGray;
                }

                formatted.Spans.Add(span);
            }

            FormattedText = formatted;
        }

        private void UpdateTypedCharacters()
        {
            if (CurrentMaterial == null)
                return;

            int count = 0;
            for (int i = 0; i < UserInput.Length && i < CurrentMaterial.Sentence.Length; i++)
            {
                if (UserInput[i] == CurrentMaterial.Sentence[i])
                    count++;
            }

            TypedCharacters = count;
        }

        private void UpdateTotalCharacters()
        {
            TotalCharacters = CurrentMaterial?.Sentence?.Length ?? 0;
        }

        private void ResetTyping()
        {
            UserInput = string.Empty;
            TypedCharacters = 0;
            _lastCheckedWordIndex = 0;
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
            Debug.WriteLine("Endless mode voorbij");
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
