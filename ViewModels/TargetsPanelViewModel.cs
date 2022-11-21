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
using SkiaSharp;

namespace taskmaker_wpf.ViewModels {
    public interface ISelectableState {
        int Id { get; }
        bool IsSelected { get; set; }
        string Name { get; }
        double[] Value { get; }
    }

    public class ControlUiTargetState : ControlUiState, IInputPort, IOutputPort {
        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public override string ToString() {
            return Name;
        }

        public object Clone() {
            return (ControlUiTargetState)MemberwiseClone();
        }
    }

    public class MotorTargetState : MotorState, IOutputPort {
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public object Clone() {
            return (MotorTargetState)MemberwiseClone();
        }

        public override string ToString() {
            return Name;
        }
    }
    public class NLinearMapState : BindableBase {
        private int _id;
        private string _name;

        private IInputPort[] _inputs;
        private IOutputPort[] _outputs;

        public int Id {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public IInputPort[] Inputs {
            get => _inputs;
            set => SetProperty(ref _inputs, value);
        }
        public IOutputPort[] Outputs { get => _outputs; set => SetProperty(ref _outputs, value); }

        public override string ToString() => Name;
    }

    //public class TargetsPanelViewModel : BindableBase, IPresenter {
    //    private readonly NLinearMapInteractorBus _mapBus;
    //    //private readonly MotorUseCase _motorUseCase;
    //    private readonly IMapper _mapper;

    //    private readonly MotorInteractorBus _motorBus;
    //    private readonly ListTargetInteractor _target;
    //    private readonly ControlUiInteractorBus _uiBus;
    //    //private readonly ListTargetUseCase _useCase;
    //    //private readonly NLinearMapUseCase _mapUseCase;

    //    private DelegateCommand _addCommand;
    //    private ObservableCollection<NLinearMapState> _maps = new ObservableCollection<NLinearMapState>();
    //    private NLinearMapState _selectedMap;
    //    private DelegateCommand<object> _updateCommand;
    //    private DelegateCommand<object> _updateMotorCommand;
    //    private ISelectableState[] _validTargets = new ISelectableState[0];
    //    public ISelectableState[] _targetsOfSelectedMap = new ISelectableState[0];

    //    private MotorTargetState[] _motorStates;
    //    private ControlUiTargetState[] _uiStates;

    //    public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(ExecuteAddCommand));

    //    //public NLinearMapState Map => Parent.MapState;

    //    //public ObservableCollection<NLinearMapState> Maps {
    //    //    get => _maps;
    //    //    set => SetProperty(ref _maps, value);
    //    //}

    //    //public NLinearMapState SelectedMap {
    //    //    get => _selectedMap;
    //    //    set {
    //    //        SetProperty(ref _selectedMap, value);


    //    //    }
    //    //}

    //    private IOutputPort[] _outputPorts;
    //    public IOutputPort[] OutputPorts {
    //        get => _outputPorts;
    //        set => SetProperty(ref _outputPorts, value);
    //    }


    //    private IInputPort[] selectedInputPorts = new IInputPort[0];
    //    public IInputPort[] SelectedInputPorts {
    //        get { return selectedInputPorts; }
    //        set { SetProperty(ref selectedInputPorts, value); }
    //    }

    //    private IInputPort[] _inputPorts;
    //    public IInputPort[] InputPorts {
    //        get { return _inputPorts; }
    //        set { SetProperty(ref _inputPorts, value); }
    //    }

    //    public NLinearMapState MapState {
    //        get; set;
    //    }

    //    public RegionControlUIViewModel Parent { get; set; }

    //    public ISelectableState[] TargetsOfSelectedMap {
    //        get => _targetsOfSelectedMap;
    //        set => SetProperty(ref _targetsOfSelectedMap, value);
    //    }

    //    private MotorTargetState[] _targetMotors;
    //    public MotorTargetState[] TargetMotors {
    //        get => _targetMotors;
    //        set => SetProperty(ref _targetMotors, value);
    //    }

    //    public DelegateCommand<object> UpdateCommand =>
    //        _updateCommand ?? (_updateCommand = new DelegateCommand<object>(ExecuteUpdateCommand));

