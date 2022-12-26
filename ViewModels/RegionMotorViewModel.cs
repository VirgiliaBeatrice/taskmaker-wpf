using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
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
    public partial class MotorState : ObservableObject, IOutputPortState {
        [ObservableProperty]
        private double[] _value;
        [ObservableProperty]
        private string name;
        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private int max;
        [ObservableProperty]
        private int min;
        [ObservableProperty]
        private int nuibotBoardId;
        [ObservableProperty]
        private int nuibotMotorId;

        public int Id { get; set; }
        public bool IsSelected { get; set; }

        public object Clone() {
            return (MotorState)MemberwiseClone();
        }
    }

    public partial class RegionMotorViewModel : BindableBase, INavigationAware {

        //private ICommand listBoardsCmd;
        //public ICommand ListBoardsCmd => listBoardsCmd ??= new DelegateCommand(ListBoardsCmdExecute);

        [RelayCommand]
        private void ListBoardsCmd() {
            EnumBoards();
        }

        private ICommand listMotorsCmd;
        public ICommand ListMotorsCmd => listMotorsCmd ??= new DelegateCommand(ListMotorsCmdExecute);

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
            InvalidateMotors();
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

        private MotorState[] _motorStates = new MotorState[0];
        public MotorState[] MotorStates {
            get => _motorStates;
            set => SetProperty(ref _motorStates, value);
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
                InvalidateMotors();
            });

            InvalidateMotors();
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
            //_motorBus.Handle(new AddMotorRequest(), (bool res) => { });
            _motorBus.Handle(new AddMotorRequest(), out MotorEntity motor);

            InvalidateMotors();
        }

        public void UpdateMotor(MotorState state) {
            var req = new UpdateMotorRequest {
                Id = state.Id,
                Value = _mapper.Map<MotorEntity>(state)
            };

            _motorBus.Handle(req, out MotorEntity motor);

            var target = MotorStates.Where(e => e.Id == motor.Id).FirstOrDefault();

            _mapper.Map(motor, target);
        }

        private void InvalidateMotors() {
            _motorBus.Handle(new ListMotorRequest(), out MotorEntity[] motors);

            MotorStates = _mapper.Map<MotorState[]>(motors);

            //_motorBus.Handle(new ListMotorRequest(), (MotorEntity[] motors) => {
            //    MotorStates.ToList()
            //        .ForEach(e => e.PropertyChanged -= Motor_PropertyChanged);
            //    MotorStates.Clear();

            //    MotorStates.AddRange(motors.Select(e => _mapper.Map<MotorState>(e)));
            //    MotorStates.ToList()
            //        .ForEach(e => e.PropertyChanged += Motor_PropertyChanged);
            //});
        }

        public void UpdateMotorValue(MotorState state, double newValue) {
            var req = new UpdateMotorRequest {
                Id = state.Id,
                PropertyName = "MotorValue",
                Value = new double[] { newValue },
            };

            _motorBus.Handle(req, out MotorEntity motor);

            var target = MotorStates.Where(e => e.Id == motor.Id).FirstOrDefault();

            _mapper.Map(motor, target);
            //target.Value = motor.Value;
        }


        private void EnumBoards() {
            var boards = Enumerable
                .Range(0, 8);

            BoardIds.Clear();
            BoardIds = new ObservableCollection<int>(boards);
            //BoardIds.AddRange(boards);
        }

        private void EnumMotors() {
            var motors = Enumerable
                .Range(0, 4);

            MotorIds.Clear();
            BoardIds = new ObservableCollection<int>(motors);
            //MotorIds.AddRange(motors);
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            InvalidateMotors();
            //throw new NotImplementedException();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            //throw new NotImplementedException();
        }
    }
}
