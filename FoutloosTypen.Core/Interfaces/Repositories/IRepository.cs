using System.Collections.Generic;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Repositories
{

    public interface IRepository<T> where T : Model
    {
        List<T> GetAll();
        T? Get(int id);
        T Add(T item);
        T? Update(T item);
        T? Delete(T item);
    }
}