﻿using AutoMapper;
using Numpy;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SkiaSharp;
using System;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using taskmaker_wpf.Domain;
using taskmaker_wpf.Views;
using CommunityToolkit.Mvvm;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using taskmaker_wpf.Views.Widget;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Collections.Generic;
using System.Xml.Linq;

namespace taskmaker_wpf.ViewModels {

    public interface ITraceRegion { }

    public static class Helper {

        public static IObservable<TSource> Debug<TSource>(this IObservable<TSource> observable) {
            observable.Subscribe(
                (e) => { Console.WriteLine($"[{DateTime.Now}] OnNext({e})"); },
                (e) => { Console.WriteLine($"[{DateTime.Now}] OnError({e})"); },
                () => { Console.WriteLine($"[{DateTime.Now}] OnCompleted()"); });

            return observable;
        }

        public static void Dump<T>(this IObservable<T> source, string name) {
            source.Subscribe(
                i => Console.WriteLine("{0}-->{1}", name, i),
                ex => Console.WriteLine("{0} failed-->{1}", name, ex.Message),
                () => Console.WriteLine("{0} completed", name));
        }

        public static NDarray<float> ToNDarray(this Point pt) {
            return np.array((float)pt.X, (float)pt.Y);
        }

        public static NDarray<float> ToNDarray(this SKPoint pt) {
            return np.array(pt.X, pt.Y);
        }

        public static Point ToPoint(this NDarray pt) {
            if (pt.ndim > 1)
                throw new Exception("Invalid ndarray");

            var castValues = pt.astype(np.float32);
            var values = castValues.GetData<float>();

            castValues.Dispose();

            return new Point { X = values[0], Y = values[1] };
        }

        public static SKPoint ToSKPoint(this NDarray pt) {
            if (pt.ndim > 1)
                throw new Exception("Invalid ndarray");

            var castValues = pt.astype(np.float32);
            var values = castValues.GetData<float>();

            castValues.Dispose();

            return new SKPoint(values[0], values[1]);
        }
    }

    public class NLinearMapState_v1 {
        public int Id { get; set; }
        public string Name { get; set; }
        public InputPort[] InputPorts { get; set; } = new InputPort[0];
        public OutputPort[] OutputPorts { get; set; } = new OutputPort[0];
        public bool IsSelected { get; set; } = false;
        public override string ToString() {
            return Name;
        }
    }

    public class ControlUiState_v1 : ObservableObject, IInputPort, IOutputPort {
        private int _id;
        public int Id {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        public string Name { get; set; }

        private NodeState_v1[] _nodes = new NodeState_v1[0];
        public NodeState_v1[] Nodes {
            get { return _nodes; }
            set { SetProperty(ref _nodes, value); }
        }

        private BaseRegionState[] _regions;
        public BaseRegionState[] Regions {
            get { return _regions; }
            set { SetProperty(ref _regions, value); }
        }

        public bool IsSelected { get; set; } = false;

        public object Clone() {
            return (ControlUiState_v1)MemberwiseClone();
        }
        public override string ToString() {
            return Name;
        }
    }

    public class ControlUiState : BindableBase {
        private int _id;

        private string _name;

        private NodeState[] _nodes;

        private RegionState[] _regions;

        private double[] _value;

        private TargetState[] _targets;

