using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IAuthService
    {
        Student? Login(string username, string password);
    }
}