    //    public DelegateCommand<object> UpdateMotorCommand => _updateMotorCommand ?? (_updateMotorCommand = new DelegateCommand<object>(ExecuteUpdateMotorCommand));

    //    public ISelectableState[] ValidTargets {
    //        get => _validTargets.OrderBy(e => e.Name).ToArray();
    //        set => SetProperty(ref _validTargets, value);
    //    }
    //    public TargetsPanelViewModel(
    //        RegionControlUIViewModel parent,
    //        ListTargetInteractor target,
    //        MotorInteractorBus motorBus,
    //        ControlUiInteractorBus uiBus,
    //        NLinearMapInteractorBus mapBus,
    //        MapperConfiguration config) {
    //        Parent = parent;
            
    //        _target = target;
    //        _motorBus = motorBus;
    //        _mapBus = mapBus;
    //        _uiBus = uiBus;

    //        _mapper = config.CreateMapper();

    //        InvalidateMaps();
    //        //InvalidateTargets();
    //        //InvalidateTargets();
    //        //InvalidateTargetsNew();

    //        //ValidTargets.OfType<MotorTargetState>().ToList().ForEach(e => e.PropertyChanged += (s, args) => {
    //        //    if (args.PropertyName == nameof(MotorTargetState.MotorValue))
    //        //        _motorUseCase.UpdateMotor(_mapper.Map<MotorEntity>(s));
    //        //});
    //    }

    //    private void ExecuteAddCommand() {
    //        var request = new AddNLinearMapRequest();
    //        var request1 = new ListNLinearMapRequest();

    //        _mapBus.Handle(request, (bool res) => { });
    //        _mapBus.Handle(request1, (NLinearMapEntity[] maps) => {
    //            Parent.Maps = _mapper.Map<NLinearMapState[]>(maps);
    //        });

    //        //InvalidateTargets();
    //    }

    //    void ExecuteUpdateCommand(object param) {
    //        var inputs = InputPorts.Where(e => e.IsSelected).ToArray();
    //        var request = new UpdateNLinearMapRequest {
    //            Id = Parent.MapState.Id,
    //            PropertyType = "UpdateInputs",
    //            PropertyValue = inputs
    //        };

    //        _mapBus.Handle(request, (NLinearMapEntity map) => {
    //            Parent.MapState = _mapper.Map<NLinearMapState>(map);
    //        });

    //        var outputs = OutputPorts.Where(e => e.IsSelected).ToArray();
    //        request = new UpdateNLinearMapRequest {
    //            Id = Parent.MapState.Id,
    //            PropertyType = "UpdateOutputs",
    //            PropertyValue = outputs
    //        };

    //        _mapBus.Handle(request, (NLinearMapEntity map) => {
    //            Parent.MapState = _mapper.Map<NLinearMapState>(map);
    //        });

    //        TargetMotors = Parent.MapState.Outputs
    //            .Where(e => e.GetType().Name.Contains("Motor"))
    //            .Cast<MotorTargetState>()
    //            .ToArray();


    //        //if (param is string mode) {
    //        //    if (mode == "UpdateInputs") {
    //        //        var inputs = InputPorts.Where(e => e.IsSelected).ToArray();
    //        //        var request = new UpdateNLinearMapRequest {
    //        //            Id = Parent.MapState.Id,
    //        //            PropertyType = "UpdateInputs",
    //        //            PropertyValue = inputs
    //        //        };

    //        //        _mapBus.Handle(request, (NLinearMapEntity map) => {
    //        //            Parent.MapState = _mapper.Map<NLinearMapState>(map);
    //        //        });
    //        //    }
    //        //    else if (mode == "UpdateOutputs") {
    //        //        var outputs = OutputPorts.Where(e => e.IsSelected).ToArray();
    //        //        var request = new UpdateNLinearMapRequest {
    //        //            Id = Parent.MapState.Id,
    //        //            PropertyType = "UpdateOutputs",
    //        //            PropertyValue = outputs
    //        //        };

