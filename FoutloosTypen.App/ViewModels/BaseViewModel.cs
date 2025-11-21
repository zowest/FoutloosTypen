using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoutloosTypen.ViewModels
{
    public partial class BaseViewModel
    {
        //    [ObservableProperty]
        //   string title = "";

        public virtual void Load() { }
        public virtual void OnAppearing() { }
        public virtual void OnDisappearing() { }
    }
}