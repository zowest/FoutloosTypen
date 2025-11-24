
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface ILessonService
    {
        public IEnumerable<Lesson> GetAll();
        public Lesson? Get(int id);
    }
}
