using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IService<T> where T : Model
    {
        List<T> GetAll();
        T? Get(int id);
        T Add(T item);
        T? Update(T item);
        T? Delete(T item);
    }
}
