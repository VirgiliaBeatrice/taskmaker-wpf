using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using taskmaker_wpf.Model;
using taskmaker_wpf.Views;
using taskmaker_wpf.Views.Widgets;
using Numpy;
using taskmaker_wpf.Views.Debug;
using System.Windows;
using System.Windows.Input;
using SkiaSharp.Views.WPF;
using Prism.Mvvm;
using Prism.Commands;
using taskmaker_wpf.Services;
using System.Reactive.Subjects;
using System.Collections.ObjectModel;
using taskmaker_wpf.Model.Data;
using System.Windows.Controls;
using taskmaker_wpf.Views.Widget;

namespace taskmaker_wpf.ViewModels {
    public class NodeData : BindableBase, IInspectorTarget {
        private Guid _uid;
        public Guid Uid {
            get { return _uid; }
            set => SetProperty(ref _uid, value);
        }

        private Point _location;
        public Point Location { 
            get => _location; 
            set => SetProperty(ref _location, value); 
        }

        private bool _isSet;
        public bool IsSet {
            get { return _isSet; }
            set => SetProperty(ref _isSet, value);
        }
    }

    public class SimplexData : BindableBase {
        private Guid _uid;
        public Guid Uid {
            get { return _uid; }
            set => SetProperty(ref _uid, value);
        }

        private Point[] _points;
        public Point[] Points {
            get => _points;
            set => SetProperty(ref _points, value);
        }
    }

    public class VoronoiData : BindableBase {
        private Guid _uid;
        public Guid Uid {
            get { return _uid; }
            set => SetProperty(ref _uid, value);
        }

        private Point[] _points;
        public Point[] Points {
            get => _points;
            set => SetProperty(ref _points, value);
        }
    }


    public static class Helper {
        static public Point ToPoint(this NDarray pt) {
            if (pt.ndim > 1)
                throw new Exception("Invalid ndarray");

            var castValues = pt.astype(np.float32);
            var values = castValues.GetData<float>();

            castValues.Dispose();

            return new Point { X = values[0], Y = values[1] };
        }

        static public SKPoint ToSKPoint(this NDarray pt) {
            if (pt.ndim > 1)
                throw new Exception("Invalid ndarray");

            var castValues = pt.astype(np.float32);
            var values = castValues.GetData<float>();

            castValues.Dispose();

            return new SKPoint(values[0], values[1]);
        }

        static public NDarray<float> ToNDarray(this Point pt) {
            return np.array((float)pt.X, (float)pt.Y);
        }

        static public NDarray<float> ToNDarray(this SKPoint pt) {
            return np.array(pt.X, pt.Y);
        }

        static public IObservable<TSource> Debug<TSource>(this IObservable<TSource> observable) {
            observable.Subscribe(
                (e) => {
                    Console.WriteLine($"[{DateTime.Now}] OnNext({e})");
                },
                (e) => {
                    Console.WriteLine($"[{DateTime.Now}] OnError({e})");
                },
                () => {
                    Console.WriteLine($"[{DateTime.Now}] OnCompleted()");
                });

            return observable;
        }

        public static void Dump<T>(this IObservable<T> source, string name) {
            source.Subscribe(
               i => Console.WriteLine("{0}-->{1}", name, i),
               ex => Console.WriteLine("{0} failed-->{1}", name, ex.Message),
               () => Console.WriteLine("{0} completed", name));
        }
    }

    public class RegionControlUIViewModel : BindableBase {
        public ComplexM Model { get; set; }

        //private TargetService _targetSvr;
        private SystemService _systemSvr;

        #region Bindable Properties
        private int _count;

        public int Count {
            get { return _count; }
            set { SetProperty(ref _count, value); }
        }


        #endregion
        private string _keymapInfo;
        private string _systemInfo;
        private string _statusMsg;

        private NLinearMap _map;

        /// <summary>
        /// Targets loaded from system service
        /// </summary>
        private ITarget[] _targets;
        public ITarget[] Targets {
            get => _targets;
            set => SetProperty(ref _targets, value);
        }

