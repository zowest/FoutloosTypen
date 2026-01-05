using FoutloosTypen.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IAssignmentRepository
    {
        public List<Assignment> GetAll();
        public Assignment Get(int id);
    }
}
