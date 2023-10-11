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
using System.Windows.Media;
using taskmaker_wpf.Domain;
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
            var input = _mapper.Map<MotorEntity>(state);

            _motorSrv.RemoveMotor(input);

            InvalidateMotors();
        }

        [RelayCommand]
        public void AddMotor() {
            _motorSrv.AddMotor();

            InvalidateMotors();
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

        public void Initialize() {
            if (MotorStates.Length == 0) {
                //_evaSrv.Initialize();
                _motorSrv.Initialize();
            }

            InvalidateMotors();
        }

        public void UpdateMotor(MotorState state) {
            var input = _mapper.Map<MotorEntity>(state);
            var output = _motorSrv.UpdateMotor(input);

            _mapper.Map(output, state);
        }
        public void UpdateMotorValue(MotorState state, double newValue) {
            var output = _motorSrv.UpdateMotorValue(state.Id, newValue);

            _mapper.Map(output, state);
        }

        public void InvalidateMotors() {
            var output = _motorSrv.InvalidateMotors();

            MotorStates = _mapper.Map<MotorState[]>(output);
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
