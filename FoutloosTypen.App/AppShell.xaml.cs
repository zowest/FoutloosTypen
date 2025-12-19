using FoutloosTypen.Views;

namespace FoutloosTypen
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            Routing.RegisterRoute(nameof(EndlessModeView), typeof(EndlessModeView));
            Routing.RegisterRoute(nameof(AssignmentView), typeof(AssignmentView));
            Routing.RegisterRoute("Login", typeof(LoginView));
            Routing.RegisterRoute(nameof(ProfileView), typeof(ProfileView));
            Routing.RegisterRoute(nameof(SettingsView), typeof(SettingsView));
            Routing.RegisterRoute(nameof(TTSAssignmentView), typeof(TTSAssignmentView));
            Routing.RegisterRoute(nameof(LeaderboardView), typeof(LeaderboardView));
            Routing.RegisterRoute(nameof(LessonLeaderboardView), typeof(LessonLeaderboardView));  // ADD THIS
        }
    }
}