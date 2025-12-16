using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public Student? Get(string email)
        {
            return _studentRepository.Get(email);
        }

        public Student? Get(int id)
        {
            return _studentRepository.Get(id);
        }

        public List<Student> GetAll()
        {
            List<Student> students = _studentRepository.GetAll();
            return students;
        }
    }
}
