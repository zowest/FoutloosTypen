using System.Collections.Generic;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Services
{
    public class PracticeMaterialService : IPracticeMaterialService
    {
        private readonly IPracticeMaterialRepository _practiceMaterialRepository;

        public PracticeMaterialService(IPracticeMaterialRepository practiceMaterialRepository)
        {
            _practiceMaterialRepository = practiceMaterialRepository;
        }

        public List<PracticeMaterial> GetAll()
        {
            return _practiceMaterialRepository.GetAll();
        }

        public PracticeMaterial? Get(int id)
        {
            return _practiceMaterialRepository.Get(id);
        }
        public List<PracticeMaterial> GetByAssignmentId(int assignmentId)
        {
            return _practiceMaterialRepository.GetByAssignmentId(assignmentId);
        }

    }
}