using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FoutloosTypen.Core.Models
{
    public abstract partial class Model(int id) : ObservableObject
    {
        public int Id { get; set; } = id;
    }
}
