using FoutloosTypen.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface ILessonRepository
    {
        public List<Lesson> GetAll();
        public Lesson Get(int id);
    }
}
