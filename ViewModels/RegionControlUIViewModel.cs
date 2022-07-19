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

        public StatefulNode(NodeM target) {
            _target = target;
            _location = _target.Location.ToPoint();

            _target.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(_target.Location)) {
                    Location = _target.Location.ToPoint();
                }
            };

        }

        public NodeM GetNode() => _target;
    }

    //public class NodeData : BindableBase, IInspectable
    //{
    //    private bool _isSet;

    //    private Point _location;
    //    private Guid _uid;

    //    public Guid Uid
    //    {
    //        get { return _uid; }
    //        set => SetProperty(ref _uid, value);
    //    }

    //    public Point Location
    //    {
    //        get => _location;
    //        set => SetProperty(ref _location, value);
    //    }

    //    public bool IsSet
    //    {
    //        get { return _isSet; }
    //        set => SetProperty(ref _isSet, value);
    //    }
    //}

    //public class SimplexData : BindableBase
    //{
    //    private Point[] _points;
    //    private Guid _uid;

    //    public Guid Uid
    //    {
    //        get { return _uid; }
    //        set => SetProperty(ref _uid, value);
    //    }

    //    public Point[] Points
    //    {
    //        get => _points;
    //        set => SetProperty(ref _points, value);
    //    }
    //}

    //public class VoronoiData : BindableBase
    //{
    //    private Point[] _points;
    //    private Guid _uid;

    //    public Guid Uid
    //    {
    //        get { return _uid; }
    //        set => SetProperty(ref _uid, value);
    //    }

    //    public Point[] Points
    //    {
    //        get => _points;
    //        set => SetProperty(ref _points, value);
    //    }
    //}


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
        private int _count;
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

        private DelegateCommand<Point?> _addNodeCommand;
        public DelegateCommand<Point?> AddNodeCommand =>
            _addNodeCommand ?? (_addNodeCommand = new DelegateCommand<Point?>(ExecuteAddNodeCommand));

        void ExecuteAddNodeCommand(Point? pt) {
            if (pt == null) return;

            var node = UI.AddNode(pt?.ToNDarray());
            var newStatefulNode = new StatefulNode(node);

            _nodes.Add(newStatefulNode);
        }


        //private VoronoiData[] _voronois;

        // https://blog.csdn.net/jiuzaizuotian2014/article/details/104856673
        private DelegateCommand<object> _addItemCommand;

        private DelegateCommand _buildExteriorCommand;

        private DelegateCommand _buildInteriorCommand;

        private DelegateCommand<object> _interpolateCommand;

        private OperationMode _operationMode;

        private DelegateCommand<IList<object>> _selectedTargetsChanged;


        public RegionControlUIViewModel(
            SystemService systemService) {
            _systemSvr = systemService;

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

        public OperationMode OperationMode
        {
            get => _operationMode;
            set => SetProperty(ref _operationMode, value);
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

        public ICommand BuildInteriorCommand
        {
            get
            {
                if (_buildInteriorCommand == null)
                {
                    _buildInteriorCommand = new DelegateCommand(BuildInterior);
                }

                return _buildInteriorCommand;
            }
        }

        public ICommand BuildExteriorCommand
        {
            get
            {
                if (_buildExteriorCommand == null)
                {
                    _buildExteriorCommand = new DelegateCommand(BuildExterior);
                }

                return _buildExteriorCommand;
            }
        }

        public ICommand InterpolateCommand
        {
            get
            {
                if (_interpolateCommand == null)
                {
                    _interpolateCommand = new DelegateCommand<object>(Interpolate);
                }

                return _interpolateCommand;
            }
        }


        private DelegateCommand<NodeM> _setNodeValueCommand;
        public DelegateCommand<NodeM> SetNodeValueCommand =>
            _setNodeValueCommand ?? (_setNodeValueCommand = new DelegateCommand<NodeM>(ExecuteSetNodeValueCommand));

        void ExecuteSetNodeValueCommand(NodeM parameter) {
            SetNodeValue(parameter);
        }

        public int Count
        {
            get { return _count; }
            set { SetProperty(ref _count, value); }
        }

        //private void Item_PropertyChanged(object sender, PropertyChangedEventArgs args)
        //{
        //    if (args.PropertyName == nameof(Motor.IsSelected))
        //    {
        //        Model.Targets.Clear();

        //        Model.Targets.AddRange(
        //            _systemSvr.Targets
        //                .Where(e => e.IsSelected));

        //        SelectedTargets = Model.Targets.ToArray();
        //        OnSelectedTargetsChanged();
        //    }
        //}

        private void OnSelectedTargetsChanged()
        {
            InvalidateMap();
            //CreateMap();
        }

        private void InvalidateMap() {
            _map.Initialize(
                new[] { Model.Bary },
                Model.Targets.Dim);
        }

        private void CreateComplex()
        {
            Model.CreateComplex();

            //Simplices = Model.GetSimplexData();
            //Voronois = Model.GetVoronoiData();

            // TODO: For test purpose
            // map has been created by system service
            _map.Initialize(
                new[] { Model.Bary },
                Model.Targets.Dim);
        }

        private void SetNodeValue(NodeM node) {
            var targetValue = UI.Complex.Targets.ToNDarray();
            var idx = Model.Nodes.FindIndex(e => e.Uid == node.Uid);

            node.IsSet = true;

            UI.Map.SetValue(new[] { idx }, targetValue);
        }

        //private void SetValue(Guid? id)
        //{
        //    if (id is null) return;

        //    var values = Model.Targets.ToNDarray();
        //    var targetNode = Model.Nodes.Find(e => e.Uid == id);
        //    var idx = Model.Nodes.FindIndex(e => e.Uid == id);

        //    targetNode.IsSet = true;

        //    _map.SetValue(new[] { idx }, values);

        //    var node = Nodes.First(e => e.Uid == id);
        //    node.IsSet = true;
        //}

        private void CreateInterior()
        {
            CreateComplex();
            //Model.CreateComplex();
            //Model.CreateRegions();

            //Model.Complex.Simplices.ForEach(e => e.SetBary());

            //var data = Model.GetSimplexCollectionData();

            //Simplices = data;
        }

        private void CreateExterior()
        {
            //var data = Model.GetVoronoiCollectionData();

            //Voronois = data;
        }

        private void AddNode(Point point) {
            var node = new NodeM {
                Location = point.ToNDarray()
            };

            UI.Complex.Nodes.Add(node);
        }

        private void RemoveNode(NodeM node) {
            UI.Complex.Nodes.Remove(node);
        }

        //private void AddItem(object pt)
        //{
        //    var value = ((Point)pt).ToNDarray();
        //    var node = Model.Add(value);

        //    Nodes.Add(node.ToData());
        //}

        //private void RemoveItem(object obj)
        //{
        //    var uid = (Guid)obj;

        //    Model.RemoveAt(uid);

        //    Nodes.Remove(Nodes.First(e => e.Uid == uid));
        //}

        private void BuildInterior()
        {
            CreateInterior();
        }

        private void BuildExterior()
        {
            CreateExterior();
        }

        private void Interpolate(object arg)
        {
            var args = (object[])arg;

            var pt = (Point)args[0];
            var targetId = (Guid?)args[1];

            if (targetId is null) return;
            else
            {
                var id = (Guid)targetId;
                var targetBary = Model.FindRegionById(id).Bary;

                var lambdas = Model.Bary.GetLambdas(targetBary, pt.ToNDarray());
                //_map.Barys
                var result = _map.MapTo(lambdas);
                Model.Targets.SetValue(result.GetData<double>());

                Debug = Model.Targets.ToString();
            }
        }

        //private void PerformSelectedTargetsChanged(IList<object> param)
        //{
        //    Model.Targets.Clear();
        //    Model.Targets.AddRange(param.OfType<ISelectableTarget>());

        //    SelectedTargets = Model.Targets.ToArray();
        //}

        private ControlUI _ui;
        private ControlUI UI {
            get => _ui;
            set {
                _ui = value;
                TargetsPanelVM.UI = _ui;
                Nodes = new ObservableCollection<StatefulNode>(
                    _ui.Complex.Nodes.Select(e => new StatefulNode(e)));
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            UI = navigationContext.Parameters["ui"] as ControlUI;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}