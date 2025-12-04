using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IStudentRepository
    {
        public Student? Get(string username);
        public Student? Get(int id);
        public List<Student> GetAll();
    }
}
