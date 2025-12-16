using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Models;


namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IShareImageService
    {
        Task ShareLessonSummaryImageAsync(string lessonName, string progressText);
        Task<string> SaveLessonSummaryImageAsync(string lessonName, string progressText);
        Task<string?> SaveWithPickerAsync(string lessonName, string progressText);
    }
}
