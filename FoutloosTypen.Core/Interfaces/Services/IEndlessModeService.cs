using System.Collections.Generic;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IEndlessModeService
    {
        public List<EndlessMode> GetAll();
        public EndlessMode? Get(int id);
    }
}
