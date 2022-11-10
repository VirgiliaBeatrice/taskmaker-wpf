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
using taskmaker_wpf.Views.Widget;

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

    public class RegionControlUIViewModel : BindableBase, INavigationAware {

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
        private ControlUiState _ui;
        private ControlUiState[] _uis;
        private ControlUiState _uiState;

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
                        Uis = _mapper.Map<ControlUiState[]>(uis);

                        TargetsPanelVM.InvalidateTargets(); 
                    });
                });
            }
        }

        private DelegateCommand _addUiCommand;
        public DelegateCommand AddUiCommand =>
            _addUiCommand ?? (_addUiCommand = new DelegateCommand(ExecuteAddUiCommand));

        void ExecuteAddUiCommand() {
            var request = new AddControlUiRequest();
            var request1 = new ListControlUiRequest();

            _uiBus.Handle(request, (bool res) => { });
            _uiBus.Handle(request1, (ControlUiEntity[] uis) => {
                Uis = _mapper.Map<ControlUiState[]>(uis);
            });
        }

        public DelegateCommand<object> AddNodeCommand =>
            _addNodeCommand ?? (_addNodeCommand = new DelegateCommand<object>(ExecuteAddNodeCommand));

        public DelegateCommand BuildCommand =>
            _buildCommand ?? (_buildCommand = new DelegateCommand(ExecuteBuildCommand));

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

        public NLinearMapState[] Maps {
            get => _maps;
            set => SetProperty(ref _maps, value);
        }

        public NLinearMapState MapState {
            get => _mapState;
            set {
                SetProperty(ref _mapState, value);

                if (value != null)
                    TargetsPanelVM.InvalidateTargets();
            }
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

        public TargetsPanelViewModel TargetsPanelVM { get; set; }

        public Point TracePoint {
            get => _tracePoint;
            set {
                SetProperty(ref _tracePoint, value);

                if (_operationMode == OperationMode.Trace) {
                    Interpolate();
                }
            }
        }

        public ControlUiState[] Uis {
            get => _uis;
            set => SetProperty(ref _uis, value);
        }

        public ControlUiState UiState {
            get => _uiState;
            set => SetProperty(ref _uiState, value);
        }


        public RegionControlUIViewModel(MapperConfiguration config,
            ControlUiInteractorBus uiBus,
            NLinearMapInteractorBus mapBus,
            MotorInteractorBus motorBus,
            ListTargetInteractor targetInteractor) {
            _mapper = config.CreateMapper();

            TargetsPanelVM = new TargetsPanelViewModel(this, targetInteractor, motorBus, uiBus, mapBus, config);
            _uiBus = uiBus;
            _mapBus = mapBus;
            _motorBus = motorBus;

            //InvalidateUi();
            //InvalidateMap();

            SystemInfo = $"{_operationMode}";
        }

        private void ExecuteAddNodeCommand(object param) {
            var pt = (Point)param;
            if (pt == null) return;
            else {
                var request = new AddNodeRequest {
                    UiId = UiState.Id,
                    Value = pt,
                };
                //var request = new UpdateControlUibRequest {
                //    Id = UiState.Id,
                //};
                _uiBus.Handle(request, (bool res) => {
                    _uiBus.Handle(new ListControlUiRequest(), (ControlUiEntity[] uis) => {
                        var entity = uis.Where(e => e.Id == UiState.Id).FirstOrDefault();

                        if (entity != null) {
                            UiState = _mapper.Map<ControlUiState>(entity);
                        }
                    });
                });

                //_uiBus.Handle(new UpdateControlUiRequest(), (bool res) => { });
                //var uiEntity = _uiUseCase.GetControlUi(UiState.Id);

                //_mapper.Map(uiEntity, UiState);
            }
        }

        private void ExecuteBuildCommand() {
            var request = new BuildRegionRequest() {
                Id = UiState.Id,
            };

            _uiBus.Handle(request, (ControlUiEntity ui) => {
                UiState = _mapper.Map<ControlUiState>(ui);
            });

            //_mapUseCase.SetBasisDim(TargetsPanelVM.SelectedMap.Id, new int[] { UiState.Nodes.Length });
            //_mapUseCase.InitializeTensor(TargetsPanelVM.SelectedMap.Id);
        }

        private void ExecuteRemoveNodeCommand(NodeState parameter) {
            RemoveNode(parameter);
        }

        private void ExecuteSetValueCommand(object param) {
            if (param is int selectedNodeId) {
                var targetValues = TargetsPanelVM.TargetsOfSelectedMap.SelectMany(e => e.Value).ToArray();
                //var value = TargetsPanelVM.TargetMotors

                var request = new UpdateNodeRequest {
                    UiId = UiState.Id,
                    NodeId = selectedNodeId,
                    PropertyName = "TargetValue",
                    PropertyValue = targetValues,
                };

                _uiBus.Handle(request, (ControlUiEntity ui) => {
                    UiState = _mapper.Map<ControlUiState>(ui);
                });
            }
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

        private void InvalidateMap() {
            _mapBus.Handle(new ListNLinearMapRequest(), (NLinearMapEntity[] maps) => {
                Maps = _mapper.Map<NLinearMapState[]>(maps);
                MapState = Maps.FirstOrDefault();
            });
        }

        private void InvalidateUi() {
            _uiBus.Handle(new ListControlUiRequest(), (ControlUiEntity[] uis) => {
                Uis = _mapper.Map<ControlUiState[]>(uis);
                UiState = Uis.First();
            });
            //_uiBus.Handle
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