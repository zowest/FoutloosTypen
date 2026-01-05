using FoutloosTypen.Core.Helpers;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Repositories;

namespace FoutloosTypen.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IStudentService _studentService;
        public AuthService(IStudentService studentService)
        {
            _studentService = studentService;
        }
        public Student? Login(string username, string password)
        {
            Student? student = _studentService.Get(username);
            if (student == null) return null;
            if (PasswordHelper.VerifyPassword(password, student.Password)) return student;
            return null;
        }
    }
}