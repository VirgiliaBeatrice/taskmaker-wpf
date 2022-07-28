using AutoMapper;
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
using taskmaker_wpf.Domain;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels {
    public class StatefulMotor : BindableBase {
        private Motor _ref;

        private string _name;
        public string Name {
            get => _name;
            set {
                SetProperty(ref _name, value);
                _ref.Name = _name;
            }
        }

        private double _value;
        public double Value {
            get => _value;
            set {
                SetProperty(ref _value, value);
                _ref.Value = _value;
            }
        }

        private int _min;
        public int Min {
            get => _min;
            set {
                SetProperty(ref _min, value);
                _ref.Min = _min;
            }
        }

        private int _max;
        public int Max {
            get => _max;
            set {
                SetProperty(ref _min, value);
                _ref.Max = _max;
            }
        }

        private int _boardId;
        public int BoardId {
            get => _boardId;
            set {
                SetProperty(ref _boardId, value);
                _ref.BoardId = _boardId;
            }
        }

        private int _motorId;
        public int MotorId {
            get => _motorId;
            set {
                SetProperty(ref _motorId, value);
                _ref.MotorId = _motorId;
            }
        }

        public StatefulMotor(Motor refMotor) {
            _ref = refMotor;

            Name = _ref.Name;
            Value = _ref.Value;
            Min = _ref.Min;
            Max = _ref.Max;
            BoardId = _ref.BoardId;
            MotorId = _ref.MotorId;
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

        private DelegateCommand<StatefulMotor> connectMotorCmd;
        public DelegateCommand<StatefulMotor> ConnectMotorCmd => connectMotorCmd ?? (connectMotorCmd = new DelegateCommand<StatefulMotor>(ConnectMotorCmdExecute));

        private void ConnectMotorCmdExecute(StatefulMotor motor) {
            throw new NotImplementedException();
            //var instance = _serialSrv.GetMotorInstance(motor.BoardId, motor.MotorId);

            //motor.Parent.Link(instance, motor.BoardId, motor.MotorId);
        }

        private ICommand removeMotorCmd;
        public ICommand RemoveMotorCmd => removeMotorCmd ?? (removeMotorCmd = new DelegateCommand<StatefulMotor>(RemoveMotorCmdExecute));

        private void RemoveMotorCmdExecute(StatefulMotor bMotor) {
            //Motors.Remove(bMotor);
        }

        private ICommand addMotorCmd;
        public ICommand AddMotorCmd => addMotorCmd ?? (addMotorCmd = new DelegateCommand(AddMotorCmdExecute));

        private void AddMotorCmdExecute() {
            var motor = _motorAgent.AddMotor();
            var sMotor = new StatefulMotor(motor);

            Motors.Add(sMotor);
        }

        private ICommand setCmd;
        public ICommand SetCmd => setCmd ?? (setCmd = new DelegateCommand<string>(SetCmdExecute));

        private void SetCmdExecute(string text) {
            Console.WriteLine(text);
        }

        private ObservableCollection<StatefulMotor> _motors = new ObservableCollection<StatefulMotor>();
        public ObservableCollection<StatefulMotor> Motors {
            get => _motors;
            set => SetProperty(ref _motors, value);
        }

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
        private readonly IMapper _mapper;
        public RegionMotorViewModel(
            IRegionManager regionManager,
            IUseCase useCase,
            MapperConfiguration config,
            SerialService serialSrv,
            SystemService systemSvr) {
            _regionManager = regionManager;
            _serialSrv = serialSrv;
            _systemSvr = systemSvr;

            _mapper = config.CreateMapper();

            //Motors.AddRange(_motorAgent.Repository.Select(e => new StatefulMotor(e)));

            Motors.AddRange();

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
