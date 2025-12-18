using System.Collections.Generic;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Services
{
    public class EndlessModeService : IEndlessModeService
    {
        private readonly IEndlessModeRepository _endlessModeRepository;

        public EndlessModeService(IEndlessModeRepository endlessModeRepository)
        {
            _endlessModeRepository = endlessModeRepository;
        }

        public List<EndlessMode> GetAll()
        {
            return _endlessModeRepository.GetAll();
        }

        public EndlessMode? Get(int id)
        {
            return _endlessModeRepository.Get(id);
        }
    }
}
