using Numpy;
using Prism.Commands;
using Prism.Mvvm;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using Prism.Regions;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Models;
using taskmaker_wpf.Services;
using taskmaker_wpf.Views;
using taskmaker_wpf.Views.Widget;
using taskmaker_wpf.Domain;
using AutoMapper;

namespace taskmaker_wpf.ViewModels {
    public class ControlUiState : BindableBase {
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

        private double[] _value;
        public double[] Value {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private NodeState[] _nodes;
        public NodeState[] Nodes {
            get => _nodes;
            set => SetProperty(ref _nodes, value);
        }

        private RegionState[] _regions;
        public RegionState[] Regions {
            get => _regions;
            set => SetProperty(ref _regions, value);
        }

        public override string ToString() {
            return $"ControlUI[{Name}]";
        }
    }

    public class NodeState : BindableBase, IEquatable<NodeState> {

        private int _id;
        public int Id {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private Point _value;
        public Point Value {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private bool _isSet;
        public bool IsSet {
            get => _isSet;
            set => SetProperty(ref _isSet, value);
        }

        public bool Equals(NodeState other) {
            return other.Id == Id;
        }
    }

    public class RegionState: BindableBase {
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

    public interface ITraceRegion { }


    public static class Helper
    {
        public static Point ToPoint(this NDarray pt)
        {
            if (pt.ndim > 1)
                throw new Exception("Invalid ndarray");

            var castValues = pt.astype(np.float32);
            var values = castValues.GetData<float>();

            castValues.Dispose();

            return new Point { X = values[0], Y = values[1] };
        }

        public static SKPoint ToSKPoint(this NDarray pt)
        {
            if (pt.ndim > 1)
                throw new Exception("Invalid ndarray");

            var castValues = pt.astype(np.float32);
            var values = castValues.GetData<float>();

            castValues.Dispose();

            return new SKPoint(values[0], values[1]);
        }

        public static NDarray<float> ToNDarray(this Point pt)
        {
            return np.array((float)pt.X, (float)pt.Y);
        }

        public static NDarray<float> ToNDarray(this SKPoint pt)
        {
            return np.array(pt.X, pt.Y);
        }

        public static IObservable<TSource> Debug<TSource>(this IObservable<TSource> observable)
        {
            observable.Subscribe(
                (e) => { Console.WriteLine($"[{DateTime.Now}] OnNext({e})"); },
                (e) => { Console.WriteLine($"[{DateTime.Now}] OnError({e})"); },
                () => { Console.WriteLine($"[{DateTime.Now}] OnCompleted()"); });

            return observable;
        }

        public static void Dump<T>(this IObservable<T> source, string name)
        {
            source.Subscribe(
                i => Console.WriteLine("{0}-->{1}", name, i),
                ex => Console.WriteLine("{0} failed-->{1}", name, ex.Message),
                () => Console.WriteLine("{0} completed", name));
        }
    }

    public class RegionControlUIViewModel : BindableBase, INavigationAware
    {
        private string _debug;

        private FrameworkElement _inspectedWidget;

        private string _keymapInfo;

        private ControlUiEntity _ui;
        public ControlUiEntity UI {
            get => _ui;
            set => SetProperty(ref _ui, value);
        }

        private ControlUiState _uiState;
        public ControlUiState UiState {
            get => _uiState;
            set => SetProperty(ref _uiState, value);
        }

        public TargetsPanelViewModel TargetsPanelVM { get; set; }

        private string _statusMsg;
        private string _systemInfo;

        private FrameworkElement _selectedNodeWidget;
        public FrameworkElement SelectedNodeWidget {
            get => _selectedNodeWidget;
            set => SetProperty(ref _selectedNodeWidget, value);
        }

        private Point _tracePoint = new Point();
        public Point TracePoint {
            get => _tracePoint;
            set {
                SetProperty(ref _tracePoint, value);

                if (_operationMode == OperationMode.Trace) {
                    Interpolate();
                }
            }
        }

        private FrameworkElement _hitElement;
        public FrameworkElement HitElement {
            get => _hitElement;
            set => SetProperty(ref _hitElement, value);
        }


        private DelegateCommand<object> _addNodeCommand;
        public DelegateCommand<object> AddNodeCommand =>
            _addNodeCommand ?? (_addNodeCommand = new DelegateCommand<object>(ExecuteAddNodeCommand));

        void ExecuteAddNodeCommand(object param) {
            var pt = (Point)param;
            if (pt == null) return;
            else {
                var request = new UpdateControlUiRequest {
                    Id = UiState.Id,
                };
                _uiBus.Handle(new UpdateControlUiRequest(), (bool res) => { });
                _uiBus.Handle(new ListControlUiRequest(), (ControlUiEntity[] uis) => { });

                //var uiEntity = _uiUseCase.GetControlUi(UiState.Id);

                //_mapper.Map(uiEntity, UiState);
            }

        }

        private void InvalidateUi() {
            //_uiBus.Handle
        }

        private DelegateCommand _buildCommand;
        public DelegateCommand BuildCommand =>
            _buildCommand ?? (_buildCommand = new DelegateCommand(ExecuteBuildCommand));

        void ExecuteBuildCommand() {
            //_buildUseCase.Build(UiState.Id);

            //var uiEntity = _uiUseCase.GetControlUi(UiState.Id);

            //_mapper.Map(uiEntity, UiState);

            //_mapUseCase.SetBasisDim(TargetsPanelVM.SelectedMap.Id, new int[] { UiState.Nodes.Length });
            //_mapUseCase.InitializeTensor(TargetsPanelVM.SelectedMap.Id);
        }


        // https://blog.csdn.net/jiuzaizuotian2014/article/details/104856673

        private DelegateCommand<object> _interpolateCommand;

        private OperationMode _operationMode = Views.OperationMode.Default;
        public OperationMode OperationMode {
            get => _operationMode;
            set => SetProperty(ref _operationMode, value);
        }

        private readonly IMapper _mapper;
        //private readonly ControlUiUseCase _uiUseCase;
        //private readonly BuildRegionUseCase _buildUseCase;
        //private readonly NLinearMapUseCase _mapUseCase;
        //private readonly MotorUseCase _motorUseCase;
        private readonly ControlUiInteractorBus _uiBus;
        private readonly NLinearMapInteractorBus _mapBus;
        private readonly MotorInteractorBus _motorBus;

        public RegionControlUIViewModel(
            MapperConfiguration config,
            ControlUiInteractorBus uiBus,
            NLinearMapInteractorBus mapBus,
            MotorInteractorBus motorBus,
            ListTargetInteractor targetInteractor) {
            _mapper = config.CreateMapper();

            TargetsPanelVM = new TargetsPanelViewModel(targetInteractor, motorBus, uiBus, mapBus, config);
            _uiBus = uiBus;
            _mapBus = mapBus;
            _motorBus = motorBus;

            SystemInfo = $"{_operationMode}";
        }

        public string Debug
        {
            get => _debug;
            set => SetProperty(ref _debug, value);
        }

        public string KeymapInfo
        {
            get => _keymapInfo;
            set => SetProperty(ref _keymapInfo, value);
        }

        public string SystemInfo
        {
            get => _systemInfo;
            set => SetProperty(ref _systemInfo, value);
        }

        public string StatusMsg
        {
            get => _statusMsg;
            set => SetProperty(ref _statusMsg, value);
        }

        private DelegateCommand<NodeState> _removeNodeCommand;
        public DelegateCommand<NodeState> RemoveNodeCommand =>
            _removeNodeCommand ?? (_removeNodeCommand = new DelegateCommand<NodeState>(ExecuteRemoveNodeCommand));

        void ExecuteRemoveNodeCommand(NodeState parameter) {
            RemoveNode(parameter);
        }

        private DelegateCommand _setValueCommand;
        public DelegateCommand SetValueCommand =>
            _setValueCommand ?? (_setValueCommand = new DelegateCommand(ExecuteSetValueCommand));

        void ExecuteSetValueCommand() {
            if (SelectedNodeWidget is null) return;
        }


        private void RemoveNode(NodeState node) {
            //UI.Complex.Nodes.Remove(node);
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

        public void OnNavigatedTo(NavigationContext navigationContext) {
            UiState = navigationContext.Parameters["ui"] as ControlUiState;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}