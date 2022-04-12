using Prism.Mvvm;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using taskmaker_wpf.Services;
using System.Collections.ObjectModel;

namespace taskmaker_wpf.ViewModels {
    public class TestWindowViewModel : BindableBase {
        private string _motorValue;
        public string MotorValue {
            get => _motorValue;
            set {
                SetProperty(ref _motorValue, value);
            }
        }

        private Model.Data.Motor _motor;
        public Model.Data.Motor Motor {
            get => _motor;
            set {
                SetProperty(ref _motor, value);
            }
        }

        private ObservableCollection<double> _values;
        public ObservableCollection<double> Values {
            get => _values;
            set {
                SetProperty(ref _values, value);
            }

        }

        private ICommand testCommand;
        public ICommand TestCommand => testCommand ?? (testCommand = new DelegateCommand(TestCommandExecute));

        private ICommand navigateCommand;
        public ICommand NavigateCommand => navigateCommand ?? (navigateCommand = new DelegateCommand<string>(NavigateCommandExecute));

        private ICommand updateCommand;
        public ICommand UpdateCommand => updateCommand ?? (updateCommand = new DelegateCommand(UpdateCommandExecute));

        private void UpdateCommandExecute() {
            //Motor.
        }

        private readonly IRegionManager _regionManager;
        private readonly MotorService _motorService;

        public TestWindowViewModel(IRegionManager regionManager, MotorService motorService) {
            _regionManager = regionManager;
            _motorService = motorService;

            var motor = _motorService.Motors[0];
            _values = new ObservableCollection<double>();
            _values.Add(motor.Value);
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