        /// <summary>
        /// Targets being selected
        /// </summary>
        private ITarget[] _selectedTargets;
        public ITarget[] SelectedTargets {
            get => _selectedTargets;
            set => SetProperty(ref _selectedTargets, value);
        }

        private Guid _selectedNode;

        //public ObservableCollection<ITarget> ValidTargets { get; set; } = new ObservableCollection<ITarget>();

        public ObservableCollection<NodeData> Nodes { get; set; } = new ObservableCollection<NodeData>();

        private SimplexData[] _simplices;
        public SimplexData[] Simplices {
            get { return _simplices; }
            set { SetProperty(ref _simplices, value); }
        }

        private VoronoiData[] _voronois;
        public VoronoiData[] Voronois {
            get { return _voronois; }
            set { SetProperty(ref _voronois, value); }
        }

        public RegionControlUIViewModel(
            SystemService systemService) {

            //_targetSvr = targetService;
            _systemSvr = systemService;


            Model = new ComplexM();
            _systemSvr.Complexes.Add(Model);

            //var target = new BinableTargetCollection();

            // Update targets from service
            Targets = _systemSvr.Targets.ToArray();

            foreach (var item in _systemSvr.Targets.OfType<BindableBase>()) {
                item.PropertyChanged += Item_PropertyChanged;
            }
            SelectedTargets = Model.Targets.ToArray();

            SystemInfo = $"{operationMode}";
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs args) {
            if (args.PropertyName == nameof(Motor.IsSelected)) {
                Model.Targets.Clear();

                Model.Targets.AddRange(
                    _systemSvr.Targets
                    .Where(e => e.IsSelected));

                SelectedTargets = Model.Targets.ToArray();
            }
        }

        private void CreateComplex() {
            Model.CreateComplex();

            Simplices = Model.GetSimplexData();
            Voronois = Model.GetVoronoiData();

            // TODO: For test purpose
            _map = new NLinearMap(
                new ComplexBaryD[] { Model.Bary },
                Model.Targets.Dim);

            _systemSvr.Maps.Add(_map);
        }

        private void CreateMap() {
            _map = new NLinearMap(
                new ComplexBaryD[] { Model.Bary },
                Model.Targets.Dim);

            _systemSvr.Maps.Add(_map);
        }

        private void SetValue(Guid? id) {
            if (id is null) return;

            var values = Model.Targets.ToNDarray();
            var targetNode = Model.Nodes.Find(e => e.Uid == id);
            var idx = Model.Nodes.FindIndex(e => e.Uid == id);

            targetNode.IsSet = true;

            _map.SetValue(new[] { idx }, values);

            var node = Nodes.Where(e => e.Uid == id).First();
            node.IsSet = true;
        }

        private void CreateInterior() {
            CreateComplex();
            //Model.CreateComplex();
            //Model.CreateRegions();

            //Model.Complex.Simplices.ForEach(e => e.SetBary());

            //var data = Model.GetSimplexCollectionData();

            //Simplices = data;
        }

        private void CreateExterior() {
            //var data = Model.GetVoronoiCollectionData();

            //Voronois = data;
        }

        private string _debug;
        public string Debug {
            get => _debug;
            set => SetProperty(ref _debug, value);
        }

        private object[] _inspectedTargets;
        public object[] InspectedTargets {
            get => _inspectedTargets;
            set => SetProperty(ref _inspectedTargets, value);
        }


        private OperationMode operationMode;

        public OperationMode OperationMode { get => operationMode; set => SetProperty(ref operationMode, value); }
        public string KeymapInfo { get => _keymapInfo; set => SetProperty(ref _keymapInfo, value); }
        public string SystemInfo { get => _systemInfo; set => SetProperty(ref _systemInfo, value); }
        public string StatusMsg { get => _statusMsg; set => SetProperty(ref _statusMsg, value); }

