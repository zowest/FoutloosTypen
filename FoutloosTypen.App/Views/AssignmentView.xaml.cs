using System;
using FoutloosTypen.ViewModels;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace FoutloosTypen.Views;

public partial class AssignmentView : ContentPage
{
    private readonly AssignmentViewModel? _vm;

    private Button? HoverButton;

    public AssignmentView()
    {
        InitializeComponent();
    }

    public AssignmentView(AssignmentViewModel vm) : this()
    {
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm is not null)
        {
            await _vm.OnAppearingAsync();
        }

        // Auto-focus the hidden entry to capture keyboard input
        await Task.Delay(100);
        HiddenEntry?.Focus();
    }

    public void SetHoverButton(Button button)
    {
        HoverButton = button;
    }

    private void OnHoverEnter(object sender, PointerEventArgs e)
    {
        switch (sender)
        {
            case Button button:
                button.BackgroundColor = Colors.LightGrey;
                break;
            case Border border:
                border.Stroke = Colors.LightGrey;
                break;
            case VisualElement ve:
                ve.BackgroundColor = Colors.LightGrey;
                break;
        }
    }

    private void OnHoverExit(object sender, PointerEventArgs e)
    {
        switch (sender)
        {
            case Button button:
                button.BackgroundColor = Colors.White;
                break;
            case Border border:
                border.Stroke = Colors.Transparent;
                break;
            case VisualElement ve:
                ve.BackgroundColor = Colors.White;
                break;
        }
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        try
        {
            _vm?.Timer?.Stop();
            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            await Navigation.PopAsync();
        }
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_vm != null)
        {
            _vm.UpdateTypedText(e.NewTextValue);
        }
    }

    private void OnTapToFocus(object sender, EventArgs e)
    {
        HiddenEntry?.Focus();
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        Debug.WriteLine("Entry focused - ready for typing");
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        Debug.WriteLine("Entry unfocused");
        // Optionally refocus automatically
        // HiddenEntry?.Focus();
    }
}