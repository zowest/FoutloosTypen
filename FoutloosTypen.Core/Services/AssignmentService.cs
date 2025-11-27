using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Interfaces;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Services;

namespace FoutloosTypen.Core.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IAssignmentRepository _assignmentRepository;
        public AssignmentService(IAssignmentRepository assignmentRepository)
        {
            _assignmentRepository = assignmentRepository;
        }
        public List<Assignment> GetAll()
        {
            return _assignmentRepository.GetAll();
        }
        public Assignment? Get(int id)
        {
            return _assignmentRepository.Get(id);
        }
    }
}
