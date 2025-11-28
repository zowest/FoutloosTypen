using System;
using FoutloosTypen.ViewModels;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;


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
            await _vm.OnAppearingAsync();
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
            await Shell.Current.GoToAsync("..");
        }
        catch
        {

            await Navigation.PopAsync();
        }
    }

}