    //        //        _mapBus.Handle(request, (NLinearMapEntity map) => {
    //        //            Parent.MapState = _mapper.Map<NLinearMapState>(map);
    //        //        });

    //        //        TargetMotors = Parent.MapState.Outputs
    //        //            .Where(e => e.GetType().Name.Contains("Motor"))
    //        //            .Cast<MotorTargetState>()
    //        //            .ToArray();
    //        //    }
    //        //    else if (mode == "Initialize") {
    //        //        var request = new UpdateNLinearMapRequest {
    //        //            Id = Parent.MapState.Id,
    //        //            PropertyType = "Initialize",
    //        //        };

    //        //        _mapBus.Handle(request, (NLinearMapEntity map) => {
    //        //            Parent.MapState = _mapper.Map<NLinearMapState>(map);
    //        //        });
    //        //    }


    //            //InvalidateTargets();
    //        }
    //        //var targets = ValidTargets.Where(e => e.IsSelected)
    //        //    .Select(e => (e.GetType().Name.Replace("TargetState", ""), e.Id))
    //        //    .ToArray();
    //        //var targets = ValidTargets.Where(e => e.IsSelected)
    //        //    .Select(e => new TargetEntity { Id = e.Id, Name = e.Name })
    //        //    .ToArray();
    //        ////var request = new UpdateNLinearMapRequest {
    //        ////    MapId = Parent.MapState?.Id ?? -1,
    //        ////    RequestType = "Targets",
    //        ////    Value = targets,
    //        ////};

    //        //var request = new UpdateControlUiRequest {
    //        //    Id = Parent.UiState?.Id ?? -1,
    //        //    PropertyName = "UpdateTargets",
    //        //    PropertyValue = targets,
    //        //};

    //        //if (Parent.UiState != null) {
    //        //    _uiBus.Handle(request, (ControlUiEntity ui) => {
    //        //        Parent.UiState = _mapper.Map<ControlUiState>(ui);

    //        //        TargetsOfSelectedMap = Parent.UiState.Targets.Where(e => e.Name.Contains("Motor"))
    //        //            .Select(e => e.Id)
    //        //            .Select(e => ValidTargets
    //        //                .Where(e1 => e1.GetType() == typeof(MotorTargetState))
    //        //                .Where(e1 => e1.Id == e).FirstOrDefault())
    //        //            .ToArray();
    //        //    });
    //        //}


    //        //if (Parent.MapState != null) {
    //        //    _mapBus.Handle(request, (bool res) => {
    //        //        InvalidateMaps();

    //        //        Parent.MapState = Parent.Maps.Where(e => e.Id == request.MapId).FirstOrDefault();

    //        //        TargetsOfSelectedMap = Parent.MapState.Targets.Where(e => e.Item1 == "Motor")
    //        //            .Select(e => e.Item2)
    //        //            .Select(e => ValidTargets
    //        //                .Where(e1 => e1.GetType() == typeof(MotorTargetState))
    //        //                .Where(e1 => e1.Id == e).FirstOrDefault())
    //        //            .ToArray();
    //        //    });

    //        //}
    //    //}

    //    public void InvalidateMap() {
    //        var request = new CreateNLinearMapRequest {
    //            Id = Parent.UiState.Id,
    //        };

    //        _mapBus.Handle(request, (NLinearMapEntity map) => {
    //            if (map != null) {
    //                Parent.MapState = _mapper.Map<NLinearMapState>(map);
    //            }
    //        });
    //    }


    //    private void InvalidateMaps() {
    //        _mapBus.Handle<ListNLinearMapRequest, NLinearMapEntity[]>(new ListNLinearMapRequest(), UpdateMaps);
    //    }

    //    private void InvalidateUis() {
    //        var request = new ListControlUiRequest();

    //        _uiBus.Handle(request, (ControlUiEntity[] uis) => {
    //            _uiStates = _mapper.Map<ControlUiTargetState[]>(uis);
    //        });
    //    }

    //    private void InvalidateMotors() {
    //        var request = new ListMotorRequest();

