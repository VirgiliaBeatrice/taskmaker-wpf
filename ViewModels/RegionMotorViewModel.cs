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
    [ObservableObject]
    public partial class MotorState : IOutputPortState {
        [ObservableProperty]
        private double[] value;
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

        [ObservableProperty]
        private int id;
        [ObservableProperty]
        private bool isSelected;

        public object Clone() {
            return (MotorState)MemberwiseClone();
        }
    }

    [ObservableObject]
    public partial class RegionMotorViewModel : INavigationAware {
        [RelayCommand]
        public void ListBoards() {
            EnumBoards();
        }

        [RelayCommand]
        public void RemoveMotor(MotorState motor) {
            //_motorUseCase.RemoveMotor(_mapper.Map<MotorEntity>(motor));

            //var motors = _motorUseCase.GetMotors();
            //var stateMotors = motors.Select(e => _mapper.Map<MotorState>(e));

            //Motors.Clear();
            //Motors.AddRange(stateMotors);
        }

        [RelayCommand]
        public void AddMotor() {
            _motorBus.Handle(new AddMotorRequest(), out MotorEntity _);

            InvalidateMotors();
        }

        [ObservableProperty]
        private MotorState[] _motorStates = Array.Empty<MotorState>();

        [ObservableProperty]
        private ObservableCollection<int> _boardIds = new();

        [ObservableProperty]
        private ObservableCollection<int> _motorIds = new();

        private IRegionManager _regionManager;
        private readonly IMapper _mapper;
        private readonly MotorInteractorBus _motorBus;
        private readonly IEventAggregator _ea;

        public RegionMotorViewModel(
            IRegionManager regionManager,
            MotorInteractorBus motorBus,
            //IEnumerable<IUseCase> useCases,
            IEventAggregator ea,
            MapperConfiguration config) {
            _regionManager = regionManager;

            _mapper = config.CreateMapper();
            _motorBus = motorBus;
            _ea = ea;

            _ea.GetEvent<SystemLoadedEvent>().Subscribe(() => {
                InvalidateMotors();
            });

            InvalidateMotors();

            EnumBoards();
            EnumMotors();
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
        }

        private void EnumMotors() {
            var motors = Enumerable
                .Range(0, 4);

            MotorIds.Clear();
            BoardIds = new ObservableCollection<int>(motors);
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            InvalidateMotors();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
