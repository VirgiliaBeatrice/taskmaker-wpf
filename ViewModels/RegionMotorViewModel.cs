using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Services;
using taskmaker_wpf.Views;

namespace taskmaker_wpf.ViewModels {
    [ObservableObject]
    public partial class MotorState {
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

        [ObservableProperty]
        private SolidColorBrush _color = Brushes.Black;
    }

    [ObservableObject]
    public partial class RegionMotorViewModel : INavigationAware {
        [RelayCommand]
        public void ListBoards() {
            EnumBoards();
        }

        [RelayCommand]
        public void RemoveMotor(MotorState state) {
            _motorSrv.Delete(state.Id);
        }

        [RelayCommand]
        public void AddMotor() {
            _motorSrv.Create(new MotorEntity());

            InvalidateMotors();
        }

        [RelayCommand]
        async public void Initialize() {
            var result = await WeakReferenceMessenger.Default.Send(new ShowMessageBoxMessage {
                Message = "Initialized all motors for experiments!",
                Caption = "Initialize",
                Button = MessageBoxButton.OK,
                Icon = MessageBoxImage.Information,
            });

            if (result == MessageBoxResult.OK) {
                InvalidateMotors();
            }
        }

        [ObservableProperty]
        private MotorState[] _motorStates = Array.Empty<MotorState>();

        [ObservableProperty]
        private ObservableCollection<int> _boardIds = new();

        [ObservableProperty]
        private ObservableCollection<int> _motorIds = new();

        [ObservableProperty]
        private bool _hasModified = false;
        [ObservableProperty]
        private bool _isInitialized = false;

        [ObservableProperty]
        private Brush[] _colors = new Brush[] {
            Brushes.Black,
            Brushes.Red,
            Brushes.Green,
            Brushes.Blue
        };

        private IRegionManager _regionManager;
        private readonly IMapper _mapper;
        //private readonly MotorInteractorBus _motorBus;
        private readonly IEventAggregator _ea;
        private readonly MotorService _motorSrv;
        private readonly EvaluationService _evaSrv;

        public RegionMotorViewModel(
            IRegionManager regionManager,
            //MotorInteractorBus motorBus,
            //IEnumerable<IUseCase> useCases,
            IEventAggregator ea,
            MapperConfiguration config,
            EvaluationService evaluation,
            MotorService motorSrv
            ) {
            _regionManager = regionManager;

            _mapper = config.CreateMapper();
            //_motorBus = motorBus;
            _ea = ea;
            _motorSrv = motorSrv;

            _evaSrv = evaluation;

            _ea.GetEvent<SystemLoadedEvent>().Subscribe(() => {
                InvalidateMotors();
            });

            InvalidateMotors();

            EnumBoards();
            EnumMotors();
        }

        //public void Initialize() {
        //    if (!IsInitialized) {
        //        _evaSrv.Initialize();
        //    }

        //    InvalidateMotors();
        //}

        public void UpdateMotor(MotorState state) {
            var motor = _mapper.Map<MotorEntity>(state);
            _motorSrv.Update(motor);

            _mapper.Map(motor, state);
        }
        //public void UpdateMotorValue(MotorState state, double newValue) {
        //    var motor = _mapper.Map<MotorEntity>(state);
        //    _motorSrv.UpdateMotorValue(ref motor, newValue);

        //    _mapper.Map(motor, state);
        //}

        public void InvalidateMotors() {
            //var output = _motorSrv.InvalidateMotors();

            //MotorStates = _mapper.Map<MotorState[]>(output);
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
