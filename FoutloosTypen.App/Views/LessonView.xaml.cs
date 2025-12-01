using FoutloosTypen.ViewModels;
using Microsoft.Maui.Controls;

namespace FoutloosTypen.Views
{
    public partial class LessonView : ContentPage
    {
        private readonly LearnpathViewModel _vm;

        public LessonView()
        {
            InitializeComponent();
        }

        public LessonView(LearnpathViewModel vm) : this()
        {
            BindingContext = _vm = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_vm != null)
                await _vm.OnAppearingAsync();
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

        private async void OnPlayClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(AssignmentView));
            }
            catch
            {
                await Navigation.PushAsync(new AssignmentView());
            }
        }
    }
}
