using CommunityToolkit.Mvvm.ComponentModel;
using FoutloosTypen.Core.Interfaces.Services;

namespace FoutloosTypen.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly GlobalViewModel _globalViewModel;
        private readonly IStudentService _studentService;

        [ObservableProperty]
        private bool _useTtsMode;

        public SettingsViewModel(GlobalViewModel globalViewModel, IStudentService studentService)
        {
            _globalViewModel = globalViewModel;
            _studentService = studentService;
            if (_globalViewModel.Student != null)
            {
                UseTtsMode = _globalViewModel.Student.UseTtsMode;
            }
        }

        partial void OnUseTtsModeChanged(bool value)
        {
            if (_globalViewModel.Student != null)
            {
                _globalViewModel.Student.UseTtsMode = value;
                _studentService.UpdateTtsMode(_globalViewModel.Student.Id, value);
            }
        }
    }
}