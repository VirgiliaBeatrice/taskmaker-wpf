using AutoMapper;
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
using static System.Windows.Forms.AxHost;
using taskmaker_wpf.Model.Data;
using SharpVectors.Dom.Svg;
using System.Windows.Media;
using System.Text;
using NLog;
using System.Diagnostics;

namespace taskmaker_wpf.ViewModels {
    public static class Helper {

        public static IObservable<TSource> Dump<TSource>(this IObservable<TSource> observable) {
            observable.Subscribe(
                (e) => { Debug.WriteLine($"[{DateTime.Now}] OnNext({e})"); },
                (e) => { Debug.WriteLine($"[{DateTime.Now}] OnError({e})"); },
                () => { Debug.WriteLine($"[{DateTime.Now}] OnCompleted()"); });

            return observable;
        }

        public static void Dump<T>(this IObservable<T> source, string name) {
            _ = source.Subscribe(
                i => Debug.WriteLine("{0}-->{1}", name, i),
                ex => Debug.WriteLine("{0} failed-->{1}", name, ex.Message),
                () => Debug.WriteLine("{0} completed", name));
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

    public class NLinearMapState : IOutputPortState {
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

    public class ControlUiState : ObservableObject, IInputPortState {
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

        private string _keymapInfo;

        private string _statusMsg;

        private string _systemInfo;

        // https://blog.csdn.net/jiuzaizuotian2014/article/details/104856673

        [ObservableProperty]
        private ControlUiState[] _uiStates = new ControlUiState[0];

        [ObservableProperty]
        private NLinearMapState[] _mapStates = new NLinearMapState[0];

        partial void OnMapStatesChanging(NLinearMapState[] value) {
            if (_selectedMap != null) {
                SelectedMap = value.Where(e => e.Id == SelectedMap.Id).FirstOrDefault();
            }
        }

        // Motor Sources
        [ObservableProperty]
        private MotorState[] _motorStates;

        [ObservableProperty]
        private DisplayOutputPort[] _validOutputPorts;

        [ObservableProperty]
        private OutputPort[] _selectedOutputPorts;

        [ObservableProperty]
        private DisplayInputPort[] _validInputPorts;

        //[ObservableProperty]
        //private ControlUiState[] _selectedUiStates = new ControlUiState[0];

        [ObservableProperty]
        private NLinearMapState _selectedMap;

        [ObservableProperty]
        private ControlUiState[] _inputsOfSelectedMap = Array.Empty<ControlUiState>();

        partial void OnSelectedMapChanged(NLinearMapState value) {
            UpdateInputsOfSelectedMap();
            UpdateOutputsOfSelectedMap();
        }

        private void UpdateInputsOfSelectedMap() {
            if (SelectedMap != null) {
                var inputs = new List<ControlUiState>();

                foreach (var input in SelectedMap.InputPorts) {
                    inputs.Add(UiStates.Where(e => e.Name == input.Name).FirstOrDefault());
                }

                InputsOfSelectedMap = inputs.ToArray();
            }
            else {
                InputsOfSelectedMap = Array.Empty<ControlUiState>();
            }
        }

        private void UpdateOutputsOfSelectedMap() {
            if (SelectedMap != null) {
                MotorOutputsOfSelectedMap = SelectedMap.OutputPorts.Where(e => e.Name.Contains("Motor"))
                    .Select(e => MotorStates.Where(e1 => e1.Name == e.Name).FirstOrDefault())
                    .ToArray();

                MapOutputsOfSelectedMap = SelectedMap.OutputPorts.Where(e => e.Name.Contains("ControlUi"))
                    .Select(e => UiStates.Where(e1 => e1.Name == e.Name).FirstOrDefault())
                    .ToArray();
            }
            else {
                MotorOutputsOfSelectedMap = Array.Empty<MotorState>();
                MapOutputsOfSelectedMap = Array.Empty<ControlUiState>();
            }
        }

        [ObservableProperty]
        private MotorState[] _motorOutputsOfSelectedMap = Array.Empty<MotorState>();

        [ObservableProperty]
        private ControlUiState[] _mapOutputsOfSelectedMap = Array.Empty<ControlUiState>();

        [RelayCommand]
        void Bind(int[] index) {
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

        [RelayCommand]
        public void ListMotors() {
            var request = new ListMotorRequest();

            _motorBus.Handle(request, out MotorEntity[] motors);

            MotorStates = _mapper.Map<MotorState[]>(motors);
        }

        [RelayCommand]
        public void ListMaps() {
            var request = new ListNLinearMapRequest();

            _mapBus.Handle(request, out NLinearMapEntity[] maps);

            MapStates = _mapper.Map<NLinearMapState[]>(maps);
        }

        [RelayCommand]
        public void ListUis() {
            var request = new ListControlUiRequest();

            _uiBus.Handle(request, out ControlUiEntity[] uis);

            UiStates = _mapper.Map<ControlUiState[]>(uis);
        }

        [RelayCommand]
        private void ListValidPorts() {
            InvalidateValidPorts();
        }


        [RelayCommand]
        private void UpdateUi(CommandParameter parameter) {
            if (parameter.Type == "AddNode") {
                var request = new AddNodeRequest {
                    Value = (Point)parameter.Payload[0],
                    UiId = (int)parameter.Payload[1],
                };

                _uiBus.Handle(request, out ControlUiEntity ui);

                var target = UiStates.Where(e => e.Id == (int)parameter.Payload[1]).FirstOrDefault();

                _mapper.Map(ui, target);
            }
            else if (parameter.Type == "Build") {
                var request = new BuildRegionRequest() {
                    Id = (int)parameter.Payload[0],
                };

                _uiBus.Handle(request, out ControlUiEntity ui);

                var target = UiStates.Where(e => e.Id == (int)parameter.Payload[0]).FirstOrDefault();

                _mapper.Map(ui, target);
            }
        }

        //[RelayCommand]
        //private void ExcuteUi(CommandParameter parameter) {
        //    if (parameter.Type == "AddNode") {
        //        var request = new AddNodeRequest {
        //            Value = (Point)parameter.Payload[0],
        //            UiId = (int)parameter.Payload[1],
        //        };

        //        _uiBus.Handle(request, out bool result);
        //        _uiBus.Handle(new ListControlUiRequest(), out ControlUiEntity[] uis);

        //        for (int i = 0; i < uis.Length; i++) {
        //            _mapper.Map(uis[i], UiStates[i]);
        //        }

        //        UpdateInputsOfSelectedMap();
        //    }
        //    else if (parameter.Type == "Build") {
        //        var request = new BuildRegionRequest() {
        //            Id = (int)parameter.Payload[0],
        //        };

        //        _uiBus.Handle(request, out ControlUiEntity _);
        //        _uiBus.Handle(new ListControlUiRequest(), out ControlUiEntity[] uis);

        //        for (int i = 0; i < uis.Length; i++) {
        //            _mapper.Map(uis[i], UiStates[i]);
        //        }
        //    }
        //}

        [RelayCommand]
        void AddMap() {
            var addReq = new AddNLinearMapRequest();
            var listReq = new ListNLinearMapRequest();

            _mapBus.Handle(addReq, out bool res);
            _mapBus.Handle(listReq, out NLinearMapEntity[] maps);

            MapStates = _mapper.Map<NLinearMapState[]>(maps);

            //_mapBus.Handle(addReq, (bool res) => { });
            //_mapBus.Handle(listReq, (NLinearMapEntity[] maps) => {
            //    MapStates = _mapper.Map<NLinearMapState[]>(maps);
            //});
        }

        [RelayCommand]
        void AddUi() {
            var request = new AddControlUiRequest();
            var request1 = new ListControlUiRequest();

            _uiBus.Handle(request, out bool res);
            
            _uiBus.Handle(request1, out ControlUiEntity[] uis);
            
            UiStates = _mapper.Map<ControlUiState[]>(uis);
        }

        [RelayCommand]
        void UpdateMap() {
            if (SelectedMap == null) return;

            var selectedMap = SelectedMap;
            var uiReq = new ListControlUiRequest();
            var motorReq = new ListMotorRequest();
            var mapReq = new ListNLinearMapRequest();

            var selectedInputPorts = ValidInputPorts.Where(e => e.IsSelected)
                .ToArray();
            var selectedOutputPorts = ValidOutputPorts.Where(e => e.IsSelected)
                .ToArray();

            _uiBus.Handle(uiReq, out ControlUiEntity[] uis);
            _motorBus.Handle(motorReq, out MotorEntity[] motors);
            _mapBus.Handle(mapReq, out NLinearMapEntity[] maps);

            var inputPortCollection = uis.Cast<IInputPort>();
            var outputPortCollection = maps.Cast<IOutputPort>().Concat(motors.Cast<IOutputPort>());

            var inputParam = selectedInputPorts
                .Select(
                    e => InputPort.Create(inputPortCollection.Where(e1 => e1.Name == e.Name).FirstOrDefault()))
                .ToArray();

            var outputParam = selectedOutputPorts
                .Select(
                    e => OutputPort.Create(outputPortCollection.Where(e1 => e1.Name == e.Name).FirstOrDefault()))
                .ToArray();

            var request = new UpdateNLinearMapRequest {
                Id = selectedMap.Id,
                PropertyType = "UpdateInputs",
                PropertyValue = inputParam
            };

            _mapBus.Handle(request, out NLinearMapEntity map);
            _mapper.Map(map, SelectedMap);

            request = new UpdateNLinearMapRequest {
                Id = selectedMap.Id,
                PropertyType = "UpdateOutputs",
                PropertyValue = outputParam
            };

            _mapBus.Handle(request, out map);
            _mapper.Map(map, SelectedMap);

            UpdateInputsOfSelectedMap();
            UpdateOutputsOfSelectedMap();
        }

        [RelayCommand]
        void InitializeMap() {
            if (SelectedMap == null) return;

            var basisDims = InputsOfSelectedMap.Select(e => e.Nodes.Length).ToArray();

            var request = new UpdateNLinearMapRequest {
                Id = SelectedMap.Id,
                PropertyType = "Init",
                PropertyValue = basisDims
            };

            _mapBus.Handle(request, out NLinearMapEntity map);
            _mapper.Map(map, SelectedMap);

            UpdateInputsOfSelectedMap();
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
            MotorInteractorBus motorBus) {
            _mapper = config.CreateMapper();

            _uiBus = uiBus;
            _mapBus = mapBus;
            _motorBus = motorBus;

            //InvalidateMotors();
            //InvalidateUis();
            //InvalidateMaps();

            //InvalidateValidPorts();

            //SystemInfo = $"{_operationMode}";
            Invalidate();
        }

        public void Invalidate() {
            ListMotors();
            ListUis();
            ListMaps();
            ListValidPorts();
        }

        public void UpdateControlUiValue(ControlUiState state, Point pt) {
            var req = new UpdateControlUiRequest {
                Id = state.Id,
                PropertyName = "Value",
                PropertyValue = new double[] { pt.X, pt.Y }
            };

            _uiBus.Handle(req, out ControlUiEntity ui);
        }

        public void Interpolate(ControlUiState state, Point pt, bool hasExtra = true) {
            var req = new UpdateControlUiRequest {
                Id = state.Id,
                PropertyName = "Value",
                PropertyValue = new double[] {pt.X, pt.Y }
            };

            _uiBus.Handle(req, out ControlUiEntity ui);

            var req0 = new ListNLinearMapRequest {
                Id = SelectedMap.Id,
            };

            _mapBus.Handle(req0, out NLinearMapEntity map);

            _debugInfo.Clear();
            MapTo(map, hasExtra);
            Debug = _debugInfo.ToString();

            _logger.Debug(_debugInfo.ToString());
        }

        private StringBuilder _debugInfo = new();

        private string _debug;
        public string Debug {
            get => _debug;
            set { SetProperty(ref _debug, value); }
        }

        // add a logger
        private readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();



        public void MapTo(NLinearMapEntity map, bool hasExtra = true) {

            var lambdas = new List<double[]>();

            for (int i = 0; i < map.InputPorts.Length; i++) {
                var input = map.InputPorts[i];
                var req = new ListControlUiRequest() {
                    Id = input.Id,
                };
                _uiBus.Handle(req, out ControlUiEntity ui);

                var pt = new Point(ui.Value[0], ui.Value[1]);
                var regions = ui.Regions;

                var hit = hasExtra ? 
                    regions.Where(e => e.HitTest(pt) != null).OfType<SimplexRegionEntity>().FirstOrDefault() :
                    regions.Where(e => e.HitTest(pt) != null).FirstOrDefault();
                //var hit = regions.Where(e => e.HitTest(pt) != null).OfType<SimplexRegionEntity>().FirstOrDefault();

                if (hit != null) {
                    lambdas.Add(hit.GetLambdas(pt, ui.Nodes));
                }

            }

            if (lambdas.Count != map.InputPorts.Length) return;
            //Debug.WriteLine($"lambdas: {string.Join(",", lambdas)}");

            var results = map.MapTo(lambdas.ToArray()).GetData<double>();

            int idx = 0;
            var values = SelectedMap.OutputPorts.Select(e => {
                var returnValue = results.Skip(idx).Take(e.Dimension).ToArray();

                idx += e.Dimension;

                return returnValue;
            }).ToArray().GetEnumerator();

            foreach (var output in map.OutputPorts) {
                values.MoveNext();

                if (output.Name.Contains("Motor")) {
                    var id = output.Id;

                    var req1 = new UpdateMotorRequest {
                        Id = id,
                        Value = values.Current
                    };

                    _motorBus.Handle(req1, out MotorEntity motorEntity);

                    var target = MotorStates.Where(e => e.Id == id).FirstOrDefault();

                    _mapper.Map(motorEntity, target);

                    _debugInfo.Append($"{output.Name}: {motorEntity.Value[0]} | ");
                }
                else {
                    var req1 = new ListNLinearMapRequest {
                        Id = output.Id,
                    };

                    _mapBus.Handle(req1, out NLinearMapEntity mapEntity);

                    var uis = mapEntity.InputPorts.Select(e => UiStates.Where(e1 => e1.Id == e.Id).FirstOrDefault()).ToArray();

                    for (int i = 0; i < uis.Length; i++) {
                        var req2 = new UpdateControlUiRequest {
                            Id = uis[i].Id,
                            PropertyName = "Value",
                            PropertyValue = (values.Current as double[]).Skip(i * output.Dimension).Take(output.Dimension).ToArray(),
                        };

                        _uiBus.Handle(req2, out ControlUiEntity ui);
                    }

                    //Debug.WriteLine($"{output.Name}");
                    MapTo(mapEntity);

                    //Debug.WriteLine($"{output.Name}: {mapEntity.Value[0]} {mapEntity.Value[1]}");
                }
            }
        }

        [RelayCommand]
        public void UpdateMotor(MotorState state) {
            var req = new UpdateMotorRequest {
                Id = state.Id,
                Value = state.Value,
            };

            _motorBus.Handle(req, out MotorEntity motor);

            var target = MotorStates.Where(e => e.Id == state.Id).FirstOrDefault();

            _mapper.Map(motor, target);
        }

        public void InvalidateValidPorts() {
            ListMotors();
            ListMaps();
            ListUis();

            InvalidateValidInputPorts();
            InvalidateValidOutputPorts();
        }

        public void InvalidateValidInputPorts() {
            ValidInputPorts = new List<IInputPortState>()
                .Concat(UiStates)
                .Select(e => new DisplayInputPort(e))
                .ToArray();
        }

        public void InvalidateValidOutputPorts() {
            ValidOutputPorts = new List<IOutputPortState>()
                .Concat(MotorStates)
                .Concat(MapStates)
                .Select(e => new DisplayOutputPort(e))
                .ToArray();
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

            var result = indices.Select(map.HasSet);

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