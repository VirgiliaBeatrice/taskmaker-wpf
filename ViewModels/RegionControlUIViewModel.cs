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

namespace taskmaker_wpf.ViewModels {
    public class Node : BindableBase {
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
        public Window Parent { get; set; }
        public Model.Core.UI Model { get; set; }
        public Views.Pages.SimplexView Page { get; set; }
        //public Model.Data.MotorCollection Motors { get; set; }

        //private MotorService _motorSvr;
        private TargetService _targetSvr;
        private SystemService _systemService;

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

        public ObservableCollection<IValue> ValidTargets { get; set; } = new ObservableCollection<IValue>();

        public ObservableCollection<Node> Nodes { get; set; } = new ObservableCollection<Node>();

        //public ObservableCollection<SimplexData> Simplices { get; set; } = new ObservableCollection<SimplexData> ();
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

        public DelegateCommand TestCommand { get; set; }


        public RegionControlUIViewModel(
            TargetService targetService,
            SystemService systemService,
            Window parent) {
            Parent = parent;
            //Model = new Model.Core.UI();

            _targetSvr = targetService;
            _systemService = systemService;

            Model = _systemService.UIs[0];

            //var target = new BinableTargetCollection();


            ValidTargets.AddRange(_targetSvr.Targets);

            SystemInfo = $"{operationMode}";
        }

        public void CommandTest() {
            Console.WriteLine("A test command has been invoked.");
        }

        //private void RegisterTouchManipulateMode() {
        //    Unregister();

        //    var touchDrag = (Parent as MainWindow).OTouchDrag
        //        .Repeat()
        //        .Subscribe(e => Console.WriteLine(e.GetTouchPoint(Parent).TouchDevice.Id));
        //}

        private void SetBarys() {
            // Init all barys
            Model.Complex.SetBary();
            Model.CreateMap();
        }

        //private void RegisterManipulateMode() {
        //    Unregister();

        //    var leftMove = (Parent as MainWindow).OMouseDrag
        //        .Subscribe(Function);

        //    void Function(MouseEventArgs x) {
        //        var position = x.GetPosition(Parent).ToSKPoint();
        //        var result = _Find_Target(Page.Root, position);

        //        if (result != null) {
        //            // TODO
        //            var target = Model.FindRegionById(result.ModelHash);

        //            if (target != null) {
        //                var lambdas = np.atleast_2d(Model.Complex.GetLambdas(target, np.array(position.X, position.Y)));

        //                var targetValue = Model.Map.MapTo(lambdas);

        //                Console.WriteLine(targetValue);

        //                //_seq.Enqueue(lambdas.astype(np.float32).GetData<float>()[0]);

        //                //var widget = Page.Root.FindByName<DataMonitorWidget>("Debug");
        //                //var state = new DataMonitorState {
        //                //    Bound = new SKRect(0, 0, 200, 40),
        //                //    Seqs = _seq.ToArray()
        //                //};

        //                //Engine.SetState(widget, state);
        //                //Console.WriteLine(lambdas);
        //            }

        //        }
        //    }

        private void CreateInterior() {
            Model.CreateRegions();

            var data = Model.GetSimplexCollectionData();

            Simplices = data;
        }

        private void CreateExterior() {
            var data = Model.GetVoronoiCollectionData();

            Voronois = data;
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
            var value = ((Point)pt).ToSKPoint().ToNDarray();
            var uid = Model.Add(value);

            Nodes.Add(new Node { Location = (Point)pt, Uid = uid });
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
            var idx = Nodes.ToList().FindIndex(e => e.Uid == uid);
            Nodes.RemoveAt(idx);
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
            var pt = (Point)arg;
            //var result = Model.Map.MapTo(pt.ToNDarray());
        }
    }
}
