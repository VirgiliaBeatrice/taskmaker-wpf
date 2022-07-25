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

namespace taskmaker_wpf.ViewModels
{
    public class StatefulNode : BindableBase {
        private NodeM _target;

        private Point _location;
        public Point Location {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        private bool _isSet;
        public bool IsSet {
            get => _isSet;
            set => SetProperty(ref _isSet, value);
        }

        public StatefulNode(NodeM target) {
            _target = target;
            _location = _target.Location.ToPoint();
            _isSet = _target.IsSet;
        }

        public NodeM GetNode() => _target;
    }

    public class StatefulSimplex : BindableBase, ITraceRegion {
        private SimplexM _target;

        private Point[] _points;
        public Point[] Points {
            get => _points;
            set => SetProperty(ref _points, value);
        }

        public StatefulSimplex(SimplexM target) {
            _target = target;

            _points = _target.Nodes
                .Select(e => e.Location.ToPoint())
                .ToArray();
        }

        public SimplexM GetSimplex() => _target;

        public object GetRef() => _target;
    }

    public class StatefulVoronoi : BindableBase, ITraceRegion {
        private VoronoiRegionM _target;

        private Point[] _points;
        public Point[] Points {
            get => _points;
            set => SetProperty(ref _points, value);
        }

        public StatefulVoronoi(VoronoiRegionM target) {
            _target = target;

            _points = _target.Vertices
                .Select(e => e.ToPoint())
                .ToArray();
        }

        public VoronoiRegionM GetSimplex() => _target;
        public object GetRef() => _target;
    }

    public interface ITraceRegion {
        object GetRef();
    }


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

        private NLinearMap _map => UI.Map;

        public TargetsPanelViewModel TargetsPanelVM { get; set; }

        private string _statusMsg;
        private string _systemInfo;

        private SystemService _systemSvr;

        private ObservableCollection<StatefulNode> _nodes;
        public ObservableCollection<StatefulNode> Nodes {
            get => _nodes;
            set => SetProperty(ref _nodes, value);
        }

        private ObservableCollection<StatefulSimplex> _simplices;
        public ObservableCollection<StatefulSimplex> Simplices {
            get => _simplices;
            set => SetProperty(ref _simplices, value);
        }

        private ObservableCollection<StatefulVoronoi> _voronois;
        public ObservableCollection<StatefulVoronoi> Voronois {
            get { return _voronois; }
            set { SetProperty(ref _voronois, value); }
        }

