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
        private string _motorId;
        public string MotorId {
            get => _motorId;
            set {
                SetProperty(ref _motorId, value);
                //Parent.MotorId = _motorId;            
            } 
        }

        private string _boardId;
        public string BoardId {
            get => _boardId;
            set {
                SetProperty(ref _boardId, value);
                //Parent.BoardId = _boardId;
            } 
        }

        private string _name;
        public string Name {
            get => _name;
            set {
                SetProperty(ref _name, value);
                Parent.Name = _name;
            }
        }

        private int _value;
        public int Value {
            get => _value;
            set {
                SetProperty(ref _value, value);
                Parent.Value = _value;
            }
        }

        private int _min;
        public int Min {
            get => _min;
            set {
                SetProperty(ref _min, value);
                Parent.Min = _min;
            }
        }

        private int _max;
        public int Max {
            get => _max;
            set {
                SetProperty(ref _max, value);
                Parent.Max = _max;
            }
        }

        public Motor Parent { get => _parent; }

        private Motor _parent;

        public BindableMotor(Motor motor) {
            _motorId = "Motor" + motor.MotorId;
            _boardId = "Board" + motor.BoardId;
            _name = motor.Name;
            _value = (int)motor.Value;
            _min = motor.Min;
            _max = motor.Max;

            _parent = motor;
        }
    }



    public class RegionMotorViewModel : BindableBase, INavigationAware {
        private ICommand listBoardsCmd;
        public ICommand ListBoardsCmd => listBoardsCmd ?? (listBoardsCmd = new DelegateCommand(ListBoardsCmdExecute));

        private void ListBoardsCmdExecute() {
            ListBoards();
        }

        private ICommand listMotorsCmd;
        public ICommand ListMotorsCmd => listMotorsCmd ?? (listMotorsCmd = new DelegateCommand(ListMotorsCmdExecute));

        private void ListMotorsCmdExecute() {
            ListMotors();
        }

        private ICommand connectMotorCmd;
        public ICommand ConnectMotorCmd => connectMotorCmd ?? (connectMotorCmd = new DelegateCommand<BindableMotor>(ConnectMotorCmdExecute));

        private void ConnectMotorCmdExecute(BindableMotor motor) {
            throw new NotImplementedException();
            //var instance = _serialSrv.GetMotorInstance(motor.BoardId, motor.MotorId);

            //motor.Parent.Link(instance, motor.BoardId, motor.MotorId);
        }

        private ICommand removeMotorCmd;
        public ICommand RemoveMotorCmd => removeMotorCmd ?? (removeMotorCmd = new DelegateCommand<BindableMotor>(RemoveMotorCmdExecute));

        private void RemoveMotorCmdExecute(BindableMotor bMotor) {
            //Motors.Remove(bMotor);
        }

        private ICommand addMotorCmd;
        public ICommand AddMotorCmd => addMotorCmd ?? (addMotorCmd = new DelegateCommand(AddMotorCmdExecute));

        private void AddMotorCmdExecute() {
            var newMotor = new Motor();
            var newBindableMotor = new BindableMotor(newMotor);

            //Motors.Add(newBindableMotor);
        }

        private ICommand setCmd;
        public ICommand SetCmd => setCmd ?? (setCmd = new DelegateCommand<string>(SetCmdExecute));

        private void SetCmdExecute(string text) {
            Console.WriteLine(text);
        }

        public ObservableCollection<Motor> Motors => _systemSvr.Motors;

        private ObservableCollection<string> _boardIds = new ObservableCollection<string>();
        public ObservableCollection<string> BoardIds {
            get => _boardIds;
            set => SetProperty(ref _boardIds, value);
        }

        private ObservableCollection<string> _motorIds = new ObservableCollection<string>();
        public ObservableCollection<string> MotorIds {
            get => _motorIds;
            set => SetProperty(ref _motorIds, value);
        }

        private IRegionManager _regionManager;
        private SerialService _serialSrv;
        private readonly SystemService _systemSvr;

        public RegionMotorViewModel(
            IRegionManager regionManager,
            SerialService serialSrv,
            SystemService systemSvr) {
            _regionManager = regionManager;
            _serialSrv = serialSrv;
            _systemSvr = systemSvr;

            //Motors = new ObservableCollection<Motor>();
            //MotorIds = new ObservableCollection<string>();

            ListBoards();
            ListMotors();
        }

        private void ListBoards() {
            var boards = Enumerable
                .Range(0, 7)
                .Select(e => $"Board{e}");

            BoardIds.Clear();
            BoardIds.AddRange(boards);
        }

        private void ListMotors() {
            var motors = Enumerable
                .Range(0, 4)
                .Select(e => $"Motor{e}");

            MotorIds.Clear();
            MotorIds.AddRange(motors);
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
