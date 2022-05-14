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

namespace taskmaker_wpf.ViewModels {
    public struct Node {
        public SKPoint location;
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
    }

    public enum RegionControlUIMode {
        Default = 0,
        ModeAdd = 1
    }

    public class RegionControlUIViewModel : BindableBase {
        public Window Parent { get; set; }
        public Model.Core.UI Model { get; set; }
        public Views.Pages.SimplexView Page { get; set; }
        public Model.Data.MotorCollection Motors { get; set; }

        private List<IDisposable> _topics = new List<IDisposable> ();
        private LimitedQueue<float> _seq = new LimitedQueue<float>(100);

        #region Bindable Properties
        private string _motors = "motor";
        public string MotorsProperty {
            get { return _motors; }
            set { SetProperty(ref _motors, value); }
        }

        #endregion

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

        private void ExecuteChangeModeCmd(string obj) {
            var castObj = Enum.Parse(typeof(RegionControlUIMode), obj);

            Console.WriteLine(castObj);
            switch(castObj) {
                case RegionControlUIMode.Default:
                    break;
                case RegionControlUIMode.ModeAdd:
                    ModeAdd();
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
            Engine.Build(Page.Root, true);

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

            //mouseDown.Take(1)
            //    .SelectMany(mouseUp.Take(1))
            //    .Repeat()
            //    .Subscribe(e => { Console.WriteLine(e.GetPosition((UIElement)e.Source)); });

            _mouseLeftClick = mouseDown
                .SelectMany(mouseUp)
                .Where(e => e.ChangedButton == MouseButton.Left)
                .Take(2)
                .Repeat();

            _mouseLeftClick.Subscribe(e => { Console.WriteLine(e.GetPosition((UIElement)e.Source)); });
        }

        public void CommandTest() {
            Console.WriteLine("A test command has been invoked.");
        }

        public void ModeAdd() {
            //Unregister();

            var topic = _mouseLeftClick
                .Subscribe(e => {
                    AddNode(e.GetPosition((UIElement)e.Source).ToSKPoint());
                });

            _topics.Add(topic);
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

            CreateRegions();
        }

        public void DeleteAll() {
            Unregister();

            Model.RemoveAll();

            var nodes = FetchNodes();
            var widget = Page.Root.FindByName("Complex") as ComplexWidget;
            var state = (ComplexWidgetState)widget.State.Clone();

            state.nodes = nodes;

            Engine.SetState(
                Page.Root.FindByName<ComplexWidget>("Complex"),
                state);
        }


        public void Unregister() {
            _topics.ForEach(x => x.Dispose());
            _topics.Clear();
        }

        public Node[] FetchNodes() {
            return Model.Nodes
                .Select(e => new Node { location = e.Location.ToSKPoint() })
                .ToArray();
        }

        public void AddNode(SKPoint location) {
            var count = Model.Nodes.Count;
            var node = new Model.Data.NodeM(count + 1) {
                Location = location.ToNDarray()
            };
            
            Model.Add(node);

            var nodes = FetchNodes();
            var widget = Page.Root.FindByName<
                ComplexWidget>("Complex");
            var state = (ComplexWidgetState)widget.State.Clone();

            state.nodes = nodes;
            state.Hash = node.Id;

            Engine.SetState(widget, state);
        }

        public void CreateRegions() {
            Model.CreateRegions();

            var info = Model.GetSimplexInfos();

            var results = info
                .Select(e => (e.Item1,
                    e.Item2
                        .Select(e1 => e1.ToSKPoint()).ToArray()))
                .ToArray();

            var widget = Page.Root.FindByName<ComplexWidget>("Complex");
            var state = (ComplexWidgetState)widget.State.Clone();

            state.simplices = results;

            var regions = Model.GetVoronoiInfos();

            results = regions
                .Select(e => (e.Item1,
                    e.Item2
                        .Select(e1 => e1.ToSKPoint()).ToArray()))
                .ToArray();

            state.voronois = results;

            Engine.SetState(widget, state);
        }


        private void CreateInterior() {
            var info = Model.GetSimplexInfos();

            var results = info
                .Select(e => (e.Item1, 
                    e.Item2
                        .Select(e1 => e1.ToSKPoint()).ToArray()))
                .ToArray();

            var widget = Page.Root.FindByName<ComplexWidget>("Complex");
            var state = (ComplexWidgetState)widget.State.Clone();

            state.simplices = results;

            Engine.SetState(widget, state);
        }

        private void CreateExterior() {
            var regions = Model.GetVoronoiInfos();

            var results = regions
                .Select(e => (e.Item1,
                    e.Item2
                        .Select(e1 => e1.ToSKPoint()).ToArray()))
                .ToArray();

            var widget = Page.Root.FindByName<ComplexWidget>("Complex");
            var state = (ComplexWidgetState)widget.State.Clone();

            state.voronois = results;

            Engine.SetState(widget, state);
        }
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
