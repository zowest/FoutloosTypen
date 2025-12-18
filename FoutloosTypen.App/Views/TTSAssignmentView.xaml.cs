using System.Diagnostics;
using FoutloosTypen.ViewModels;

namespace FoutloosTypen.Views;

public partial class TTSAssignmentView : ContentPage
{
    private Button? HoverButton;
    private readonly TTSAssignmentViewModel? _vm;
    public TTSAssignmentView()
	{
		InitializeComponent();
	}
    public TTSAssignmentView(TTSAssignmentViewModel vm) : this()
    {
        BindingContext = _vm = vm;
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IsVisible = false,
            IsEnabled = false
        });
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
    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_vm != null)
        {
            //_vm.UpdateTypedText(e.NewTextValue);
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