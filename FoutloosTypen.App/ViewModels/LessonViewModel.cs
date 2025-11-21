using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using FoutloosTypen.Core.Interfaces.Services;


namespace FoutloosTypen.ViewModels
{
    public partial class LessonViewModel : BaseViewModel
    {
        private readonly ILessonService _lessonService;

        public ObservableCollection<Lesson> Lessons { get; set; }
    }
}
