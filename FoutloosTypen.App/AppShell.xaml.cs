using FoutloosTypen.Views;

namespace FoutloosTypen
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Register routes for navigation
            Routing.RegisterRoute(nameof(AssignmentView), typeof(AssignmentView));
            Routing.RegisterRoute("Login", typeof(LoginView));
        }
    }
}
