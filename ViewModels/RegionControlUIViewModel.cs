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
using taskmaker_wpf.Views.Widgets.Container;
using taskmaker_wpf.Views.Debug;
using System.Windows;
using System.Windows.Input;
using SkiaSharp.Views.WPF;
using Prism.Mvvm;
using Prism.Commands;
using taskmaker_wpf.Services;
using System.Reactive.Subjects;
using System.Collections.ObjectModel;

namespace taskmaker_wpf.ViewModels {
    public class Node : BindableBase {
        private SKPoint _location;
        public SKPoint Location { 
            get => _location; 
            set => SetProperty(ref _location, value); 
        }

        private bool _willRemove;
        public bool WillRemove {
            get => _willRemove;
            set => SetProperty(ref _willRemove, value);
        }
    }


    public static class Helper {
        static public SKPoint ToSKPoint(this NDarray self) {
            if (self.ndim > 1)
                throw new Exception("Invalid ndarray");

            var castValues = self.astype(np.float32);
            var values = castValues.GetData<float>();

            castValues.Dispose();

            return new SKPoint(values[0], values[1]);
        }

        static public NDarray<float> ToNDarray(this SKPoint self) {
            return np.array(self.X, self.Y);
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

    public enum OperationMode {
        ModeDefault = 0,
        ModeAdd = 1,
        ModeEdit = 2,
        ModeRemove = 3,
    }

    public class RegionControlUIViewModel : BindableBase {
        public Window Parent { get; set; }
        public Model.Core.UI Model { get; set; }
        public Views.Pages.SimplexView Page { get; set; }
        public Model.Data.MotorCollection Motors { get; set; }

        private List<IDisposable> _topics = new List<IDisposable> ();
        private LimitedQueue<float> _seq = new LimitedQueue<float>(100);

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

        private SKPoint[] _nodes;

        public ObservableCollection<Node> Nodes_v1 { get; set; } = new ObservableCollection<Node>();

        public DelegateCommand TestCommand { get; set; }

        private Subject<MouseButtonEventArgs> mouseDown;
        private ICommand _mouseDownCmd;
        public ICommand MouseDownCmd => _mouseDownCmd ?? (_mouseDownCmd = new DelegateCommand<MouseButtonEventArgs>(MouseDownExecute));

        private Subject<MouseButtonEventArgs> mouseUp;

        private ICommand _mouseUpCmd;
        public ICommand MouseUpCmd => _mouseUpCmd ?? (_mouseUpCmd = new DelegateCommand<MouseButtonEventArgs>(MouseUpExecute));

        private Subject<MouseEventArgs> mouseMove;
        private ICommand _mouseMoveCmd;
        public ICommand MouseMoveCmd => _mouseMoveCmd ?? (_mouseMoveCmd = new DelegateCommand<MouseEventArgs>(MouseMoveExecute));

        private IObservable<MouseButtonEventArgs> _mouseLeftClick;

        private ICommand _changeModeCmd;
        public ICommand ChangeModeCmd => _changeModeCmd ?? (_changeModeCmd = new DelegateCommand<string>(ExecuteChangeModeCmd));

        private ICommand itemRemoveCmd;
        public ICommand ItemRemoveCmd => itemRemoveCmd ?? (itemRemoveCmd = new DelegateCommand<object>(ExecuteItemRemoveCmd));

        private void ExecuteItemRemoveCmd(object obj) {
            Nodes_v1.RemoveAt((int)obj);
        }

        private void ExecuteChangeModeCmd(string obj) {
            var castObj = Enum.Parse(typeof(OperationMode), obj);

            OperationMode = (OperationMode)castObj;

            switch(castObj) {
                case OperationMode.ModeDefault:
                    Unregister();
                    break;
                case OperationMode.ModeAdd:
                    ModeAdd();
                    break;
                case OperationMode.ModeEdit:
                    ModeEdit();
                    break;
                case OperationMode.ModeRemove:
                    ModeRemove();
                    break;
            }
        }

        private void MouseMoveExecute(MouseEventArgs obj) {
            mouseMove.OnNext(obj);
        }

        private void MouseUpExecute(MouseButtonEventArgs obj) {
            mouseUp.OnNext(obj);
        }

        private void MouseDownExecute(MouseButtonEventArgs obj) {
            mouseDown.OnNext(obj);
        }

        public RegionControlUIViewModel(
            MotorService motorService,
            Window parent) {
            Parent = parent;
            Model = new Model.Core.UI();
            Motors = new Model.Data.MotorCollection();
            Page = new Views.Pages.SimplexView(this);

            Motors.Motors.Add(new Model.Data.Motor());
            Motors.Motors.Add(new Model.Data.Motor());
            Motors.Motors.Add(new Model.Data.Motor());

            Model.BindTarget(Motors);

            // 1st build
            Engine.Build(Page.Root);

            // Subscribe observable
            //RegisterKeyPress();
            //RegisterAddAndDeleteMode();
            mouseDown = new Subject<MouseButtonEventArgs>();
            mouseMove = new Subject<MouseEventArgs>();
            mouseUp = new Subject<MouseButtonEventArgs>();
            _mouseLeftClick = new Subject<MouseButtonEventArgs>();

            //var disposable0 = mouseDown.Subscribe(
            //    (e) => { },
            //    (e) => { Console.WriteLine(e); },
            //    () => { });

            mouseDown.Take(1)
                .SelectMany(mouseUp.Take(1))
                .Repeat()
                .Subscribe((e) => { Count += 1; });

            //_mouseLeftClick = mouseDown
            //    .SelectMany(mouseUp)
            //    .Where(e => e.ChangedButton == MouseButton.Left)
            //    .Take(2)
            //    .Repeat();

            //_mouseLeftClick.Subscribe(e => { Console.WriteLine(e.GetPosition((UIElement)e.Source)); });
            SystemInfo = $"{operationMode}";
        }

        public void CommandTest() {
            Console.WriteLine("A test command has been invoked.");
        }

        public Dictionary<string, object> RegisteredSubjects = new Dictionary<string, object>();


        public void ModeAdd() {
            Unregister();

            RegisteredSubjects.Clear();

            var add = mouseDown
                .Where(e => e.ChangedButton == MouseButton.Left)
                .Take(1)
                .SelectMany(
                    mouseUp
                        .Where(e => e.ChangedButton == MouseButton.Left)
                        .Take(1))
                .Repeat()
                .Subscribe(AddNode);

            SystemInfo = $"{operationMode}";
            KeymapInfo = $"(1) Single click to add.";

           _topics.Add(add);
        }

        private IWidget _selectedWidget;

        public void ModeRemove() {
            Unregister();

            RegisteredSubjects.Clear();

            var select = mouseDown
                .Take(1)
                .SelectMany(mouseUp.Take(1))
                .LastAsync()
                .Repeat()
                .Subscribe(HitTarget);

            void HitTarget(MouseButtonEventArgs e) {
                var point = e.GetPosition((UIElement)e.Source).ToSKPoint();

                var targets = _HitTest(Page.Root, point);
                _selectedWidget = targets.Last();

                // Test
                Nodes_v1[0].WillRemove = true;
            }

            List<IWidget> _HitTest(IWidget widget, SKPoint point) {
                var targets = new List<IWidget>();
                var result = widget.HitTest(point);

                if (result) {
                    targets.Add(widget);

                    var children = widget.GetAll<IWidget>();

                    var targetCollection = children.Select(e => _HitTest(e, point)).ToList();

                    targetCollection.ForEach(e => targets.AddRange(e));
                }

                return targets;
            }

            SystemInfo = $"{operationMode}";
            KeymapInfo = $"(1) Click to remove.";
        }

        //private IWidget selectedWidget;
        public void ModeEdit() {
            //Unregister();

            //void MoveTo(MouseEventArgs args) {
            //    var hash = (selectedWidget as NodeWidget).ModelHash;
            //    var target = Model.Nodes.Find(e => e.Id == hash);

            //    target.Location = args.GetPosition((UIElement)args.Source).ToSKPoint().ToNDarray();

            //    // fetch latest info from model
            //    var nodes = FetchNodes();
            //    var widget = Page.FindByName<ComplexWidget>("Complex");
            //    var state = (ComplexWidgetState)widget.State.Clone();

            //    state.nodes = nodes;

            //    Engine.SetState(widget, state);
            //}

            //var move = mouseDown
            //    .Take(1)
            //    .Do(SelectTargetNode)
            //    .SelectMany(mouseMove)
            //    .TakeUntil(mouseUp)
            //    .Repeat()
            //    .Subscribe(MoveTo);

            ////var topic = mouseDown
            ////    .SelectMany(mouseMove)
            ////    .TakeUntil(mouseUp)
            ////    .Repeat()
            ////    .Subscribe((e) => { Console.WriteLine("aaaa"); });

            //SystemInfo = $"{operationMode}";
            //KeymapInfo = $"(1) Click&Hold to drag.";

            //_topics.Add(move);
        }

        private void SelectTargetNode(MouseButtonEventArgs args) {
            //var location = args.GetPosition((UIElement)args.Source).ToSKPoint();

            //selectedWidget = Page.FindTargetWidget(location);
        }

        //public void RegisterKeyPress() {
        //    var keyPress = (Parent as Window).OKeyPress
        //        .Repeat()
        //        .Subscribe(x => {
        //            switch (x.Key) {
        //                case Key.A:
        //                    RegisterAddAndDeleteMode();
        //                    break;
        //                case Key.S:
        //                    RegisterTriangulateMode();
        //                    break;
        //                case Key.D:
        //                    DeleteAll();
        //                    break;
        //                //case 'v':
        //                //    CreateExterior();
        //                //    break;
        //                case Key.M:
        //                    TestMotor();
        //                    break;
        //                case Key.N:
        //                    SetBarys();
        //                    break;
        //                case Key.P:
        //                    RegisterManipulateMode();
        //                    break;
        //                case Key.T:
        //                    RegisterTouchManipulateMode();
        //                    break;
        //                case Key.Escape:
        //                    Unregister();
        //                    break;
        //            }

        //            Console.WriteLine($"Mode-{x.Key}");
        //        });
        //}

        //private void RegisterTouchManipulateMode() {
        //    Unregister();

        //    var touchDrag = (Parent as MainWindow).OTouchDrag
        //        .Repeat()
        //        .Subscribe(e => Console.WriteLine(e.GetTouchPoint(Parent).TouchDevice.Id));
        //}

        private void SetBarys() {
            Unregister();

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

        //    IWidget _Find_Target(IWidget widget, SKPoint location) {
        //        var ret = widget.Contains(location);

        //        if (ret)
        //            return widget;
        //        else
        //            foreach (var item in widget.GetAllChild()) {
        //                var childRet = _Find_Target(item, location);

        //                if (childRet != null)
        //                    return childRet;
        //            }

        //        return null;
        //    }

        //    _topics.Add(leftMove);
        //}

        private void TestMotor() {
            Unregister();

            Motors.SetValue(new object[] { 0, 100, 0 });
            Model.BindData(Model.Nodes[0]);
            Motors.SetValue(new object[] { 100, 0, 0});
            Model.BindData(Model.Nodes[1]);
            Motors.SetValue(new object[] { 0, 0, 100 });
            Model.BindData(Model.Nodes[2]);
            Motors.SetValue(new object[] { 100, 100, 100 });
            Model.BindData(Model.Nodes[3]);

            //var w = new Window();
            //w.Width = 300;
            //w.Height = 600;
            //w.Content = new Motors(Motors.Motors.ToArray());
            //w.Show();
        }

        //public void RegisterAddAndDeleteMode() {
        //    Unregister();

        //    var leftClick = (Parent as MainWindow).OMouseClick
        //        //.Where(x => Mouse.LeftButton == Mouse)
        //        .Repeat()
        //        .Subscribe(e => {
        //            AddNode(e.GetPosition(Parent).ToSKPoint());
        //        });
        //    //var rightClick = (Parent as MainWindow).OMouseClick
        //    //    //.Where(x => x.Button == MouseButtons.Right)
        //    //    .Repeat()
        //    //    .Subscribe(e => {
        //    //        var target = Page.DeleteNode(e.GetPosition(Parent).ToSKPoint());

        //    //        Console.WriteLine(Page.Root.PrintAllChild());
        //    //    });

        //    _topics.Add(leftClick);
        //    //_topics.Add(rightClick);
        //}

        public void RegisterTriangulateMode() {
            Unregister();

            //CreateRegions();
        }

        //public void DeleteAll() {
        //    Unregister();

        //    Model.RemoveAll();

        //    var nodes = FetchNodes();
        //    var widget = Page.Root.FindByName("Complex") as ComplexWidget;
        //    var state = (ComplexWidgetState)widget.State.Clone();

        //    state.nodes = nodes;

        //    Engine.SetState(
        //        Page.Root.FindByName<ComplexWidget>("Complex"),
        //        state);
        //}


        public void Unregister() {
            _topics.ForEach(x => x.Dispose());
            _topics.Clear();
        }

        //public Node[] FetchNodes() {
        //    return Model.Nodes
        //        .Select(e => new Node { location = e.Location.ToSKPoint() })
        //        .ToArray();
        //}

        public SKPoint[] FetchNodes() {
            return Model.Nodes.Select(e => e.Location.ToSKPoint())
                              .ToArray();
        }

        private void AddNode(MouseButtonEventArgs args) {
            //// Add node to model
            var skLocation = args
                .GetPosition((UIElement)args.Source)
                .ToSKPoint();
            var ndLocation = skLocation
                .ToNDarray();

            Model.Add(ndLocation);
            Nodes_v1.Add(new Node { Location = skLocation });
        }

        private void RemoveNode(MouseButtonEventArgs args) {
        
        }


        //[Obsolete]
        //public void AddNode(SKPoint location) {
        //    var count = Model.Nodes.Count;
        //    var node = new Model.Data.NodeM(count + 1) {
        //        Location = location.ToNDarray()
        //    };

        //    Model.Add(node);

        //    var nodes = FetchNodes();
        //    var widget = Page.Root.FindByName<
        //        ComplexWidget>("Complex");
        //    var state = (ComplexWidgetState)widget.State.Clone();

        //    state.nodes = nodes;
        //    state.Hash = node.Id;

        //    Engine.SetState(widget, state);
        //}

        //public void CreateRegions() {
        //    Model.CreateRegions();

        //    var info = Model.GetSimplexInfos();

        //    var results = info
        //        .Select(e => (e.Item1,
        //            e.Item2
        //                .Select(e1 => e1.ToSKPoint()).ToArray()))
        //        .ToArray();

        //    var widget = Page.Root.FindByName<ComplexWidget>("Complex");
        //    var state = (ComplexWidgetState)widget.State.Clone();

        //    state.simplices = results;

        //    var regions = Model.GetVoronoiInfos();

        //    results = regions
        //        .Select(e => (e.Item1,
        //            e.Item2
        //                .Select(e1 => e1.ToSKPoint()).ToArray()))
        //        .ToArray();

        //    state.voronois = results;

        //    Engine.SetState(widget, state);
        //}


        //private void CreateInterior() {
        //    var info = Model.GetSimplexInfos();

        //    var results = info
        //        .Select(e => (e.Item1, 
        //            e.Item2
        //                .Select(e1 => e1.ToSKPoint()).ToArray()))
        //        .ToArray();

        //    var widget = Page.Root.FindByName<ComplexWidget>("Complex");
        //    var state = (ComplexWidgetState)widget.State.Clone();

        //    state.simplices = results;

        //    Engine.SetState(widget, state);
        //}

        //private void CreateExterior() {
        //    var regions = Model.GetVoronoiInfos();

        //    var results = regions
        //        .Select(e => (e.Item1,
        //            e.Item2
        //                .Select(e1 => e1.ToSKPoint()).ToArray()))
        //        .ToArray();

        //    var widget = Page.Root.FindByName<ComplexWidget>("Complex");
        //    var state = (ComplexWidgetState)widget.State.Clone();

        //    state.voronois = results;

        //    Engine.SetState(widget, state);
        //}

        private OperationMode operationMode;

        public OperationMode OperationMode { get => operationMode; set => SetProperty(ref operationMode, value); }
        public string KeymapInfo { get => _keymapInfo; set => SetProperty(ref _keymapInfo, value); }
        public string SystemInfo { get => _systemInfo; set => SetProperty(ref _systemInfo, value); }
        public string StatusMsg { get => _statusMsg; set => SetProperty(ref _statusMsg, value); }
        public SKPoint[] Nodes { get => _nodes; set => SetProperty(ref _nodes, value); }
    }

    public class LimitedQueue<T> : Queue<T> {
        public int Limit { get; set; }

        public LimitedQueue(int limit) : base(limit) {
            Limit = limit;
        }

        public new void Enqueue(T item) {
            while (Count >= Limit) {
                Dequeue();
            }
            base.Enqueue(item);
        }
    }
}