        // https://blog.csdn.net/jiuzaizuotian2014/article/details/104856673
        private DelegateCommand<object> addItemCommand;

        public ICommand AddItemCommand {
            get {
                if (addItemCommand == null) {
                    addItemCommand = new DelegateCommand<object>(AddItem);
                }

                return addItemCommand;
            }
        }

        private void AddItem(object pt) {
            var value = ((Point)pt).ToNDarray();
            var node = Model.Add(value);

            Nodes.Add(node.ToData());
        }

        private DelegateCommand<object> removeItemCommand;

        public ICommand RemoveItemCommand {
            get {
                if (removeItemCommand == null) {
                    removeItemCommand = new DelegateCommand<object>(RemoveItem);
                }

                return removeItemCommand;
            }
        }

        private void RemoveItem(object obj) {
            var uid = (Guid)obj;

            Model.RemoveAt(uid);

            Nodes.Remove(Nodes.Where(e => e.Uid == uid).First());
        }

        private DelegateCommand buildInteriorCommand;

        public ICommand BuildInteriorCommand {
            get {
                if (buildInteriorCommand == null) {
                    buildInteriorCommand = new DelegateCommand(BuildInterior);
                }

                return buildInteriorCommand;
            }
        }

        private void BuildInterior() {
            CreateInterior();
        }

        private DelegateCommand buildExteriorCommand;

        public ICommand BuildExteriorCommand {
            get {
                if (buildExteriorCommand == null) {
                    buildExteriorCommand = new DelegateCommand(BuildExterior);
                }

                return buildExteriorCommand;
            }
        }

        private void BuildExterior() {
            CreateExterior();
        }

        private DelegateCommand<object> interpolateCommand;

        public ICommand InterpolateCommand {
            get {
                if (interpolateCommand == null) {
                    interpolateCommand = new DelegateCommand<object>(Interpolate);
                }

                return interpolateCommand;
            }
        }

        private void Interpolate(object arg) {
            var args = (object[])arg;

            var pt = (Point)args[0];
            var targetId = (Guid?)args[1];

            if (targetId is null) return;
            else {
                var id = (Guid)targetId;
                var targetBary = Model.FindRegionById(id).Bary;

                var lambdas = Model.Bary.GetLambdas(targetBary, pt.ToNDarray());
                //_map.Barys
                var result = _map.MapTo(lambdas);
                Model.Targets.SetValue(result.GetData<double>());

                Debug = Model.Targets.ToString();
            }

        }

        private DelegateCommand<IList<object>> selectedTargetsChanged;

        public ICommand SelectedTargetsChanged {
            get {
                if (selectedTargetsChanged == null) {
                    selectedTargetsChanged = new DelegateCommand<IList<object>>(PerformSelectedTargetsChanged);
                }

                return selectedTargetsChanged;
            }
        }

        private void PerformSelectedTargetsChanged(IList<object> param) {
            Model.Targets.Clear();
            Model.Targets.AddRange(param.OfType<ISelectableTarget>());

            SelectedTargets = Model.Targets.ToArray();
        }

        private DelegateCommand<Guid?> setValueCommand;

        public ICommand SetValueCommand {
            get {
                if (setValueCommand == null) {
                    setValueCommand = new DelegateCommand<Guid?>(SetValue);
                }

                return setValueCommand;
            }
        }

        private DelegateCommand<Guid?> setInspectedObject;

        public ICommand SetInspectedObjectCommand {
            get {
                if (setInspectedObject == null) {
                    setInspectedObject = new DelegateCommand<Guid?>(SetInspectedObject);
                }

                return setInspectedObject;
            }
        }

        private void SetInspectedObject(Guid? obj) {
            if (obj is null) return;

            var target = Nodes.ToList().Find(e => e.Uid == obj);

            InspectedTargets = new object[] {
                target
            };
            //InspectedTargets.Clear();
            //InspectedTargets.Add(target);
        }
    }
}
