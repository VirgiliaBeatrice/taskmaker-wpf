using AutoMapper;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using taskmaker_wpf.Domain;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Services;
using taskmaker_wpf.Views;

namespace taskmaker_wpf.ViewModels {
    public class MotorState_v1 : IOutputPort {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double[] Value { get; set; }
        public int Max { get; set; }
        public int Min { get; set; }
        public int NuibotBoardId { get; set; }
        public int NuibotMotorId { get; set; }
        public bool IsSelected { get; set; }

        public object Clone() {
            return (MotorState_v1)MemberwiseClone();
        }
    }

        public class MotorState : BindableBase {
        private int id;
        private string name;
        //private double[] _value;
        private double _motorValue;
        private int max;
        private int min;
        private int boardId;
        private int motorId;

        public int Id { get => id; set => SetProperty(ref id, value); }
        public string Name { get => name; set => SetProperty(ref name, value); }
        public double[] Value => new double[] { MotorValue };
        public double MotorValue { get => _motorValue; set => SetProperty(ref _motorValue, value); }
        public int Max { get => max; set => SetProperty(ref max, value); }
        public int Min { get => min; set => SetProperty(ref min, value); }
        public int BoardId { get => boardId; set => SetProperty(ref boardId, value); }
        public int MotorId { get => motorId; set => SetProperty(ref motorId, value); }

    }


    public class RegionMotorViewModel : BindableBase, INavigationAware {
        private ICommand listBoardsCmd;
        public ICommand ListBoardsCmd => listBoardsCmd ?? (listBoardsCmd = new DelegateCommand(ListBoardsCmdExecute));

        private void ListBoardsCmdExecute() {
            EnumBoards();
        }

        private ICommand listMotorsCmd;
        public ICommand ListMotorsCmd => listMotorsCmd ?? (listMotorsCmd = new DelegateCommand(ListMotorsCmdExecute));

        private void ListMotorsCmdExecute() {
            EnumMotors();
        }

        private DelegateCommand<MotorState> connectMotorCmd;
        public DelegateCommand<MotorState> ConnectMotorCmd => connectMotorCmd ?? (connectMotorCmd = new DelegateCommand<MotorState>(ConnectMotorCmdExecute));

        private void ConnectMotorCmdExecute(MotorState motor) {
            //_motorUseCase.UpdateMotor(_mapper.Map<MotorEntity>(motor));
        }

        private ICommand removeMotorCmd;
        public ICommand RemoveMotorCmd => removeMotorCmd ?? (removeMotorCmd = new DelegateCommand<MotorState>(RemoveMotorCmdExecute));

        private void RemoveMotorCmdExecute(MotorState motor) {
            //_motorUseCase.RemoveMotor(_mapper.Map<MotorEntity>(motor));

            //var motors = _motorUseCase.GetMotors();
            //var stateMotors = motors.Select(e => _mapper.Map<MotorState>(e));

            //Motors.Clear();
            //Motors.AddRange(stateMotors);
        }

        private ICommand addMotorCmd;
        public ICommand AddMotorCmd => addMotorCmd ?? (addMotorCmd = new DelegateCommand(AddMotorCmdExecute));

        private void AddMotorCmdExecute() {
            AddMotor();
            RefreshMotors();
            //_motorUseCase.AddMotor();

            //var motors = _motorUseCase.GetMotors();
            //var stateMotors = motors.Select(e => _mapper.Map<MotorState>(e));

            //Motors.Clear();
            //Motors.AddRange(stateMotors);

            //Motors.ToList()
            //    .ForEach(e => e.PropertyChanged += (s, args) => {
            //        _motorUseCase.UpdateMotor(_mapper.Map<MotorEntity>(s as MotorState));
            //    });
        }

        private ICommand setCmd;
        public ICommand SetCmd => setCmd ?? (setCmd = new DelegateCommand<string>(SetCmdExecute));

        private void SetCmdExecute(string text) {
            Console.WriteLine(text);
        }

        private ObservableCollection<MotorState> _motors = new ObservableCollection<MotorState>();
        public ObservableCollection<MotorState> Motors {
            get => _motors;
            set => SetProperty(ref _motors, value);
        }

        private ObservableCollection<int> _boardIds = new ObservableCollection<int>();
        public ObservableCollection<int> BoardIds {
            get => _boardIds;
            set => SetProperty(ref _boardIds, value);
        }

        private ObservableCollection<int> _motorIds = new ObservableCollection<int>();
        public ObservableCollection<int> MotorIds {
            get => _motorIds;
            set => SetProperty(ref _motorIds, value);
        }

        private IRegionManager _regionManager;
        private SerialService _serialSrv;
        private readonly IMapper _mapper;
        //private readonly MotorUseCase _motorUseCase;
        private readonly MotorInteractorBus _motorBus;
        private readonly IEventAggregator _ea;

        public RegionMotorViewModel(
            IRegionManager regionManager,
            MotorInteractorBus motorBus,
            //IEnumerable<IUseCase> useCases,
            IEventAggregator ea,
            MapperConfiguration config,
            SerialService serialSrv) {
            _regionManager = regionManager;
            _serialSrv = serialSrv;

            _mapper = config.CreateMapper();
            _motorBus = motorBus;
            _ea = ea;

            _ea.GetEvent<SystemLoadedEvent>().Subscribe(() => {
                RefreshMotors();
            });

            RefreshMotors();
            //_motorBus.Handle(new ListMotorRequest(), (MotorEntity[] motors) => {
            //    Motors.Clear();
            //    Motors.AddRange(motors.Select(e => _mapper.Map<MotorState>(e)));
            //});
            //_motorUseCase = useCases.OfType<MotorUseCase>().First();
            ////Motors.AddRange(_motorAgent.Repository.Select(e => new StatefulMotor(e)));
            //Motors.AddRange(_motorUseCase.GetMotors().Select(e => _mapper.Map<MotorState>(e)));
            //Motors.ToList().ForEach(e => e.PropertyChanged += Motor_PropertyChanged);


            //foreach(var motor in Motors) {
            //    motor.PropertyChanged += (s, args) => {
            //        _motorUseCase.UpdateMotor(_mapper.Map<MotorEntity>(s as MotorState));
            //    };
            //}

            EnumBoards();
            EnumMotors();
        }

        private void AddMotor() {
            _motorBus.Handle(new AddMotorRequest(), (bool res) => { });
        }

        private void RefreshMotors() {
            _motorBus.Handle(new ListMotorRequest(), (MotorEntity[] motors) => {
                Motors.ToList()
                    .ForEach(e => e.PropertyChanged -= Motor_PropertyChanged);
                Motors.Clear();

                Motors.AddRange(motors.Select(e => _mapper.Map<MotorState>(e)));
                Motors.ToList()
                    .ForEach(e => e.PropertyChanged += Motor_PropertyChanged);
            });
        }

        private void Motor_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var prop = typeof(MotorState).GetProperty(e.PropertyName);

            var req = new UpdateMotorRequest {
                Id = ((MotorState)sender).Id,
                PropertyName = e.PropertyName,
                ValueType = prop.PropertyType.Name,
                Value = prop.GetValue(sender)
            };

            // TODO
            if (e.PropertyName == "MotorValue") {
                req.PropertyName = "Value";
                req.Value = ((MotorState)sender).Value;
            }

            _motorBus.Handle(req, (bool res) => { });
        }

        private void EnumBoards() {
            var boards = Enumerable
                .Range(0, 8);

            BoardIds.Clear();
            BoardIds.AddRange(boards);
        }

        private void EnumMotors() {
            var motors = Enumerable
                .Range(0, 4);

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
