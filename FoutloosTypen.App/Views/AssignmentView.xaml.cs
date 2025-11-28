using FoutloosTypen.ViewModels;

namespace FoutloosTypen.Views;

public partial class AssignmentView : ContentPage
{
    private readonly AssignmentViewModel? _vm;
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

    }