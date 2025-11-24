using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Services;

namespace FoutloosTypen.ViewModels
{
    public partial class LessonViewModel : BaseViewModel
    {
        private readonly ILessonService _lessonService;
        private readonly ICourseService _courseService;

        public ObservableCollection<Lesson> Lessons { get; set; } = new();
        public ObservableCollection<Course> Courses { get; set; } = new();

        public LessonViewModel(ILessonService lessonService, ICourseService courseService)
        {
            _lessonService = lessonService;
            _courseService = courseService;
        }

        public async Task OnAppearingAsync()
        {
            var lessonItems = _lessonService.GetAll();
            Lessons.Clear();
            foreach (var lesson in lessonItems)
            {
                Lessons.Add(lesson);
            }

            var courseItems = _courseService.GetAll();
            Courses.Clear();
            foreach (var course in courseItems)
            {
                Courses.Add(course);
            }
        }
    }
}
