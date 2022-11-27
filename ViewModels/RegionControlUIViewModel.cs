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
using PCController;
using System.Windows.Controls;
using System.Xml.XPath;

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

    public class NLinearMapState {
        public int Id { get; set; }
        public string Name { get; set; }
        public InputPort[] InputPorts { get; set; } = new InputPort[0];
        public OutputPort[] OutputPorts { get; set; } = new OutputPort[0];
        public bool IsSelected { get; set; } = false;

        public int[] Shape { get; set; }
        public double[] Value { get; set; }

        public override string ToString() {
            return Name;
        }
    }

    public class ControlUiState : ObservableObject, IInputPort, IOutputPort {
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
            return (ControlUiState)MemberwiseClone();
        }
        public override string ToString() {
            return Name;
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

        private string _keymapInfo;

        private string _statusMsg;

        private string _systemInfo;

        private Point _tracePoint = new Point();

        // https://blog.csdn.net/jiuzaizuotian2014/article/details/104856673

        private ControlUiState[] _uiStates = new ControlUiState[0];
        public ControlUiState[] UiStates {
            get { return _uiStates; }
            set { SetProperty(ref _uiStates, value); }
        }

        private NLinearMapState[] _mapStates = new NLinearMapState[0];
        public NLinearMapState[] MapStates {
            get { return _mapStates; }
            set { SetProperty(ref _mapStates, value); }
        }


        // Motor Sources
        private MotorState[] _motorStates;
        public MotorState[] MotorStates {
            get { return _motorStates; }
            set { SetProperty(ref _motorStates, value); }
        }

        private DisplayOutputPort[] _validOutputPorts;
        public DisplayOutputPort[] ValidOutputPorts {
            get { return _validOutputPorts; }
            set { SetProperty(ref _validOutputPorts, value); }
        }

        private OutputPort[] _selectedOutputPorts;
        public OutputPort[] SelectedOutputPorts {
            get { return _selectedOutputPorts; }
            set { SetProperty(ref _selectedOutputPorts, value); }
        }

        private DisplayInputPort[] _validInputPorts;
        public DisplayInputPort[] ValidInputPorts {
            get { return _validInputPorts; }
            set { SetProperty(ref _validInputPorts, value); }
        }

        private ControlUiState[] _selectedUiStates = new ControlUiState[0];
        public ControlUiState[] SelectedUiStates {
            get { return _selectedUiStates; }
            set { SetProperty(ref _selectedUiStates, value); }
        }

        private NLinearMapState _selectedMap;
        public NLinearMapState SelectedMap {
            get { return _selectedMap; }
            set { 
                SetProperty(ref _selectedMap, value);

                if (_selectedMap != null)
                    SelectedUiStates = _selectedMap.InputPorts
                    .Select(e => UiStates.Where(e1 => e1.Name == e.Name).FirstOrDefault()).ToArray();
                else
                    SelectedUiStates = new ControlUiState[0];
            }
        }


        private MotorState[] _displayMotors = new MotorState[0];
        public MotorState[] DisplayMotors {
            get { return _displayMotors; }
            set { SetProperty(ref _displayMotors, value); }
        }

        private ControlUiState[] _displayUis = new ControlUiState[0];
        public ControlUiState[] DisplayUis {
            get { return _displayUis; }
            set { SetProperty(ref _displayUis, value); }
        }


        private ICommand _bindCommand;
        public ICommand BindCommand =>
            _bindCommand ?? (_bindCommand = new DelegateCommand<int[]>(ExecuteBindCommand));

        void ExecuteBindCommand(int[] index) {
            //var value = SelectedMap.OutputPorts.SelectMany()
            var value = SelectedMap.OutputPorts
                .SelectMany(e => {
                    if (e.Name.Contains("Motor")) {
                        var req = new ListMotorRequest() {
                            Id = e.Id
                        };

                        _motorBus.Handle(req, out MotorEntity motor);

                        return motor.Value;
                    }
                    else {
                        var req = new ListControlUiRequest() {
                            Id = e.Id
                        };

                        _uiBus.Handle(req, out ControlUiEntity ui);

                        return ui.Value;
                    }})
                .ToArray();


            var req1 = new UpdateNLinearMapRequest {
                Id = SelectedMap.Id,
                PropertyType = "SetValue",
                PropertyValue = new ValueContract {
                    Value = value,
                    Index = index
                }
            };

            _mapBus.Handle(req1, out NLinearMapEntity map);
            _mapper.Map(map, SelectedMap);


        }

        private ICommand _uiCommand;
        public ICommand UiCommand => _uiCommand ?? (_uiCommand = new RelayCommand<CommandParameter>(ExecuteUiCommand));

        private void ExecuteUiCommand(CommandParameter parameter) {
            if (parameter.Type == "AddNode") {
                var request = new AddNodeRequest {
                    Value = (Point)parameter.Payload[0],
                    UiId = (int)parameter.Payload[1],
                };

                _uiBus.Handle(request, out bool result);
                _uiBus.Handle(new ListControlUiRequest(), out ControlUiEntity[] uis);

                for (int i = 0; i < uis.Length; i++) {
                    _mapper.Map(uis[i], UiStates[i]);
                }

            }
            else if (parameter.Type == "Build") {
                var request = new BuildRegionRequest() {
                    Id = (int)parameter.Payload[0],
                };

                _uiBus.Handle(request, out ControlUiEntity _);
                _uiBus.Handle(new ListControlUiRequest(), out ControlUiEntity[] uis);

                for (int i = 0; i < uis.Length; i++) {
                    _mapper.Map(uis[i], UiStates[i]);
                }
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
                MapStates = _mapper.Map<NLinearMapState[]>(maps);
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
                UiStates = _mapper.Map<ControlUiState[]>(uis);
            });
        }

        private DelegateCommand _updateMapCommand;
        public DelegateCommand UpdateMapCommand =>
            _updateMapCommand ?? (_updateMapCommand = new DelegateCommand(ExecuteUpdateMapCommand));

        void ExecuteUpdateMapCommand() {
            if (SelectedMap == null) return;

            var selectedMap = SelectedMap;
            var uiReq = new ListControlUiRequest();
            var motorReq = new ListMotorRequest();


            var selectedInputPorts = ValidInputPorts.Where(e => e.IsSelected)
                .ToArray();
            var selectedOutputPorts = ValidOutputPorts.Where(e => e.IsSelected)
                .ToArray();

            _uiBus.Handle(uiReq, out ControlUiEntity[] uis);
            _motorBus.Handle(motorReq, out MotorEntity[] motors);

            var inputPortCollection = uis.Cast<IInputPort>();
            var outputPortCollection = uis.Cast<IOutputPort>().Concat(motors.Cast<IOutputPort>());

            var inputParameter = selectedInputPorts.Select(
                e0 => InputPort.Create(inputPortCollection.First(e1 => e1.Name == e0.Name)))
                .ToArray();
            var outputParameter = selectedOutputPorts.Select(
                e0 => OutputPort.Create(outputPortCollection.First(e1 => e1.Name == e0.Name)))
                .ToArray();

            var request = new UpdateNLinearMapRequest {
                Id = selectedMap.Id,
                PropertyType = "UpdateInputs",
                PropertyValue = inputParameter
            };

            _mapBus.Handle(request, out NLinearMapEntity map);
            _mapper.Map(map, SelectedMap);

            request = new UpdateNLinearMapRequest {
                Id = selectedMap.Id,
                PropertyType = "UpdateOutputs",
                PropertyValue = outputParameter
            };

            _mapBus.Handle(request, out map);
            _mapper.Map(map, SelectedMap);

            SelectedUiStates = UiStates
                .Where(e => SelectedMap.InputPorts.Any(e1 => e1.Name == e.Name))
                .ToArray();


            // Invalidate valid ports
            foreach (var input in ValidInputPorts) {
                input.IsSelected = false;
            }
            foreach (var output in ValidOutputPorts) {
                output.IsSelected = false;
            }

            // Invalidate display motors and uis
            DisplayMotors = SelectedMap.OutputPorts.Where(e => e.Name.Contains("Motor"))
                .Select(e => motors.Select(e1 => _mapper.Map<MotorState>(e1))
                                   .First(e1 => e1.Name == e.Name))
                .ToArray();
            DisplayUis = SelectedMap.OutputPorts.Where(e => e.Name.Contains("ControlUi"))
                .Select(e => uis.Select(e1 => _mapper.Map<ControlUiState>(e1))
                                .First(e1 => e1.Name == e.Name))
                .ToArray();
        }

        private DelegateCommand _initMapCommand;
        public DelegateCommand InitMapCommand =>
            _initMapCommand ?? (_initMapCommand = new DelegateCommand(ExecuteInitMapCommand));

        void ExecuteInitMapCommand() {
            if (SelectedMap == null) return;

            var uiReq = new ListControlUiRequest();
            var motorReq = new ListMotorRequest();

            var inputPorts = SelectedMap.InputPorts.ToArray();
            var outputPorts = SelectedMap.OutputPorts.ToArray();

            _uiBus.Handle(uiReq, out ControlUiEntity[] uis);
            _motorBus.Handle(motorReq, out MotorEntity[] motors);

            var inputPortCollection = uis.Cast<IInputPort>();
            var outputPortCollection = uis.Cast<IOutputPort>().Concat(motors.Cast<IOutputPort>());

            var inputParameter = inputPorts.Select(
                e0 => InputPort.Create(inputPortCollection.First(e1 => e1.Name == e0.Name)))
                .ToArray();
            var outputParameter = outputPorts.Select(
                e0 => OutputPort.Create(outputPortCollection.First(e1 => e1.Name == e0.Name)))
                .ToArray();

            var request = new UpdateNLinearMapRequest {
                Id = SelectedMap.Id,
                PropertyType = "UpdateInputs",
                PropertyValue = inputParameter
            };

            _mapBus.Handle(request, out NLinearMapEntity map);
            _mapper.Map(map, SelectedMap);

            request = new UpdateNLinearMapRequest {
                Id = SelectedMap.Id,
                PropertyType = "UpdateOutputs",
                PropertyValue = outputParameter
            };

            _mapBus.Handle(request, out map);
            _mapper.Map(map, SelectedMap);


            request = new UpdateNLinearMapRequest {
                Id = SelectedMap.Id,
                PropertyType = "Init",
                PropertyValue = null
            };

            _mapBus.Handle(request, out map);
            _mapper.Map(map, SelectedMap);

            SelectedUiStates = UiStates.Where(
                e => SelectedMap.InputPorts.Any(e1 => e1.Name == e.Name))
                .ToArray();
        }

        public string KeymapInfo {
            get => _keymapInfo;
            set => SetProperty(ref _keymapInfo, value);
        }

        public string StatusMsg {
            get => _statusMsg;
            set => SetProperty(ref _statusMsg, value);
        }

        public string SystemInfo {
            get => _systemInfo;
            set => SetProperty(ref _systemInfo, value);
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

            //SystemInfo = $"{_operationMode}";
        }

        public void UpdateControlUiInput(ControlUiState state, Point value, BaseRegionState hitRegion) {
            var req = new ListControlUiRequest() {
                Id = state.Id,
            };

            _uiBus.Handle(req, out ControlUiEntity ui);

            if (hitRegion != null) {
                var lambdas = ui.Regions.Where(e => e.Id == hitRegion.Id).First().GetLambdas(value, ui.Nodes);

                var req0 = new ListNLinearMapRequest() {
                    Id = SelectedMap.Id,
                };

                _mapBus.Handle(req0, out NLinearMapEntity map);

                _uiBus.Handle(new ListControlUiRequest(), out ControlUiEntity[] uis);
                _motorBus.Handle(new ListMotorRequest(), out MotorEntity[] motors);

                var result = map.MapTo(lambdas).GetData<double>();

                Console.WriteLine($"{state.Name} trace a point.{value} - {string.Join(",", lambdas)} - {result}");

                int idx = 0;
                var values = SelectedMap.OutputPorts.Select(e => {
                    var returnValue = result.Skip(idx).Take(e.Dimension).ToArray();

                    idx += e.Dimension;

                    return returnValue;
                }).ToArray().GetEnumerator();

                foreach(var output in SelectedMap.OutputPorts) {
                    values.MoveNext();

                    if (output.Name.Contains("Motor")) {
                        var req1 = new UpdateMotorRequest {
                            Id = output.Id,
                            Value = values.Current
                        };

                        _motorBus.Handle(req1, out MotorEntity motorEntity);

                        Console.WriteLine($"{output.Name}: {motorEntity.Value[0]}");
                    }
                    else {
                        var req1 = new UpdateControlUiRequest {
                            Id = output.Id,
                            PropertyName = "Value",
                            PropertyValue = values.Current
                        };

                        _uiBus.Handle(req1, out ControlUiEntity uiEntity);

                        Console.WriteLine($"{output.Name}: {uiEntity.Value[0]} {uiEntity.Value[1]}");
                    }
                }
            }
        }

        public void UpdateMotor(MotorState state) {
            var req = new UpdateMotorRequest {
                Id = state.Id,
                Value = state.Value,
            };

            _motorBus.Handle(req, out MotorEntity motor);
        }

        public void InvalidateValidPorts() {
            InvalidateMotors();
            InvalidateUis();
            InvalidateMaps();

            ValidInputPorts = new List<IInputPort>()
                .Concat(UiStates)
                .Select(e => new DisplayInputPort(e))
                .ToArray();
            ValidOutputPorts = new List<IOutputPort>()
                .Concat(UiStates)
                .Concat(MotorStates)
                .Select(e => new DisplayOutputPort(e))
                .ToArray();
        }


        private void InvalidateMaps() {
            var request = new ListNLinearMapRequest();

            _mapBus.Handle(request, (NLinearMapEntity[] maps) => {
                MapStates = _mapper.Map<NLinearMapState[]>(maps);
            });
        }

        private void InvalidateMotors() {
            var request = new ListMotorRequest();

            _motorBus.Handle(request, (MotorEntity[] motors) => {
                MotorStates = _mapper.Map<MotorState[]>(motors);
            });
        }


        private void InvalidateUis() {
            _uiBus.Handle(new ListControlUiRequest(), (ControlUiEntity[] uis) => {
                UiStates = _mapper.Map<ControlUiState[]>(uis);
            });
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

        public bool[] GetTensorStatus(int[][] indices) {
            if (SelectedMap == null) return default;

            var req = new ListNLinearMapRequest {
                Id = SelectedMap.Id
            };

            _mapBus.Handle(req, out NLinearMapEntity map);

            var result = indices.Select(e => {
                return map.HasSet(e);
            });

            return result.ToArray();
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

    public class SimplexState : BaseRegionState {
        public Point[] Points { get; set; } = new Point[0];

        public override string ToString() {
            return Name;
        }
    }


    public class VoronoiState : BaseRegionState {
        public Point[] Points { get; set; } = new Point[0];
        public override string ToString() {
            return Name;
        }
    }
}