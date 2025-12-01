using System.Collections.Generic;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IPracticeMaterialService
    {
        public List<PracticeMaterial> GetAll();
        public PracticeMaterial? Get(int id);
    }
}
