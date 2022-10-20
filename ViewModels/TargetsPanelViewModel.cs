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
        event PropertyChangedEventHandler PropertyChanged;

        int Id { get; }
        bool IsSelected { get; set; }
        string Name { get; }
        double[] Value { get; }
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

    public class MotorTargetState : MotorState, ISelectableState {
        private bool _isSelected;
        private MotorInteractorBus _motorBus;
        protected BindableBase Parent { get; set; }
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
        private string _name;

        private (string, int)[] _targets;

        public int Id {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public (string, int)[] Targets {
            get => _targets;
            set => SetProperty(ref _targets, value);
        }

        public override string ToString() => Name;
    }

    public class TargetsPanelViewModel : BindableBase, IPresenter {
        private readonly NLinearMapInteractorBus _mapBus;
        //private readonly MotorUseCase _motorUseCase;
        private readonly IMapper _mapper;

        private readonly MotorInteractorBus _motorBus;
        private readonly ListTargetInteractor _target;
        private readonly ControlUiInteractorBus _uiBus;
        //private readonly ListTargetUseCase _useCase;
        //private readonly NLinearMapUseCase _mapUseCase;

        private DelegateCommand _addCommand;
        private ObservableCollection<NLinearMapState> _maps = new ObservableCollection<NLinearMapState>();
        private NLinearMapState _selectedMap;
        private DelegateCommand _updateCommand;
        private DelegateCommand<object> _updateMotorCommand;
        private ISelectableState[] _validTargets = new ISelectableState[0];
        public ISelectableState[] _targetsOfSelectedMap = new ISelectableState[0];

        public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(ExecuteAddCommand));

        //public NLinearMapState Map => Parent.MapState;

        //public ObservableCollection<NLinearMapState> Maps {
        //    get => _maps;
        //    set => SetProperty(ref _maps, value);
        //}

        //public NLinearMapState SelectedMap {
        //    get => _selectedMap;
        //    set {
        //        SetProperty(ref _selectedMap, value);


        //    }
        //}

        public RegionControlUIViewModel Parent { get; set; }

        public ISelectableState[] TargetsOfSelectedMap {
            get => _targetsOfSelectedMap;
            set => SetProperty(ref _targetsOfSelectedMap, value);
        }

        public DelegateCommand UpdateCommand =>
            _updateCommand ?? (_updateCommand = new DelegateCommand(ExecuteUpdateCommand));

        public DelegateCommand<object> UpdateMotorCommand => _updateMotorCommand ?? (_updateMotorCommand = new DelegateCommand<object>(ExecuteUpdateMotorCommand));

        public ISelectableState[] ValidTargets {
            get => _validTargets.OrderBy(e => e.Name).ToArray();
            set => SetProperty(ref _validTargets, value);
        }
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

        private void ExecuteAddCommand() {
            _mapBus.Handle<AddNLinearMapRequest, bool>(new AddNLinearMapRequest(), (e) => { });
            _mapBus.Handle<ListNLinearMapRequest, NLinearMapEntity[]>(new ListNLinearMapRequest(),
                UpdateMaps);
        }

        void ExecuteUpdateCommand() {
            var targets = ValidTargets.Where(e => e.IsSelected)
                .Select(e => (e.GetType().Name.Replace("TargetState", ""), e.Id))
                .ToArray();
            var request = new UpdateNLinearMapRequest {
                MapId = Parent.MapState?.Id ?? -1,
                RequestType = "Targets",
                Value = targets,
            };

            if (Parent.MapState != null) {
                _mapBus.Handle(request, (bool res) => {
                    InvalidateMaps();

                    Parent.MapState = Parent.Maps.Where(e => e.Id == request.MapId).FirstOrDefault();

                    TargetsOfSelectedMap = Parent.MapState.Targets.Where(e => e.Item1 == "Motor")
                        .Select(e => e.Item2)
                        .Select(e => ValidTargets
                            .Where(e1 => e1.GetType() == typeof(MotorTargetState))
                            .Where(e1 => e1.Id == e).FirstOrDefault())
                        .ToArray();
                });

            }
        }

        private void InvalidateMaps() {
            _mapBus.Handle<ListNLinearMapRequest, NLinearMapEntity[]>(new ListNLinearMapRequest(), UpdateMaps);
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

        private void UpdateMaps(NLinearMapEntity[] maps) {
            if (Parent != null)
                Parent.Maps = maps.Select(e => _mapper.Map<NLinearMapState>(e)).ToArray();
        }
        public TargetsPanelViewModel Bind(RegionControlUIViewModel parent) {
            Parent = parent;
            return this;
        }

        public void ExecuteUpdateMotorCommand(object id) {
            var request = new UpdateMotorRequest {
                Id = (int)id,
                PropertyName = "Value",
                Value = TargetsOfSelectedMap[(int)id].Value,
            };

            _motorBus.Handle(request, (bool res) => { });
        }
    }
}
