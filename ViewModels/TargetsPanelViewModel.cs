using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Domain;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Models;
using taskmaker_wpf.Services;
using AutoMapper;
using System.Runtime.CompilerServices;
using System.Data.SqlClient;
using Prism.Events;

namespace taskmaker_wpf.ViewModels {
    public interface ISelectableState {
        bool IsSelected { get; set; }
        string Name { get; }
        int Id { get; }
        double[] Value { get; }
        event PropertyChangedEventHandler PropertyChanged;
    }

    public class MotorTargetState : MotorState, ISelectableState {
        protected BindableBase Parent { get; set; }
        private MotorInteractorBus _motorBus;

        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public override string ToString() {
            return Name;
        }
    }

    public class ControlUiTargetState : ControlUiState, ISelectableState {
        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public override string ToString() {
            return Name;
        }
    }

    public class NLinearMapState : BindableBase {
        private int _id;
        public int Id {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name;
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private (string, int)[] _targets;
        public (string, int)[] Targets {
            get => _targets;
            set => SetProperty(ref _targets, value);
        }

        public override string ToString() => Name;
    }

    public class TargetsPanelViewModel : BindableBase, IPresenter {
        private readonly ListTargetInteractor _target;
        private readonly NLinearMapInteractorBus _mapBus;
        private readonly MotorInteractorBus _motorBus;
        private readonly ControlUiInteractorBus _uiBus;
        //private readonly ListTargetUseCase _useCase;
        //private readonly NLinearMapUseCase _mapUseCase;

        private ISelectableState[] _validTargets = new ISelectableState[0];
        public ISelectableState[] ValidTargets {
            get => _validTargets.OrderBy(e => e.Name).ToArray();
            set => SetProperty(ref _validTargets, value);
        }

        public ISelectableState[] _targetsOfSelectedMap = new ISelectableState[0];
        public ISelectableState[] TargetsOfSelectedMap {
            get => _targetsOfSelectedMap;
            set => SetProperty(ref _targetsOfSelectedMap, value);
        }

        private ObservableCollection<NLinearMapState> _maps = new ObservableCollection<NLinearMapState>();
        public ObservableCollection<NLinearMapState> Maps {
            get => _maps;
            set => SetProperty(ref _maps, value);
        }

        private NLinearMapState _selectedMap;
        public NLinearMapState SelectedMap {
            get => _selectedMap;
            set {
                SetProperty(ref _selectedMap, value);


            }
        }

        private DelegateCommand _addCommand;
        public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(ExecuteAddCommand));
        private void ExecuteAddCommand() {
            _mapBus.Handle<AddNLinearMapRequest, bool>(new AddNLinearMapRequest(), (e) => { });
            _mapBus.Handle<ListNLinearMapRequest, NLinearMapEntity[]>(new ListNLinearMapRequest(),
                UpdateMaps);
        }

        private void UpdateMaps(NLinearMapEntity[] maps) {
            Maps.Clear();
            Maps.AddRange(maps.Select(e => _mapper.Map<NLinearMapState>(e)));
        }

        private DelegateCommand _updateCommand;
        public DelegateCommand UpdateCommand =>
            _updateCommand ?? (_updateCommand = new DelegateCommand(ExecuteUpdateCommand));
        void ExecuteUpdateCommand() {
            var targets = ValidTargets.Where(e => e.IsSelected)
                .Select(e => (e.GetType().Name.Replace("TargetState", ""), e.Id))
                .ToArray();
            var request = new UpdateNLinearMapRequest {
                MapId = SelectedMap?.Id ?? -1,
                RequestType = "Targets",
                Value = targets,
            };

            if (SelectedMap != null) {
                _mapBus.Handle(request, (bool res) => {
                    InvalidateMaps();

                    SelectedMap = Maps.Where(e => e.Id == request.MapId).FirstOrDefault();

                    TargetsOfSelectedMap = SelectedMap.Targets.Where(e => e.Item1 == "Motor")
                        .Select(e => e.Item2)
                        .Select(e => ValidTargets
                            .Where(e1 => e1.GetType() == typeof(MotorTargetState))
                            .Where(e1 => e1.Id == e).FirstOrDefault())
                        .ToArray();
                });

            }
        }

        private DelegateCommand<object> _updateMotorCommand;
        public DelegateCommand<object> UpdateMotorCommand => _updateMotorCommand ?? (_updateMotorCommand = new DelegateCommand<object>(ExecuteUpdateMotorCommand));

        public void ExecuteUpdateMotorCommand(object id) {
            var request = new UpdateMotorRequest {
                Id = (int)id,
                PropertyName = "Value",
                Value = TargetsOfSelectedMap[(int)id].Value,
            };

            _motorBus.Handle(request, (bool res) => { });
        }

        //private readonly MotorUseCase _motorUseCase;
        private readonly IMapper _mapper;
        public TargetsPanelViewModel(
            ListTargetInteractor target,
            MotorInteractorBus motorBus,
            ControlUiInteractorBus uiBus,
            NLinearMapInteractorBus mapBus,
            MapperConfiguration config) {
            _target = target;
            _motorBus = motorBus;
            _mapBus = mapBus;
            _uiBus = uiBus;

            _mapper = config.CreateMapper();

            InvalidateMaps();
            InvalidateTargets();

            //ValidTargets.OfType<MotorTargetState>().ToList().ForEach(e => e.PropertyChanged += (s, args) => {
            //    if (args.PropertyName == nameof(MotorTargetState.MotorValue))
            //        _motorUseCase.UpdateMotor(_mapper.Map<MotorEntity>(s));
            //});
        }

        private void InvalidateTargets() {
            _uiBus.Handle(new ListControlUiRequest(), (ControlUiEntity[] uis) => {
                _motorBus.Handle(new ListMotorRequest(), (MotorEntity[] motors) => {
                    var targets = new List<BaseEntity>();

                    targets = targets.Concat(uis).Concat(motors).ToList();

                    ValidTargets = targets.Select(e => {
                        if (e is MotorEntity) {
                            return _mapper.Map<MotorTargetState>(e);
                        }
                        else if (e is ControlUiEntity) {
                            return _mapper.Map<ControlUiTargetState>(e);
                        }
                        else
                            return default(ISelectableState);
                    }).ToArray();
                });
            });
            //_target.Handle<ListTargetRequest, ITargetable[]>(new ListTargetRequest(), targets => {
            //    ValidTargets = targets.Select(e => {
            //        if (e is MotorEntity) {
            //            return _mapper.Map<MotorTargetState>(e);
            //        }
            //        else if (e is ControlUiEntity) {
            //            return _mapper.Map<ControlUiTargetState>(e);
            //        }
            //        else
            //            return default(ISelectableState);
            //    }).ToArray();
            //});
        }

        private void InvalidateMaps() {
            _mapBus.Handle<ListNLinearMapRequest, NLinearMapEntity[]>(new ListNLinearMapRequest(), UpdateMaps);
        }
    }
}
