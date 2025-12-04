using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly List<Student> studentList;

        public StudentRepository()
        {
            studentList = [
                new Student(1, "co2mott", "Matthijs", "bHbXpFGYmI/YjrZvIVvu0Q==.kf5UUcO9kF5t9hSplOjbDUX2u2vle52Y4FHj4cFgE+s=", 1),
                new Student(2, "meesvz123", "Mees", "bHbXpFGYmI/YjrZvIVvu0Q==.kf5UUcO9kF5t9hSplOjbDUX2u2vle52Y4FHj4cFgE+s=", 1),
                new Student(3, "danial8910848", "Danial", "bHbXpFGYmI/YjrZvIVvu0Q==.kf5UUcO9kF5t9hSplOjbDUX2u2vle52Y4FHj4cFgE+s=", 1),
                new Student(4, "sowiaelys", "Zoe", "bHbXpFGYmI/YjrZvIVvu0Q==.kf5UUcO9kF5t9hSplOjbDUX2u2vle52Y4FHj4cFgE+s=", 1),
                new Student(5, "ikweetgeennaam", "Anne Dirk", "bHbXpFGYmI/YjrZvIVvu0Q==.kf5UUcO9kF5t9hSplOjbDUX2u2vle52Y4FHj4cFgE+s=", 1)
                ];
        }

        public Student? Get(string username)
        {
            Student? student = studentList.FirstOrDefault(c => c.Username.Equals(username));
            return student;
        }

        public Student? Get(int id)
        {
            Student? student = studentList.FirstOrDefault(c => c.Id == id);
            return student;
        }

        public List<Student> GetAll()
        {
            return studentList;
        }
    }
}