        private NodeWidget _selectedNodeWidget;
        public NodeWidget SelectedNodeWidget {
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


        private DelegateCommand<Point?> _addNodeCommand;
        public DelegateCommand<Point?> AddNodeCommand =>
            _addNodeCommand ?? (_addNodeCommand = new DelegateCommand<Point?>(ExecuteAddNodeCommand));

        void ExecuteAddNodeCommand(Point? pt) {
            if (pt == null) return;

            var node = UI.AddNode(pt?.ToNDarray());
            var newStatefulNode = new StatefulNode(node);

            _nodes.Add(newStatefulNode);
        }

        private DelegateCommand _buildCommand;
        public DelegateCommand BuildCommand =>
            _buildCommand ?? (_buildCommand = new DelegateCommand(ExecuteBuildCommand));

        void ExecuteBuildCommand() {
            UI.Complex.Build();

            if (Simplices == null)
                Simplices = new ObservableCollection<StatefulSimplex>();
            if (Voronois == null)
                Voronois = new ObservableCollection<StatefulVoronoi>();

            Simplices.Clear();
            Voronois.Clear();

            var simplices = UI.Complex.Simplices
                .Select(e => new StatefulSimplex(e));
            var voronois = UI.Complex.Regions
                .Select(e => new StatefulVoronoi(e));

            Simplices.AddRange(simplices);
            Voronois.AddRange(voronois);

            if (UI.Complex.Targets.Count != 0) {
                UI.SetMap();
            }
        }


        // https://blog.csdn.net/jiuzaizuotian2014/article/details/104856673

        private DelegateCommand<object> _interpolateCommand;

        private OperationMode _operationMode = Views.OperationMode.Default;
        public OperationMode OperationMode {
            get => _operationMode;
            set => SetProperty(ref _operationMode, value);
        }

        private MotorAgent _motorAgent;

        public RegionControlUIViewModel(
            SystemService systemService,
            MotorAgent motorAgent
            ) {
            _systemSvr = systemService;
            _motorAgent = motorAgent;


            TargetsPanelVM = new TargetsPanelViewModel(_systemSvr);
            //TargetsPanelVM.UI = _ui;
            SystemInfo = $"{_operationMode}";
        }


        public ComplexM Model { get; set; }

        public FrameworkElement InspectedWidget
        {
            get { return _inspectedWidget; }
            set { SetProperty(ref _inspectedWidget, value); }
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

        private DelegateCommand<NodeM> _removeNodeCommand;
        public DelegateCommand<NodeM> RemoveNodeCommand =>
            _removeNodeCommand ?? (_removeNodeCommand = new DelegateCommand<NodeM>(ExecuteRemoveNodeCommand));

        void ExecuteRemoveNodeCommand(NodeM parameter) {
            RemoveNode(parameter);
        }

        private DelegateCommand _setValueCommand;
        public DelegateCommand SetValueCommand =>
            _setValueCommand ?? (_setValueCommand = new DelegateCommand(ExecuteSetValueCommand));

        void ExecuteSetValueCommand() {
            if (SelectedNodeWidget is null) return;

            SetNodeValue(SelectedNodeWidget.DataContext as StatefulNode);
        }

        private void SetNodeValue(StatefulNode sNode) {
            var node = sNode.GetNode();
            var targetValue = UI.Complex.Targets.ToNDarray();
            //var idx = Model.Nodes.FindIndex(e => e.Uid == node.Uid);
            var idx = UI.Complex.Nodes.FindIndex(e => e == node);
            node.IsSet = true;
            sNode.IsSet = true;

            UI.Map.SetValue(new[] { idx }, targetValue);
        }

        private void RemoveNode(NodeM node) {
            UI.Complex.Nodes.Remove(node);
        }

        //private void Interpolate(object arg)
        //{
        //    var args = (object[])arg;

        //    var pt = (Point)args[0];
        //    var targetId = (Guid?)args[1];

        //    if (targetId is null) return;
        //    else
        //    {
        //        var id = (Guid)targetId;
        //        var targetBary = Model.FindRegionById(id).Bary;

        //        var lambdas = Model.Bary.GetLambdas(targetBary, pt.ToNDarray());
        //        //_map.Barys
        //        var result = _map.MapTo(lambdas);
        //        Model.Targets.SetValue(result.GetData<double>());

        //        Debug = Model.Targets.ToString();
        //    }
        //}

        private void Interpolate() {
            var pt = TracePoint;

            var target = HitElement?.DataContext as ITraceRegion;

            if (target is null) return;
            else {
                if (target is StatefulSimplex) {
                    var bary = (target.GetRef() as SimplexM).Bary;
                    var lambdas = UI.Complex.Bary.GetLambdas(bary, pt.ToNDarray());
                    var result = _map.MapTo(lambdas);

                    UI.Complex.Targets.SetValue(result.GetData<double>());
                }
                else if (target is StatefulVoronoi) {
                    var bary = (target.GetRef() as VoronoiRegionM).Bary;
                    var lambdas = UI.Complex.Bary.GetLambdas(bary, pt.ToNDarray());
                    var result = _map.MapTo(lambdas);

                    UI.Complex.Targets.SetValue(result.GetData<double>());
                }

                Debug = UI.Complex.Targets.ToString();
            }
        }

        //private void PerformSelectedTargetsChanged(IList<object> param)
        //{
        //    Model.Targets.Clear();
        //    Model.Targets.AddRange(param.OfType<ISelectableTarget>());

        //    SelectedTargets = Model.Targets.ToArray();
        //}

        private ControlUi _ui;
        private ControlUi UI {
            get => _ui;
            set {
                _ui = value;
                TargetsPanelVM.UI = _ui;
                Nodes = new ObservableCollection<StatefulNode>(
                    _ui.Complex.Nodes.Select(e => new StatefulNode(e)));
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            UI = navigationContext.Parameters["ui"] as ControlUi;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}