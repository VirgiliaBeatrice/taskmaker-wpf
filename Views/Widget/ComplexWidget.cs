using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Views {
    public enum OperationMode {
        Default = 0,
        Add,
        Edit,
        Panning
    }

    public class ViewPort {
        private double _width;
        private double _height;

        private Rect _bound;

        public ViewPort() {
            _bound = new Rect();
        }

        public Point WorldToScreen(Point wPt) {
            var screenCenterX = _width / 2.0;
            var screenCenterY = _height / 2.0;
            var screenX = (wPt.X - center)
        }

    public class ComplexWidget : Canvas {

        public OperationMode Mode {
            get { return (OperationMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Mode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(OperationMode), typeof(ComplexWidget), new PropertyMetadata(OperationMode.Default, OnModeChanged));

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue) { }
        }

        private List<IDisposable> _topics = new List<IDisposable>();

        private void Unsubscribe() {
            _topics.ForEach(e => e.Dispose());
            _topics.Clear();
        }

        protected override void OnMouseEnter(MouseEventArgs e) {
            var result = Focus();

            Console.WriteLine(result);
            base.OnMouseEnter(e);
        }

        public NodeWidget SelectedNode {
            get { return (NodeWidget)GetValue(SelectedNodeProperty); }
            set { SetValue(SelectedNodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedNode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedNodeProperty =
            DependencyProperty.Register("SelectedNode", typeof(NodeWidget), typeof(ComplexWidget), new PropertyMetadata(null));




        public IEnumerable VoronoiSource {
            get { return (IEnumerable)GetValue(VoronoiSourceProperty); }
            set { SetValue(VoronoiSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VoronoiSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VoronoiSourceProperty =
            DependencyProperty.Register("VoronoiSource", typeof(IEnumerable), typeof(ComplexWidget), new PropertyMetadata(null, OnPropertyChanged_Voronoi));

        private static void OnPropertyChanged_Voronoi(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var complex = d as ComplexWidget;

            // Clear
            complex.Children.OfType<VoronoiWidget>()
                .ToList()
                .ForEach(x => complex.Children.Remove(x));

            foreach (var item in (VoronoiData[])e.NewValue) {
                var newVoronoi = new VoronoiWidget {
                    DataContext = item,
                    Id = item.Uid,
                };

                BindingOperations.SetBinding(
                    newVoronoi,
                    VoronoiWidget.PointsProperty,
                    new Binding {
                        Source = item,
                        Path = new PropertyPath("Points")
                    });

                ComplexWidget.SetZIndex(newVoronoi, 1);

                complex.Children.Add(newVoronoi);
            }
        }

        public IEnumerable SimplexSource {
            get { return (IEnumerable)GetValue(SimplexSourceProperty); }
            set { SetValue(SimplexSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SimplexSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SimplexSourceProperty =
            DependencyProperty.Register("SimplexSource", typeof(IEnumerable), typeof(ComplexWidget), new PropertyMetadata(null, OnPropertyChanged_Simplex));

        private static void OnPropertyChanged_Simplex(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var complex = d as ComplexWidget;

            // Clear
            complex.Children.OfType<SimplexWidget>()
                .ToList()
                .ForEach(x => complex.Children.Remove(x));

            foreach (var item in (SimplexData[])e.NewValue) {
                var newSimplex = new SimplexWidget() {
                    DataContext = item,
                    Id = item.Uid,
                };

                BindingOperations.SetBinding(
                    newSimplex,
                    SimplexWidget.PointsProperty,
                    new Binding {
                        Source = item,
                        Path = new PropertyPath("Points")
                    });

                ComplexWidget.SetZIndex(newSimplex, 2);

                complex.Children.Add(newSimplex);
            }
        }

        public IEnumerable NodeSource {
            get { return (IEnumerable)GetValue(NodeSourceProperty); }
            set { SetValue(NodeSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NodeSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NodeSourceProperty =
            DependencyProperty.Register("NodeSource", typeof(IEnumerable), typeof(ComplexWidget), new FrameworkPropertyMetadata(OnCollectionPropertyChanged));

        private static void OnCollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is ComplexWidget cw) {
                if (e.OldValue is INotifyCollectionChanged oldCollection) {
                    oldCollection.CollectionChanged -= cw.CollectionChanged;
                }

                if (e.NewValue is INotifyCollectionChanged newCollection) {
                    newCollection.CollectionChanged += cw.CollectionChanged;
                }
            }
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add) {
                foreach (var item in e.NewItems) {
                    if (item is Node node) {
                        var newNode = new NodeWidget() {
                            DataContext = node,
                            Id = node.Uid,
                        };
                        newNode.Style = ItemStyle;
                        newNode.Click += OnClick;

                        ComplexWidget.SetZIndex(newNode, 5);

                        Children.Add(newNode);

                    }
                    
                    else if (item is VoronoiData voronoi) {
                        //var newVoronoi = new VoronoiWidget {
                        //    DataContext = voronoi,
                        //    Id = voronoi.Uid,
                        //};

                        //BindingOperations.SetBinding(
                        //    newVoronoi,
                        //    VoronoiWidget.PointsProperty,
                        //    new Binding {
                        //        Source = item,
                        //        Path = new PropertyPath("Points")
                        //    });

                        //Children.OfType<VoronoiWidget>()
                        //    .ToList()
                        //    .ForEach(x => Children.Remove(x));
                        //Children.Add(newVoronoi);
                    }

                }
                //foreach (var item in e.NewItems) {
                //    var idx = e.NewStartingIndex;
                //    var newNode = new NodeWidget("Node" + idx) {
                //        DataContext = DataContext
                //    };
                //    //newNode.ItemRemove += ItemRemove;

                //    AddChild(newNode);

                //    var bind = new Binding {
                //        Source = DataContext,
                //        Path = new PropertyPath($"Nodes_v1[{idx}].Location"),
                //    };

                //    BindingOperations.SetBinding(
                //        newNode,
                //        NodeWidget.LocationProperty,
                //        bind);
                //    //BindingOperations.SetBinding(
                //    //    newNode,
                //    //    NodeWidget.WillRemoveProperty,
                //    //    new Binding {
                //    //        Source = DataContext,
                //    //        Path = new PropertyPath($"Nodes_v1[{idx}].WillRemove")});
                //}
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove) {
                foreach (var item in e.OldItems.OfType<Node>()) {
                    var target = Children.Cast<NodeWidget>()
                        .Where(x => x.Id == item.Uid)
                        .First();

                    Children.Remove(target);
                }
                //foreach (var item in e.OldItems) {
                //    var idx = e.OldStartingIndex;

                //    Remove("Node" + idx);
                //}
            }
        }

        internal void OnClick(object sender, RoutedEventArgs e) {
            // TODO: Ugly
            if (sender is NodeWidget w) {
                if (SelectedNode == null) {
                    SelectedNode = w;
                }
                else if (SelectedNode != null && SelectedNode != w) {
                    SelectedNode.IsSelected = false;
                    SelectedNode = w;
                }
                else {
                    SelectedNode = SelectedNode.IsSelected ? w : null;
                }
            }
        }



        public ICommand InteriorCommand {
            get { return (ICommand)GetValue(InteriorCommandProperty); }
            set { SetValue(InteriorCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InteriorCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InteriorCommandProperty =
            DependencyProperty.Register("InteriorCommand", typeof(ICommand), typeof(ComplexWidget), new PropertyMetadata(null));



        public ICommand ExteriorCommand {
            get { return (ICommand)GetValue(ExteriorCommandProperty); }
            set { SetValue(ExteriorCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExteriorCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExteriorCommandProperty =
            DependencyProperty.Register("ExteriorCommand", typeof(ICommand), typeof(ComplexWidget), new PropertyMetadata(null));




        public ICommand RemoveItemCommand {
            get { return (ICommand)GetValue(RemoveItemCommandProperty); }
            set { SetValue(RemoveItemCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RemoveItemCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RemoveItemCommandProperty =
            DependencyProperty.Register("RemoveItemCommand", typeof(ICommand), typeof(ComplexWidget), new PropertyMetadata(null));



        public ICommand AddItemCommand {
            get { return (ICommand)GetValue(AddItemCommandProperty); }
            set { SetValue(AddItemCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddItemCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddItemCommandProperty =
            DependencyProperty.Register("AddItemCommand", typeof(ICommand), typeof(ComplexWidget), new UIPropertyMetadata(null));



        public Style ItemStyle {
            get { return (Style)GetValue(ItemStyleProperty); }
            set { SetValue(ItemStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemStyleProperty =
            DependencyProperty.Register("ItemStyle", typeof(Style), typeof(ComplexWidget), new PropertyMetadata(null));




        public IObservable<EventPattern<MouseButtonEventArgs>> MouseDownObs { get; set; }
        public IObservable<EventPattern<MouseButtonEventArgs>> MouseUpObs { get; set; }
        public IObservable<EventPattern<MouseEventArgs>> MouseMoveObs { get; set; }

        public IObservable<EventPattern<KeyEventArgs>> KeyDownObs { get; set; }
        public IObservable<EventPattern<KeyEventArgs>> KeyUpObs { get; set; }

        public ComplexWidget() {
            PrepareObservable();
            //AddNode();

            var keyPressed = KeyDownObs
                .Take(1)
                .Concat(KeyUpObs.Take(1))
                .TakeLast(1)
                .Repeat()
                .Subscribe(OnKeyPressed);

            var add = MouseDownObs.Take(1)
                .Concat(MouseUpObs.Take(1))
                .TakeLast(1)
                .Repeat()
                .Subscribe(OnAddNode);

            var pan = MouseDownObs
                .Where(e => Mode == OperationMode.Panning)
                .Take(1)
                .Do((e) => {
                    CaptureMouse();
                    _last = e.EventArgs.GetPosition(this);
                })
                .SelectMany(MouseMoveObs.Throttle(TimeSpan.FromMilliseconds(50)))
                .TakeUntil(MouseUpObs)
                .Finally(() => { ReleaseMouseCapture(); })
                //.Do((e) => 
                //    ReleaseMouseCapture())
                .Repeat()
                .ObserveOn(Dispatcher)
                .Subscribe(OnPanning);

            _topics.Add(add);
            _topics.Add(pan);
        }
        private Point _last;

        private void OnPanning(EventPattern<MouseEventArgs> obj) {
            if (Mode != OperationMode.Panning)
                return;

            if (!IsMouseCaptured) return;

            var v = _last - obj.EventArgs.GetPosition(this);
            var t = ((MatrixTransform)RenderTransform).Matrix;

            t.Translate(-v.X, -v.Y);

            RenderTransform = new MatrixTransform(t);

            _last = obj.EventArgs.GetPosition(this);
            InvalidateVisual();
        }

        private void OnKeyPressed(EventPattern<KeyEventArgs> obj) {
            if (obj.EventArgs.Key == Key.D1) {
                Mode = OperationMode.Add;

                return;
            }

            if (obj.EventArgs.Key == Key.D2) {
                Mode = OperationMode.Panning;
                return;
            }

            if (obj.EventArgs.Key == Key.Escape) {
                Mode = OperationMode.Default;

                return;
            }

            switch (obj.EventArgs.Key) {
                case Key.Delete:
                    OnRemoveNode(obj);
                    break;
                case Key.D3:
                    InteriorCommand.Execute(null);
                    break;
                case Key.D4:
                    ExteriorCommand.Execute(null);
                    break;
                case Key.T:
                    var lb = (FindName("lbTargets") as ListBox);
                    if (lb.Visibility == Visibility.Hidden)
                        lb.Visibility = Visibility.Visible;
                    else 
                        lb.Visibility = Visibility.Hidden;
                    break;
            }
        }

        protected void PrepareObservable() {
            MouseDownObs = Observable.FromEventPattern<MouseButtonEventArgs>(
                this, nameof(MouseDown));
            MouseUpObs = Observable.FromEventPattern<MouseButtonEventArgs>(
                this, nameof(MouseUp));
            MouseMoveObs = Observable.FromEventPattern<MouseEventArgs>(
                this, nameof(MouseMove));

            KeyDownObs = Observable.FromEventPattern<KeyEventArgs>(
                this, nameof(KeyDown));
            KeyUpObs = Observable.FromEventPattern<KeyEventArgs>(
                this, nameof(KeyUp));
        } 

        private void OnAddNode(EventPattern<MouseButtonEventArgs> e) {
            if (Mode != OperationMode.Add)
                return;
            
            Console.WriteLine("Add");
            AddItemCommand.Execute(e.EventArgs.GetPosition((IInputElement)e.Sender));

            // Exit to default
            Mode = OperationMode.Default;

        }

        private void OnRemoveNode(EventPattern<KeyEventArgs> obj) {
            if (SelectedNode != null) {
                Console.WriteLine($"Remove a selected node. Uid: {SelectedNode.Id}");

                RemoveItemCommand.Execute(SelectedNode.Id);
                obj.EventArgs.Handled = true;
            }
        }
    }
}
