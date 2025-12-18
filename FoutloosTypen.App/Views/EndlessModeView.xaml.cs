using System;
using System.ComponentModel;
using System.Diagnostics;
using FoutloosTypen.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FoutloosTypen.Views;

public partial class EndlessModeView : ContentPage
{
    private EndlessModeViewModel? _vm;
    private int _lastScore;

    public EndlessModeView()
    {
        InitializeComponent();
    }

    public EndlessModeView(EndlessModeViewModel vm) : this()
    {
        BindingContext = _vm = vm;

        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IsVisible = false,
            IsEnabled = false
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_vm != null)
        {
            _lastScore = _vm.Score;
            _vm.PropertyChanged += OnViewModelPropertyChanged;
            await _vm.OnAppearingAsync();
        }

        await Task.Delay(100);
        HiddenEntry?.Focus();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_vm != null)
        {
            _vm.PropertyChanged -= OnViewModelPropertyChanged;
            _vm.Timer.Stop();
        }
    }

    // =========================
    // ViewModel ? UI animaties
    // =========================
    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_vm == null)
            return;

        if (e.PropertyName == nameof(EndlessModeViewModel.ComboText)
            && !string.IsNullOrWhiteSpace(_vm.ComboText))
        {
            await AnimateComboAsync();
        }

        if (e.PropertyName == nameof(EndlessModeViewModel.Score))
        {
            if (_vm.Score > _lastScore)
                await AnimateScoreAsync();

            _lastScore = _vm.Score;
        }
    }

    private async Task AnimateComboAsync()
    {
        if (ComboLabel == null)
            return;

        ComboLabel.Scale = 1;
        await ComboLabel.ScaleTo(1.35, 120, Easing.CubicOut);
        await ComboLabel.ScaleTo(1.0, 120, Easing.CubicIn);
    }

    private async Task AnimateScoreAsync()
    {
        if (ScoreLabel == null)
            return;

        ScoreLabel.Scale = 1;
        await ScoreLabel.ScaleTo(1.2, 80, Easing.CubicOut);
        await ScoreLabel.ScaleTo(1.0, 80, Easing.CubicIn);
    }

    // =========================
    // UI events
    // =========================
    private async void OnHomeClicked(object sender, EventArgs e)
    {
        _vm?.Timer.Stop();
        await Shell.Current.GoToAsync("..");
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _vm?.UpdateTypedText(e.NewTextValue ?? string.Empty);
    }

    private void OnTapToFocus(object sender, EventArgs e)
    {
        HiddenEntry?.Focus();
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        Debug.WriteLine("Entry focused");
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        Debug.WriteLine("Entry unfocused");
    }

    private void OnHoverEnter(object sender, PointerEventArgs e)
    {
        if (sender is VisualElement ve)
            ve.BackgroundColor = Colors.LightGrey;
    }

    private void OnHoverExit(object sender, PointerEventArgs e)
    {
        if (sender is VisualElement ve)
            ve.BackgroundColor = Colors.Transparent;
    }
}
