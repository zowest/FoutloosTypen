using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Services;

namespace FoutloosTypen.ViewModels
{
    public partial class LessonViewModel : BaseViewModel
    {
        private readonly ILessonService _lessonService;

        public ObservableCollection<Lesson> Lessons { get; set; } = new();

        public LessonViewModel(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        public async Task OnAppearingAsync()
        {
            // Load lessons when the view appears
            var items = _lessonService.GetAll();
            Lessons.Clear();
            foreach (var lesson in items)
            {
                Lessons.Add(lesson);
            }
            await Task.CompletedTask;
        }
    }
}
