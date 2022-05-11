using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels {
    public class BindableMotor : BindableBase {
        private int _motorId;
        public int MotorId {
            get => _motorId;
            set {
                SetProperty(ref _motorId, value);
                Instance.MotorId = _motorId;            
            } 
        }

        private int _boardId;
        public int BoardId {
            get => _boardId;
            set {
                SetProperty(ref _boardId, value);
                Instance.BoardId = _boardId;
            } 
        }

        private string _name;
        public string Name {
            get => _name;
            set {
                SetProperty(ref _name, value);
                Instance.Alias = _name;
            }
        }

        private int _value;
        public int Value {
            get => _value;
            set {
                SetProperty(ref _value, value);
                Instance.Value = _value;
            }
        }

        private int _min;
        public int Min {
            get => _min;
            set {
                SetProperty(ref _min, value);
                Instance.Min = _min;
            }
        }

        private int _max;
        public int Max {
            get => _max;
            set {
                SetProperty(ref _max, value);
                Instance.Max = _max;
            }
        }

        public Motor Instance { get => _instance; }

        private Motor _instance;

        public BindableMotor(Motor motor) {
            _motorId = motor.MotorId;
            _boardId = motor.BoardId;
            _name = motor.Alias;
            _value = motor.Value;
            _min = motor.Min;
            _max = motor.Max;

            _instance = motor;
        }
    }



    public class RegionMotorViewModel : BindableBase, INavigationAware {
        private ICommand connectMotorCmd;
        public ICommand ConnectMotorCmd => connectMotorCmd ?? (connectMotorCmd = new DelegateCommand<BindableMotor>(ConnectMotorCmdExecute));

        private void ConnectMotorCmdExecute(BindableMotor motor) {
            var instance = _serialSrv.GetMotorInstance(motor.BoardId, motor.MotorId);

            motor.Instance.Link(instance, motor.BoardId, motor.MotorId);
        }

        private ICommand removeMotorCmd;
        public ICommand RemoveMotorCmd => removeMotorCmd ?? (removeMotorCmd = new DelegateCommand<BindableMotor>(RemoveMotorCmdExecute));

        private void RemoveMotorCmdExecute(BindableMotor bMotor) {
            Motors.Remove(bMotor);
            _motorService.Motors.Remove(bMotor.Instance);
        }

        private ICommand addMotorCmd;
        public ICommand AddMotorCmd => addMotorCmd ?? (addMotorCmd = new DelegateCommand(AddMotorCmdExecute));

        private void AddMotorCmdExecute() {
            var newMotor = new Motor();
            var newBindableMotor = new BindableMotor(newMotor);

            _motorService.Motors.Add(newMotor);
            Motors.Add(newBindableMotor);
        }

        private ICommand setCmd;
        public ICommand SetCmd => setCmd ?? (setCmd = new DelegateCommand<string>(SetCmdExecute));

        private void SetCmdExecute(string text) {
            Console.WriteLine(text);
        }

        public ObservableCollection<BindableMotor> Motors { get; private set; }

        private IRegionManager _regionManager;
        private MotorService _motorService;
        private SerialService _serialSrv;

        public RegionMotorViewModel(
            IRegionManager regionManager,
            SerialService serialSrv,
            MotorService motorService) {
            _regionManager = regionManager;
            _motorService = motorService;
            _serialSrv = serialSrv;

            Motors = new ObservableCollection<BindableMotor>();
            _motorService.Motors.ForEach(e => {
                Motors.Add(new BindableMotor(e));
            });
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            //throw new NotImplementedException();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            //throw new NotImplementedException();
        }
    }
}
