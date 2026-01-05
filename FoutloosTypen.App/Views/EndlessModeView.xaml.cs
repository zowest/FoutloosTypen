using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using FoutloosTypen.ViewModels;
using FoutloosTypen.Views.Drawables;

namespace FoutloosTypen.Views
{
    public partial class EndlessModeView : ContentPage
    {
        private readonly EndlessModeViewModel _vm;
        private readonly ComboBarDrawable _comboDrawable;
        private bool _handledGameOver;

        public EndlessModeView(EndlessModeViewModel vm)
        {
            InitializeComponent();

            _vm = vm;
            BindingContext = _vm;

            // Combo bar (GraphicsView)
            _comboDrawable = new ComboBarDrawable(_vm);
            ComboBar.Drawable = _comboDrawable;

            // Back button uit
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                IsVisible = false,
                IsEnabled = false
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _handledGameOver = false;

            _vm.PropertyChanged += OnVmChanged;
            await _vm.OnAppearingAsync();

            await Task.Delay(100);
            HiddenEntry?.Focus();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.PropertyChanged -= OnVmChanged;
            _vm.Timer.Stop();
        }

        private async void OnVmChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EndlessModeViewModel.ComboProgress))
            {
                ComboBar?.Invalidate();
                return;
            }

            if (e.PropertyName == nameof(EndlessModeViewModel.IsGameOver))
            {
                if (_handledGameOver || !_vm.IsGameOver)
                    return;

                _handledGameOver = true;

                bool restart = await DisplayAlert(
                    "Game over",
                    $"Score: {_vm.Score}",
                    "Opnieuw",
                    "Home"
                );

                if (restart)
                {
                    _vm.RefreshCommand.Execute(null);
                    await Task.Delay(100);
                    HiddenEntry?.Focus();
                    _handledGameOver = false;
                }
                else
                {
                    await Shell.Current.GoToAsync("..");
                }
            }
        }

        // =====================
        // UI event handlers
        // =====================

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _vm.UpdateTypedText(e.NewTextValue ?? string.Empty);
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            _vm.Timer.Stop();
            await Shell.Current.GoToAsync("..");
        }

        private void OnTapToFocus(object sender, EventArgs e)
        {
            HiddenEntry?.Focus();
        }
    }
}
