using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IPracticeMaterialService
    {
        //public list<PracticeMaterial> GetAll();
        public PracticeMaterial? Get(int id);
    }
}
