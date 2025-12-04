using FoutloosTypen.ViewModels;
using FoutloosTypen.Views;

namespace FoutloosTypen
{
    public partial class App : Application
    {
        public App(LoginViewModel viewModel)
        {
            InitializeComponent();

            //MainPage = new AppShell();
            MainPage = new LoginView(viewModel);
        }
    }
}