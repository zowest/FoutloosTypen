using FoutloosTypen.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IPracticeMaterialRepository
    {
        public List<PracticeMaterial> GetAll();
        public PracticeMaterial Get(int id);

        List<PracticeMaterial> GetByAssignmentId(int assignmentId);
    }
}
