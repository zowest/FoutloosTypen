using FoutloosTypen.ViewModels;

namespace FoutloosTypen.Views;

public partial class ProfileView : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfileView(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel?.OnAppearing();
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
    private void OnHoverEnter(object sender, PointerEventArgs e)
    {
        switch (sender)
        {
            case Button btn:
                btn.BackgroundColor = Colors.LightGray;
                break;

            case Border border:
                border.Background = new SolidColorBrush(Colors.LightGray);
                break;

            case Label lbl:
                lbl.BackgroundColor = Colors.LightGray;
                break;
        }
    }

    private void OnHoverExit(object sender, PointerEventArgs e)
    {
        switch (sender)
        {
            case Button btn:
                btn.BackgroundColor = Colors.White;
                break;

            case Border border:
                border.Background = new SolidColorBrush(Colors.White);
                break;

            case Label lbl:
                lbl.BackgroundColor = Colors.White;
                break;
        }
    }
}