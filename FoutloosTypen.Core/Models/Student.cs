using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoutloosTypen.Core.Models
{
    public partial class Student : Model
    {
        public string  Username { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Level { get; set; }
        public double AvgSpeed { get; set; }
        public double AvgPrecision { get; set; }

        public Student(int id, string username,  string name, string password, int level)
            : base(id)
        {
            Username = username;
            Name = name;
            Password = password;
            Level = level;
            AvgSpeed = 0;
            AvgPrecision = 0;
        }
    }
}
