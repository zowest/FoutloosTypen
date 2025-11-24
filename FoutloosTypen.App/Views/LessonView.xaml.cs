using FoutloosTypen.ViewModels;
using FoutloosTypen.Core.Interfaces.Services;

namespace FoutloosTypen.Views;

public partial class LessonView : ContentPage
{
    public LessonView(ILessonService lessonService)
    {
        InitializeComponent();
        BindingContext = new LessonViewModel(lessonService);
    }
}