using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels {
    public class RegionSettingsViewModel : BindableBase {
        private MotorService _motorSvr;

        private ICommand connectCmd;

        public RegionSettingsViewModel(
            MotorService service) { 
            _motorSvr = service;
        }

        public ICommand ConnectCmd => connectCmd?? (connectCmd = new DelegateCommand(ConnectCmdExecute));

        private void ConnectCmdExecute() {
            throw new NotImplementedException();
        }
    }
}
