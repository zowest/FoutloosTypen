using System.Collections.Generic;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IPracticeMaterialService
    {
        List<PracticeMaterial> GetAll();
        PracticeMaterial? Get(int id);

        List<PracticeMaterial> GetByAssignmentId(int assignmentId);
    }
}