        public int Id {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public NodeState[] Nodes {
            get => _nodes;
            set => SetProperty(ref _nodes, value);
        }

        public RegionState[] Regions {
            get => _regions;
            set => SetProperty(ref _regions, value);
        }

        public double[] Value {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public TargetState[] Targets {
            get => _targets;
            set => SetProperty(ref _targets, value);
        }
        public override string ToString() {
            return Name;
        }
    }

    public class NodeState : BindableBase {
        private int _id;

        private bool _isSet;

        private double[] _targetValue;

        private Point _value;

        public int Id {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        public bool IsSet {
            get => _isSet;
            set => SetProperty(ref _isSet, value);
        }

        public double[] TargetValue {
            get => _targetValue;
            set => SetProperty(ref _targetValue, value);
        }

        public Point Value {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }

    public class NodeState_v1 : ObservableObject {
        private int _id;
        public int Id {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        private Point _value = new Point();
        public Point Value {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public NodeState_v1(int id, Point value) {
            Id = id;
            Value = value;
        }
        public override string ToString() {
            return $"Node[{Id}] - ({Value})";
        }
    }

    public partial class RegionControlUIViewModel : ObservableObject, INavigationAware {
        private readonly NLinearMapInteractorBus _mapBus;

        private readonly IMapper _mapper;

        private readonly MotorInteractorBus _motorBus;

        private readonly ControlUiInteractorBus _uiBus;

        private DelegateCommand<object> _addNodeCommand;

        private DelegateCommand _buildCommand;

        private string _debug;

        private FrameworkElement _hitElement;

        private FrameworkElement _inspectedWidget;

        private DelegateCommand<object> _interpolateCommand;

        private string _keymapInfo;

        private NLinearMapState[] _maps = new NLinearMapState[0];

        private NLinearMapState _mapState;

        private OperationMode _operationMode = Views.OperationMode.Default;

        private DelegateCommand<NodeState> _removeNodeCommand;

        private FrameworkElement _selectedNodeWidget;

        private DelegateCommand<object> _setValueCommand;

        private string _statusMsg;

        private string _systemInfo;

        private Point _tracePoint = new Point();

        // https://blog.csdn.net/jiuzaizuotian2014/article/details/104856673

        private ControlUiState_v1[] _uiStates = new ControlUiState_v1[0];
        public ControlUiState_v1[] UiStates {
            get { return _uiStates; }
            set { SetProperty(ref _uiStates, value); }
        }

        private NLinearMapState_v1[] _mapStates = new NLinearMapState_v1[0];
        public NLinearMapState_v1[] MapStates {
            get { return _mapStates; }
            set { SetProperty(ref _mapStates, value); }
        }

        private MotorState_v1[] _motorStates;
        public MotorState_v1[] MotorStates {
            get { return _motorStates; }
            set { SetProperty(ref _motorStates, value); }
        }

        private OutputPort[] _validOutputPorts;
        public OutputPort[] ValidOutputPorts {
            get { return _validOutputPorts; }
            set { SetProperty(ref _validOutputPorts, value); }
        }

        private OutputPort[] _selectedOutputPorts;
        public OutputPort[] SelectedOutputPorts {
            get { return _selectedOutputPorts; }
            set { SetProperty(ref _selectedOutputPorts, value); }
        }

        private InputPort[] _validInputPorts;
        public InputPort[] ValidInputPorts {
            get { return _validInputPorts; }
            set { SetProperty(ref _validInputPorts, value); }
        }

        private ControlUiState_v1[] _selectedUiStates = new ControlUiState_v1[0];
        public ControlUiState_v1[] SelectedUiStates {
            get { return _selectedUiStates; }
            set { SetProperty(ref _selectedUiStates, value); }
        }

        private NLinearMapState_v1 _selectedMap;
        public NLinearMapState_v1 SelectedMap {
            get { return _selectedMap; }
            set { SetProperty(ref _selectedMap, value); }
        }

        private ICommand _uiCommand;
        public ICommand UiCommand => _uiCommand ?? (_uiCommand = new RelayCommand<CommandParameter>(ExecuteUiCommand));

        private void ExecuteUiCommand(CommandParameter parameter) {
            if (parameter.Type == "AddNode") {
                var request = new AddNodeRequest {
                    Value = (Point)parameter.Payload[0],
                    UiId = (int)parameter.Payload[1],
                };

                _uiBus.Handle(request, (bool res) => {
                    _uiBus.Handle(new ListControlUiRequest(), (ControlUiEntity[] uis) => {
                        for (int i = 0; i < uis.Length; i++) {
                            _mapper.Map(uis[i], UiStates[i]);
                        }

                    });
                });
            }
            else if (parameter.Type == "Build") {
                var request = new BuildRegionRequest() {
                    Id = (int)parameter.Payload[0],
                };

                _uiBus.Handle(request, (ControlUiEntity ui) => {
                    _uiBus.Handle(new ListControlUiRequest(), (ControlUiEntity[] uis) => {
                        for (int i = 0; i < uis.Length; i++) {
                            _mapper.Map(uis[i], UiStates[i]);
                        }
                    });
                });
            }
            else if (parameter.Type == "Select") {
                var selectedNode = (NodeInfo)parameter.Payload[0];


            }
            else if (parameter.Type == "SelectMap") {
                //SelectedMap = (NLinearMapState_v1)parameter.Payload[1];
            }
            else if (parameter.Type == "AddMap") {
                
            }
            else if (parameter.Type == "UpdateMap") {
                //var mapId = (int)parameter.Payload[1];
                //var selectedMap = MapStates
                //    .Where(e => e.Id == mapId)
                //    .First();

                //var inputs = selectedMap.InputPorts.Where(e => e.IsSelected).ToArray();
                //var request = new UpdateNLinearMapRequest {
                //    Id = selectedMap.Id,
                //    PropertyType = "UpdateInputs",
                //    PropertyValue = inputs
                //};

                //_mapBus.Handle(request, (NLinearMapEntity map) => {
                //    selectedMap = _mapper.Map<NLinearMapState_v1>(map);
                //});

                //var outputs = selectedMap.OutputPorts.Where(e => e.IsSelected).ToArray();
                //request = new UpdateNLinearMapRequest {
                //    Id = selectedMap.Id,
                //    PropertyType = "UpdateOutputs",
                //    PropertyValue = outputs
                //};

                //_mapBus.Handle(request, (NLinearMapEntity map) => {
                //    selectedMap = _mapper.Map<NLinearMapState_v1>(map);
                //});

                //TargetMotors = Parent.MapState.Outputs
                //    .Where(e => e.GetType().Name.Contains("Motor"))
                //    .Cast<MotorTargetState>()
                //    .ToArray();
            }
        }

        private ICommand _addMapCommand;
        public ICommand AddMapCommand =>
            _addMapCommand ?? (_addMapCommand = new DelegateCommand(ExecuteAddMapCommand));

        void ExecuteAddMapCommand() {
            var addReq = new AddNLinearMapRequest();
            var listReq = new ListNLinearMapRequest();

            _mapBus.Handle(addReq, (bool res) => { });
            _mapBus.Handle(listReq, (NLinearMapEntity[] maps) => {
                MapStates = _mapper.Map<NLinearMapState_v1[]>(maps);
            });
        }

        private ICommand _addUiCommand;
        public ICommand AddUiCommand =>
            _addUiCommand ?? (_addUiCommand = new DelegateCommand(ExecuteAddUiCommand));

        void ExecuteAddUiCommand() {
            var request = new AddControlUiRequest();
            var request1 = new ListControlUiRequest();

            _uiBus.Handle(request, (bool res) => { });
            _uiBus.Handle(request1, (ControlUiEntity[] uis) => {
                UiStates = _mapper.Map<ControlUiState_v1[]>(uis);
            });
        }

        private DelegateCommand _updateMapCommand;
        public DelegateCommand UpdateMapCommand =>
            _updateMapCommand ?? (_updateMapCommand = new DelegateCommand(ExecuteUpdateMapCommand));

        void ExecuteUpdateMapCommand() {
            if (SelectedMap == null) return;

            var selectedMap = SelectedMap;

            var inputs = ValidInputPorts.Where(e => e.IsSelected).ToArray();
            var request = new UpdateNLinearMapRequest {
                Id = selectedMap.Id,
                PropertyType = "UpdateInputs",
                PropertyValue = inputs
            };

            _mapBus.Handle(request, (NLinearMapEntity map) => {
                _mapper.Map(map, SelectedMap);
            });

            var outputs = ValidOutputPorts.Where(e => e.IsSelected).ToArray();
            request = new UpdateNLinearMapRequest {
                Id = selectedMap.Id,
                PropertyType = "UpdateOutputs",
                PropertyValue = outputs
            };

            _mapBus.Handle(request, (NLinearMapEntity map) => {
                _mapper.Map(map, SelectedMap);
            });

            SelectedUiStates = UiStates
                .Where(e => SelectedMap.InputPorts.Any(e1 => e1.Name == e.Name))
                .ToArray();

            //TargetMotors = Parent.MapState.Outputs
            //    .Where(e => e.GetType().Name.Contains("Motor"))
            //    .Cast<MotorTargetState>()
            //    .ToArray();
        }

        public string Debug {
            get => _debug;
            set => SetProperty(ref _debug, value);
        }

        public FrameworkElement HitElement {
            get => _hitElement;
            set => SetProperty(ref _hitElement, value);
        }

        public string KeymapInfo {
            get => _keymapInfo;
            set => SetProperty(ref _keymapInfo, value);
        }

        public OperationMode OperationMode {
            get => _operationMode;
            set => SetProperty(ref _operationMode, value);
        }

        public DelegateCommand<NodeState> RemoveNodeCommand =>
            _removeNodeCommand ?? (_removeNodeCommand = new DelegateCommand<NodeState>(ExecuteRemoveNodeCommand));

        public FrameworkElement SelectedNodeWidget {
            get => _selectedNodeWidget;
            set => SetProperty(ref _selectedNodeWidget, value);
        }

        public DelegateCommand<object> SetValueCommand =>
            _setValueCommand ?? (_setValueCommand = new DelegateCommand<object>(ExecuteSetValueCommand));

        public string StatusMsg {
            get => _statusMsg;
            set => SetProperty(ref _statusMsg, value);
        }

        public string SystemInfo {
            get => _systemInfo;
            set => SetProperty(ref _systemInfo, value);
        }

        public Point TracePoint {
            get => _tracePoint;
            set {
                SetProperty(ref _tracePoint, value);

                if (_operationMode == OperationMode.Trace) {
                    Interpolate();
                }
            }
        }

        public RegionControlUIViewModel(MapperConfiguration config,
            ControlUiInteractorBus uiBus,
            NLinearMapInteractorBus mapBus,
            MotorInteractorBus motorBus,
            ListTargetInteractor targetInteractor) {
            _mapper = config.CreateMapper();

            _uiBus = uiBus;
            _mapBus = mapBus;
            _motorBus = motorBus;

            InvalidateMotors();
            InvalidateUis();
            InvalidateMaps();

            InvalidateValidPorts();

            SystemInfo = $"{_operationMode}";
        }

        private void ExecuteRemoveNodeCommand(NodeState parameter) {
            RemoveNode(parameter);
        }

        private void ExecuteSetValueCommand(object param) {
            //if (param is int selectedNodeId) {
            //    var targetValues = TargetsPanelVM.TargetsOfSelectedMap.SelectMany(e => e.Value).ToArray();
            //    //var value = TargetsPanelVM.TargetMotors

            //    var request = new UpdateNodeRequest {
            //        UiId = UiState.Id,
            //        NodeId = selectedNodeId,
            //        PropertyName = "TargetValue",
            //        PropertyValue = targetValues,
            //    };

            //    _uiBus.Handle(request, (ControlUiEntity ui) => {
            //        UiState = _mapper.Map<ControlUiState>(ui);
            //    });
            //}
            //if (SelectedNodeWidget is null) return;

            
            //_uiBus.Handle()
        }

        private void Interpolate() {
            var pt = TracePoint;

            var target = HitElement?.DataContext as ITraceRegion;

            if (target is null) return;
            else {
                //if (target is StatefulSimplex) {
                //    var bary = (target.GetRef() as SimplexM).Bary;
                //    var lambdas = UI.Complex.Bary.GetLambdas(bary, pt.ToNDarray());
                //    //var result = _map.MapTo(lambdas);

                //    //UI.Complex.Targets.SetValue(result.GetData<double>());
                //}
                //else if (target is StatefulVoronoi) {
                //    var bary = (target.GetRef() as VoronoiRegionM).Bary;
                //    var lambdas = UI.Complex.Bary.GetLambdas(bary, pt.ToNDarray());
                //    //var result = _map.MapTo(lambdas);

                //    //UI.Complex.Targets.SetValue(result.GetData<double>());
                //}

                //Debug = UI.Complex.Targets.ToString();
            }
        }

        public void InvalidateValidPorts() {
            InvalidateMotors();
            InvalidateUis();
            InvalidateMaps();

            ValidInputPorts = new List<IInputPort>()
                .Concat(UiStates)
                .Select(e => new InputPort(e))
                .ToArray();
            ValidOutputPorts = new List<IOutputPort>()
                .Concat(UiStates)
                .Concat(MotorStates)
                .Select(e => new OutputPort(e))
                .ToArray();

            //InvalidatePortStates();
        }

        private void InvalidatePortStates() {
            var targetMap = MapStates.Where(e => e.IsSelected).FirstOrDefault();

            if (targetMap != null) {
                foreach (var i in targetMap.InputPorts) {
                    ValidInputPorts.Where(e => e.Name == i.Name).First().IsSelected = true;
                }

                foreach (var o in targetMap.OutputPorts) {
                    ValidOutputPorts.Where(e => e.Name == o.Name).First().IsSelected = true;
                }
            }

        }

        private void InvalidateMaps() {
            var request = new ListNLinearMapRequest();

            _mapBus.Handle(request, (NLinearMapEntity[] maps) => {
                MapStates = _mapper.Map<NLinearMapState_v1[]>(maps);
            });
        }

        private void InvalidateMotors() {
            var request = new ListMotorRequest();

            _motorBus.Handle(request, (MotorEntity[] motors) => {
                MotorStates = _mapper.Map<MotorState_v1[]>(motors);
            });
        }


        private void InvalidateUis() {
            _uiBus.Handle(new ListControlUiRequest(), (ControlUiEntity[] uis) => {
                UiStates = _mapper.Map<ControlUiState_v1[]>(uis);
            });
        }

        private void RemoveNode(NodeState node) {
            //UI.Complex.Nodes.Remove(node);
        }
        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            //UiState = navigationContext.Parameters["ui"] as ControlUiState;
            //InvalidateUi();
            //InvalidateMap();
        }
    }

    public interface IRegionState {
        int Id { get; set; }
        string Name { get; set; }
    }

    public class BaseRegionState : IRegionState {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SimplexState_v1 : BaseRegionState {
        public Point[] Points { get; set; } = new Point[0];

        public override string ToString() {
            return Name;
        }
    }


    public class VoronoiState_v1 : BaseRegionState {
        public Point[] Points { get; set; } = new Point[0];
        public override string ToString() {
            return Name;
        }
    }

    public class RegionState : BindableBase {
        private int _id;

        private string _name;

        public int Id {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }

    public class SimplexState : RegionState, ITraceRegion {
        private Point[] _points;

        public Point[] Points {
            get => _points;
            set => SetProperty(ref _points, value);
        }
    }

    public class VoronoiState : RegionState, ITraceRegion {
        private Point[] _points;

        public Point[] Points {
            get => _points;
            set => SetProperty(ref _points, value);
        }
    }
}