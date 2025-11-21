using FoutloosTypen.ViewModels;

namespace FoutloosTypen.Views;

public partial class LessonView : ContentPage
{
    private readonly LessonViewModel _vm;

    public LessonView(LessonViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.OnAppearingAsync();
    }

}