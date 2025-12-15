using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FoutloosTypen.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly GlobalViewModel _global;

        private string studentName = string.Empty;

        public string StudentName
        {
            get => studentName;
            set => SetProperty(ref studentName, value);
        }

        public ProfileViewModel(GlobalViewModel global)
        {
            _global = global;
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            
            if (_global.Student != null)
            {
                StudentName = _global.Student.Name;
            }
        }
    }
}
