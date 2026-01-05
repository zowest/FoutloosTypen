using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FoutloosTypen.Core.Models
{
    public abstract partial class Model : ObservableObject
    {
        public int Id { get; set; }

        protected Model(int id)
        {
            Id = id;
        }
    }
}
