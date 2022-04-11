using Prism.Mvvm;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace taskmaker_wpf.ViewModels {
    public class TestWindowViewModel : BindableBase {
        private string _motorValue;
        public string MotorValue {
            get => _motorValue;
            set {
                SetProperty(ref _motorValue, value);
            }
        }

        private ICommand testCommand;
        public ICommand TestCommand => testCommand ?? (testCommand = new DelegateCommand(TestCommandExecute));

        private ICommand navigateCommand;
        public ICommand NavigateCommand => navigateCommand ?? (navigateCommand = new DelegateCommand<string>(NavigateCommandExecute));

        private readonly IRegionManager _regionManager;

        public TestWindowViewModel(IRegionManager regionManager) {
            _regionManager = regionManager;
        }

        public void TestCommandExecute() {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void NavigateCommandExecute(string navigatePath) {
            if (navigatePath != null)
                _regionManager.RequestNavigate("ContentRegion", navigatePath);
        }
    }
}