    //        _motorBus.Handle(request, (MotorEntity[] motors) => {
    //            _motorStates = _mapper.Map<MotorTargetState[]>(motors);
    //        });
    //    }

    //    public void InvalidateTargets() {
    //        InvalidateUis();
    //        InvalidateMotors();

    //        InputPorts = new List<IInputPort>()
    //            .Concat(_uiStates)
    //            .Select(e => (IInputPort)e.Clone())
    //            .ToArray();
    //        OutputPorts = new List<IOutputPort>()
    //            .Concat(_uiStates)
    //            .Concat(_motorStates)
    //            .Select(e => (IOutputPort)e.Clone())
    //            .ToArray();

    //        InputPorts.Where(e => Parent.MapState.Inputs.Any(e1 => e.Name == e1.Name)).ToList().ForEach(e => e.IsSelected = true);
    //        OutputPorts.Where(e => Parent.MapState.Outputs.Any(e1 => e.Name == e1.Name)).ToList().ForEach(e => e.IsSelected = true);

    //        SelectedInputPorts = InputPorts.Where(e => e.IsSelected).ToArray();

    //        //ValidTargets = new List<ISelectableState>().Concat(_uiStates).Concat(_motorStates).ToArray();

    //        //var targets = Parent.UiState.Targets;

    //        //foreach(var item in targets) {
    //        //    var target = ValidTargets.Where(e => e.GetType().Name.Contains(item.Name) & e.Id == item.Id).FirstOrDefault();

    //        //    if (target != null)
    //        //        target.IsSelected = true;
    //        //}

    //    }

    //    //private void InvalidateTargets() {
    //    //    _uiBus.Handle(new ListControlUiRequest(), (ControlUiEntity[] uis) => {
    //    //        _motorBus.Handle(new ListMotorRequest(), (MotorEntity[] motors) => {
    //    //            var targets = new List<BaseEntity>();

    //    //            targets = targets.Concat(uis).Concat(motors).ToList();

    //    //            ValidTargets = targets.Select(e => {
    //    //                if (e is MotorEntity) {
    //    //                    return _mapper.Map<MotorTargetState>(e);
    //    //                }
    //    //                else if (e is ControlUiEntity) {
    //    //                    return _mapper.Map<ControlUiTargetState>(e);
    //    //                }
    //    //                else
    //    //                    return default(ISelectableState);
    //    //            }).ToArray();
    //    //        });
    //    //    });
    //    //    //_target.Handle<ListTargetRequest, ITargetable[]>(new ListTargetRequest(), targets => {
    //    //    //    ValidTargets = targets.Select(e => {
    //    //    //        if (e is MotorEntity) {
    //    //    //            return _mapper.Map<MotorTargetState>(e);
    //    //    //        }
    //    //    //        else if (e is ControlUiEntity) {
    //    //    //            return _mapper.Map<ControlUiTargetState>(e);
    //    //    //        }
    //    //    //        else
    //    //    //            return default(ISelectableState);
    //    //    //    }).ToArray();
    //    //    //});
    //    //}

    //    private void UpdateMaps(NLinearMapEntity[] maps) {
    //        if (Parent != null)
    //            Parent.Maps = maps.Select(e => _mapper.Map<NLinearMapState>(e)).ToArray();
    //    }

    //    public void ExecuteUpdateMotorCommand(object id) {
    //        var request = new UpdateMotorRequest {
    //            Id = (int)id,
    //            PropertyName = "Value",
    //            Value = TargetsOfSelectedMap[(int)id].Value,
    //        };

    //        _motorBus.Handle(request, (bool res) => { });
    //    }
    //}

    public interface IOutputPort : ICloneable {
        string Name { get; set; }
    }
    public interface IInputPort : ICloneable {
        string Name { get; set; }
    }

    public class OutputPort {
        public bool IsSelected { get; set; } = false;
        public string Name { get; set; }

        public OutputPort(IOutputPort reference) {
            Name = reference.Name;
        }
    }

    public class InputPort {
        public string Name { get; set; }
        public bool IsSelected { get; set; } = false;

        public InputPort(IInputPort reference) {
            Name = reference.Name;
        }
    }

}
