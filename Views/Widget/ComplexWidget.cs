using Prism.Commands;
using Prism.Mvvm;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Views {
    public enum OperationMode {
        Default = 0,
        Add,
        Edit,
        Panning,
        Trace
    }

    public class SKMatrixConverter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var text = string.Join(",", ((SKMatrix)value).Values);
            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }

    public class ViewPort : BindableBase {
        private WriteableBitmap _bitmap;
        private SKSurface _surface;
        private SKImageInfo _info;
        private double _width;
        private double _height;

        private SKRect _bound;

        // Viewport to World
        private SKMatrix _translate = SKMatrix.Identity;

        public SKMatrix Transform {
            get { return _translate; }
            set { SetProperty(ref _translate, value); }
        }


        private SKPoint _wOffset;
        public SKPoint WOffset {
            get => _wOffset;
            private set { SetProperty(ref _wOffset, value); }
        }


        public ViewPort(float width, float height) {
            _bound = new SKRect() {
                Size = new SKSize(width, height),
            };

            WOffset = new SKPoint() {
                X = 0,
                Y = 0
            };

            // 
            // _wCenter = _translate.MapPoint(v);
        }

        public void SetTranslate(float x, float y) {
            _translate = SKMatrix.CreateTranslation(x, y);
        }


        public void Translate(float x, float y) {
            //_vCenter.X += x;
            //_vCenter.Y += y;
            var newX = _wOffset.X + x;
            var newY = _wOffset.Y + y;

            WOffset = new SKPoint(newX, newY);

            Transform = SKMatrix.CreateTranslation(_wOffset.X, _wOffset.Y);
        }

        public SKMatrix GetTranslate() {
            return _translate;
        }

        // V(world) = Inv(T) * V(viewport) 
        public SKPoint ViewportToWorld(SKPoint vPt) {
            return Transform.Invert().MapPoint(vPt);
        }

        public Point ViewportToWorld(Point vPt) {
            return ViewportToWorld(vPt.ToSKPoint()).ToPoint();
        }

        // V(viewport) = T * V(world)
        public SKPoint WorldToViewport(SKPoint wPt) {
            return Transform.MapPoint(wPt);
        }

        public Point WorldToViewport(Point wPt) {
            return WorldToViewport(wPt.ToSKPoint()).ToPoint();
        }

        public void CreateContext(int width, int height) {
            if (height > 0 && width > 0) {
                _bitmap = new WriteableBitmap(
                    width,
                    height,
                    96,
                    96,
                    PixelFormats.Pbgra32,
                    BitmapPalettes.Halftone256Transparent);
                _info = new SKImageInfo {
                    Width = (int)_bitmap.Width,
                    Height = (int)_bitmap.Height,
                    ColorType = SKColorType.Bgra8888,
                    AlphaType = SKAlphaType.Premul,
                };

                _surface?.Dispose();

                _surface = SKSurface.Create(_info, _bitmap.BackBuffer, _bitmap.BackBufferStride);
            }
            else {
                _bitmap = null;
                _surface?.Dispose();
            }
        }

        public void Clear() {
            _bitmap.Lock();

            _surface.Canvas.Clear();

            _bitmap.Unlock();
        }

        public SortedDictionary<int, List<Action<SKCanvas>>> RenderQueue = new SortedDictionary<int, List<Action<SKCanvas>>>();

        //public void Register(Action<SKCanvas> onDraw, int zIdx) {
        //    var hasVal = RenderQueue.ContainsKey(zIdx);

        //    if (hasVal) {
        //        RenderQueue[zIdx].Add(onDraw);
        //    }
        //    else {
        //        RenderQueue[zIdx] = new List<Action<SKCanvas>>();
        //        RenderQueue[zIdx].Add(onDraw);
        //    }
        //}

        //public 

        public void Render(IEnumerable<SKFrameworkElement> objects) {
            if (_bitmap is null) return;

            _bitmap.Lock();

            _surface.Canvas.Clear();

            foreach (var item in objects) {
                item.Draw(_surface.Canvas);
            }

            //foreach(var value in RenderQueue.Values) {
            //    value.ForEach(e => e.Invoke(_surface.Canvas));
            //}
            //onDraw(_surface.Canvas);

            _bitmap.AddDirtyRect(new Int32Rect {
                X = 0,
                Y = 0,
                Width = (int)_bitmap.Width,
                Height = (int)_bitmap.Height
            });

            _bitmap.Unlock();
        }

        public void Render(Action<SKCanvas> onDraw) {
            if (_bitmap is null) return;

            _bitmap.Lock();

            onDraw(_surface.Canvas);

            _bitmap.AddDirtyRect(new Int32Rect {
                X = 0,
                Y = 0,
                Width = (int)_bitmap.Width,
                Height = (int)_bitmap.Height
            });

            _bitmap.Unlock();
        }

        public WriteableBitmap GetContext() => _bitmap;
    }

    public class ComplexWidget : Canvas {
        public ViewPort ViewPort { get; set; }

        internal static readonly DependencyPropertyKey IsSetPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsSet",
            typeof(bool),
            typeof(ComplexWidget),
            new FrameworkPropertyMetadata());

        public bool IsSet => (bool)GetValue(IsSetPropertyKey.DependencyProperty);


        public OperationMode Mode {
            get { return (OperationMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Mode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(OperationMode), typeof(ComplexWidget),
                new PropertyMetadata(OperationMode.Default, OnModeChanged));

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue) { }
        }

        protected override void OnMouseEnter(MouseEventArgs e) {
            //var result = Focus();

            ////Console.WriteLine(result);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            Focus();
            base.OnMouseLeftButtonDown(e);
        }



        public Point TracePoint {
            get { return (Point)GetValue(TracePointProperty); }
            set { SetValue(TracePointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TracePoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TracePointProperty =
            DependencyProperty.Register("TracePoint", typeof(Point), typeof(ComplexWidget), new PropertyMetadata(new Point()));


        public FrameworkElement InspectedWidget {
            get { return (FrameworkElement)GetValue(InspectedWidgetProperty); }
            set { SetValue(InspectedWidgetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InspectedObject.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InspectedWidgetProperty =
            DependencyProperty.Register("InspectedWidget", typeof(FrameworkElement), typeof(ComplexWidget),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));


        public NodeWidget SelectedNode {
            get { return (NodeWidget)GetValue(SelectedNodeProperty); }
            set { SetValue(SelectedNodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedNode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedNodeProperty =
            DependencyProperty.Register("SelectedNode", typeof(NodeWidget), typeof(ComplexWidget),
                new PropertyMetadata(null));



        public ControlUiState UiState {
            get { return (ControlUiState)GetValue(UiStateProperty); }
            set { SetValue(UiStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UiState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UiStateProperty =
            DependencyProperty.Register("UiState", typeof(ControlUiState), typeof(ComplexWidget), new FrameworkPropertyMetadata(null, OnUiStatePropertyChanged));

        private static void OnUiStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            var complex = (ComplexWidget)d;
            var oldValue = (ControlUiState)args.OldValue;
            var newValue = (ControlUiState)args.NewValue;

            void OnPropertyChanged(object s, PropertyChangedEventArgs args1) {
                //var complex = (ComplexWidget)s;
                if (args1.PropertyName == nameof(ControlUiState.Nodes)) {
                    var oldNodeStates = complex.Children.OfType<NodeWidget>()
                        .Select(e => e.DataContext)
                        .Cast<NodeState>()
                        .ToArray();
                    var newNodeStates = complex.UiState.Nodes;
                    var remove = oldNodeStates.Except(newNodeStates);
                    var add = newNodeStates.Except(oldNodeStates);

                    //var diff = oldNodeStates.Except(newNodeStates);
                    foreach (var nodeState in remove) {
                        var nodeWidget = complex.Children
                            .OfType<NodeWidget>()
                            .Where(e => e.DataContext == nodeState).FirstOrDefault();

                        complex.Children.Remove(nodeWidget);
                    }


                    foreach (var nodeState in add) {
                        var nodeWidget = new NodeWidget {
                            DataContext = nodeState,
                            Focusable = true,
                        };

                        SetLeft(nodeWidget, 0);
                        SetTop(nodeWidget, 0);
                        SetZIndex(nodeWidget, 5);

                        complex.Children.Add(nodeWidget);

                        BindingOperations.SetBinding(
                            nodeWidget,
                            NodeWidget.LocationProperty,
                            new Binding {
                                Path = new PropertyPath("Value")
                            });
                        BindingOperations.SetBinding(
                            nodeWidget,
                            WidthProperty,
                            new Binding {
                                ElementName = "complex",
                                Path = new PropertyPath("ActualWidth")
                            });
                        BindingOperations.SetBinding(
                            nodeWidget,
                            HeightProperty,
                            new Binding {
                                ElementName = "complex",
                                Path = new PropertyPath("ActualHeight")
                            });
                        BindingOperations.SetBinding(
                            nodeWidget,
                            NodeWidget.IsSetProperty,
                            new Binding {
                                Path = new PropertyPath("IsSet")
                            });
                    }
                }
                else if (args1.PropertyName == nameof(ControlUiState.Regions)) {
                    var oldSimplexStates = complex.Children.OfType<SimplexWidget>()
                        .Select(e => e.DataContext)
                        .Cast<SimplexState>()
                        .ToArray();
                    var newSimplexStates = complex.UiState.Regions
                        .OfType<SimplexState>()
                        .ToArray();

                    complex.OnSimplexCollectionChanged(oldSimplexStates, newSimplexStates);

                    var oldVoronoiStates = complex.Children.OfType<VoronoiWidget>()
                        .Select(e => e.DataContext)
                        .Cast<VoronoiState>()
                        .ToArray();
                    var newVoronoiStates = complex.UiState.Regions
                        .OfType<VoronoiState>()
                        .ToArray();

                    complex.OnVoronoiCollectionChanged(oldVoronoiStates, newVoronoiStates);
                }
            }

            if (newValue != null) {
                newValue.PropertyChanged += OnPropertyChanged;
                OnPropertyChanged(null, new PropertyChangedEventArgs(nameof(ControlUiState.Nodes)));
                OnPropertyChanged(null, new PropertyChangedEventArgs(nameof(ControlUiState.Regions)));
            }
            if (oldValue != null)
                oldValue.PropertyChanged -= OnPropertyChanged;

            complex.InvalidateVisual();
        }

        //public IEnumerable NodeSource {
        //    get { return (IEnumerable)GetValue(NodeSourceProperty); }
        //    set { SetValue(NodeSourceProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for NodeSource.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty NodeSourceProperty =
        //    DependencyProperty.Register("NodeSource", typeof(IEnumerable), typeof(ComplexWidget),
        //        new FrameworkPropertyMetadata(OnCollectionPropertyChanged));

        //private static void OnCollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        //    if (d is ComplexWidget cw) {
        //        if (e.Property == NodeSourceProperty) {
        //            if (e.OldValue is INotifyCollectionChanged oldCollection) {
        //                oldCollection.CollectionChanged -= cw.OnNodeCollectionChanged;
        //            }

        //            if (e.NewValue is INotifyCollectionChanged newCollection) {
        //                newCollection.CollectionChanged += cw.OnNodeCollectionChanged;
        //            }
        //        }
        //        else if (e.Property == SimplexSourceProperty) {
        //            if (e.OldValue is INotifyCollectionChanged oldCollection) {
        //                oldCollection.CollectionChanged -= cw.OnSimplexCollectionChanged;
        //            }

        //            if (e.NewValue is INotifyCollectionChanged newCollection) {
        //                newCollection.CollectionChanged += cw.OnSimplexCollectionChanged;
        //            }
        //        }
        //        else if (e.Property == VoronoiSourceProperty) {
        //            if (e.OldValue is INotifyCollectionChanged oldCollection) {
        //                oldCollection.CollectionChanged -= cw.OnVoronoiCollectionChanged;
        //            }

        //            if (e.NewValue is INotifyCollectionChanged newCollection) {
        //                newCollection.CollectionChanged += cw.OnVoronoiCollectionChanged;
        //            }
        //        }
        //    }
        //}

        private void OnNodeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add) {
                foreach(NodeState item in e.NewItems) {
                    var widget = new NodeWidget {
                        DataContext = item,
                        Focusable = true,
                        //Width = ActualWidth,
                        Height = ActualHeight
                    };

                    SetLeft(widget, 0);
                    SetTop(widget, 0);
                    SetZIndex(widget, 5);

                    //widget.Style = ItemStyle;

                    Children.Add(widget);

                    BindingOperations.SetBinding(
                        widget,
                        NodeWidget.LocationProperty,
                        new Binding {
                            Path = new PropertyPath("Value")
                        });
                    BindingOperations.SetBinding(
                        widget,
                        WidthProperty,
                        new Binding {
                            ElementName = "complex",
                            Path = new PropertyPath("ActualWidth")
                        });
                    BindingOperations.SetBinding(
                        widget,
                        HeightProperty,
                        new Binding {
                            ElementName = "complex",
                            Path = new PropertyPath("ActualHeight")
                        });
                    BindingOperations.SetBinding(
                        widget,
                        NodeWidget.IsSetProperty,
                        new Binding {
                            Path = new PropertyPath("IsSet")
                        });
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove) {
                foreach(NodeState item in e.OldItems) {
                    var widget = Children
                        .OfType<NodeWidget>()
                        .Where(e1 => e1.DataContext == item).First();

                    Children.Remove(widget);
                }
            }
        }



        //public IEnumerable SimplexSource {
        //    get { return (IEnumerable)GetValue(SimplexSourceProperty); }
        //    set { SetValue(SimplexSourceProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for SimplexSource.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty SimplexSourceProperty =
        //    DependencyProperty.Register("SimplexSource", typeof(IEnumerable), typeof(ComplexWidget), new PropertyMetadata(null, OnCollectionPropertyChanged));

        public void OnSimplexCollectionChanged(SimplexState[] oldStates, SimplexState[] newStates) {
            var remove = oldStates.Except(newStates);
            var add = newStates.Except(oldStates);

            foreach (SimplexState item in remove) {
                var widget = Children
                    .OfType<SimplexWidget>()
                    .Where(e1 => e1.DataContext == item).FirstOrDefault();

                Children.Remove(widget);
            }

            foreach (SimplexState item in add) {
                var widget = new SimplexWidget {
                    DataContext = item,
                    Focusable = true
                };

                SetLeft(widget, 0);
                SetTop(widget, 0);
                SetZIndex(widget, 2);

                Children.Add(widget);

                BindingOperations.SetBinding(
                    widget,
                    WidthProperty,
                    new Binding {
                        ElementName = "complex",
                        Path = new PropertyPath("ActualWidth")
                    });
                BindingOperations.SetBinding(
                    widget,
                    HeightProperty,
                    new Binding {
                        ElementName = "complex",
                        Path = new PropertyPath("ActualHeight")
                    });
                BindingOperations.SetBinding(
                    widget,
                    SimplexWidget.PointsProperty,
                    new Binding {
                        Path = new PropertyPath("Points")
                    });
            }
        }

        public void OnVoronoiCollectionChanged(VoronoiState[] oldStates, VoronoiState[] newStates) {
            var remove = oldStates.Except(newStates);
            var add = newStates.Except(oldStates);

            foreach (VoronoiState item in remove) {
                var widget = Children
                    .OfType<VoronoiWidget>()
                    .Where(e1 => e1.DataContext == item).First();

                Children.Remove(widget);
            }

            foreach (VoronoiState item in add) {
                var widget = new VoronoiWidget {
                    DataContext = item,
                    Focusable = true
                };

                SetLeft(widget, 0);
                SetTop(widget, 0);
                SetZIndex(widget, 2);

                Children.Add(widget);

                BindingOperations.SetBinding(
                    widget,
                    WidthProperty,
                    new Binding {
                        ElementName = "complex",
                        Path = new PropertyPath("ActualWidth")
                    });
                BindingOperations.SetBinding(
                    widget,
                    HeightProperty,
                    new Binding {
                        ElementName = "complex",
                        Path = new PropertyPath("ActualHeight")
                    });
                BindingOperations.SetBinding(
                    widget,
                    VoronoiWidget.PointsProperty,
                    new Binding {
                        Path = new PropertyPath("Points")
                    });
            }
        }



        public FrameworkElement SelectedNodeWidget {
            get { return (FrameworkElement)GetValue(SelectedNodeWidgetProperty); }
            set { SetValue(SelectedNodeWidgetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedNodeWidget.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedNodeWidgetProperty =
            DependencyProperty.Register("SelectedNodeWidget", typeof(FrameworkElement), typeof(ComplexWidget), new PropertyMetadata(null));



        public FrameworkElement HitElement {
            get { return (FrameworkElement)GetValue(HitElementProperty); }
            set { SetValue(HitElementProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HitElement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HitElementProperty =
            DependencyProperty.Register("HitElement", typeof(FrameworkElement), typeof(ComplexWidget), new PropertyMetadata(null));



        public void Select(NodeWidget el) {
            foreach(var widget in Children.OfType<NodeWidget>()) {
                widget.IsSelected = false;
            }

            if (el != null) {
                el.IsSelected = true;
                SelectedNodeWidget = el;
            }
        }

        public ICommand SetValueCommand {
            get { return (ICommand)GetValue(SetValueCommandProperty); }
            set { SetValue(SetValueCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SetValueCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SetValueCommandProperty =
            DependencyProperty.Register("SetValueCommand", typeof(ICommand), typeof(ComplexWidget),
                new PropertyMetadata(null));



        public ICommand BuildCommand {
            get { return (ICommand)GetValue(BuildCommandProperty); }
            set { SetValue(BuildCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CreateCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BuildCommandProperty =
            DependencyProperty.Register("BuildCommand", typeof(ICommand), typeof(ComplexWidget), new PropertyMetadata(null));


        public ICommand RemoveItemCommand {
            get { return (ICommand)GetValue(RemoveItemCommandProperty); }
            set { SetValue(RemoveItemCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RemoveItemCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RemoveItemCommandProperty =
            DependencyProperty.Register("RemoveItemCommand", typeof(ICommand), typeof(ComplexWidget),
                new PropertyMetadata(null));


        public ICommand AddNodeCommand {
            get { return (ICommand)GetValue(AddNodeCommandProperty); }
            set { SetValue(AddNodeCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddItemCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddNodeCommandProperty =
            DependencyProperty.Register("AddNodeCommand", typeof(ICommand), typeof(ComplexWidget),
                new UIPropertyMetadata(default(DelegateCommand<Point>)));


        public ICommand InterpolateCommand {
            get { return (ICommand)GetValue(InterpolateCommandProperty); }
            set { SetValue(InterpolateCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InterpolateCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InterpolateCommandProperty =
            DependencyProperty.Register("InterpolateCommand", typeof(ICommand), typeof(ComplexWidget),
                new UIPropertyMetadata(null));


        public ICommand SetInspectedObjectCommand {
            get { return (ICommand)GetValue(SetInspectedObjectCommandProperty); }
            set { SetValue(SetInspectedObjectCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SetInspectedObjectCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SetInspectedObjectCommandProperty =
            DependencyProperty.Register("SetInspectedObjectCommand", typeof(ICommand), typeof(ComplexWidget),
                new PropertyMetadata(null));

        public IObservable<EventPattern<MouseButtonEventArgs>> MouseDownObs { get; set; }
        public IObservable<EventPattern<MouseButtonEventArgs>> MouseUpObs { get; set; }
        public IObservable<EventPattern<MouseEventArgs>> MouseMoveObs { get; set; }

        public IObservable<EventPattern<KeyEventArgs>> KeyDownObs { get; set; }
        public IObservable<EventPattern<KeyEventArgs>> KeyUpObs { get; set; }

        public Subject<OperationMode> ModeObs { get; set; }

        private DateTime _timestamp;
        private int _capturedTouchDevice = -1;
        private Queue<object> _tapArgs;

        private Key? _capturedKey;

        public ComplexWidget() {
            PrepareObservable();
            //AddNode();

            ModeObs = new Subject<OperationMode>();

            KeyDown += (s, e) => {
                _capturedKey = e.Key;
            };
            KeyUp += (s, e) => {
                if (e.Key == _capturedKey) {
                    OnKeyPressed(e);
                }

                _capturedKey = null;
            };


            //var keyPressed = KeyDownObs
            //                 .Take(1)
            //                 .Concat(KeyUpObs.Take(1))
            //                 .TakeLast(1)
            //                 .Repeat()
            //                 .Subscribe(OnKeyPressed);

            var add = MouseDownObs.Take(1)
                                  .Concat(MouseUpObs.Take(1))
                                  .TakeLast(1)
                                  .Repeat()
                                  .Subscribe(OnAddNode);

            var click = MouseDownObs
                        .Take(1)
                        .Concat(MouseUpObs.Take(1))
                        .TakeLast(1)
                        .Repeat()
                        .Subscribe(OnClicked);


            // touch manipulation
            TouchDown += (sender, e) => {
                if (_capturedTouchDevice == -1) {

                    if (e.TouchDevice.Capture(this)) {
                        _timestamp = DateTime.Now;
                        _capturedTouchDevice = e.TouchDevice.Id;
                    }
                }
            };

            TouchUp += (sender, e) => {
                if (e.TouchDevice.Id == _capturedTouchDevice) {
                    var timeout = _timestamp - DateTime.Now > TimeSpan.FromMilliseconds(1000);

                    if (!timeout) {
                        Console.WriteLine("Tap! {0}", _capturedTouchDevice);

                        _capturedTouchDevice = -1;
                    }
                }
            };
            //var touchDown = Observable
            //    .FromEventPattern<TouchEventArgs>(this, nameof(TouchDown));
            //var touchUp = Observable
            //    .FromEventPattern<TouchEventArgs>(this, nameof(TouchUp));
            //var touchMove = Observable
            //    .FromEventPattern<TouchEventArgs>(this, nameof(TouchMove));


            //touchDown
            //    .Subscribe((e) => {
            //        var args = e.EventArgs;

            //        Console.WriteLine("Touchdown. {0}", args.GetIntermediateTouchPoints(this));
            //    });

            //touchUp
            //    .Subscribe((e) => {
            //        var args = e.EventArgs;

            //        Console.WriteLine("Touchup. {0}", args.GetIntermediateTouchPoints(this));
            //    });

            //var tapGesture = touchDown
            //    //.Take(1)
            //    .Merge(touchUp)
            //    .Timeout(TimeSpan.FromMilliseconds(1000), Observable.Empty<EventPattern<TouchEventArgs>>())
            //    .ToArray()
            //    //.Repeat()
            //    .Subscribe(
            //        array => {
            //            Console.WriteLine(array.Length);
            //        });


            //var pan = MouseDownObs
            //    .Where(e => Mode == OperationMode.Panning)
            //    .Take(1)
            //    .Do((e) => {
            //        CaptureMouse();
            //        _last = e.EventArgs.GetPosition(this);
            //    })
            //    .SelectMany(MouseMoveObs.Throttle(TimeSpan.FromMilliseconds(50)))
            //    .TakeUntil(MouseUpObs)
            //    .Finally(() => { ReleaseMouseCapture(); })
            //    //.Do((e) => 
            //    //    ReleaseMouseCapture())
            //    .Repeat()
            //    .ObserveOn(Dispatcher)
            //    .Subscribe(OnPanning);

            var pan = MouseMoveObs
                      .SkipUntil(MouseDownObs.Do(e => {
                          CaptureMouse();
                          _last = e.EventArgs.GetPosition(this);
                      }))
                      .TakeUntil(MouseUpObs.Do(e => {
                          ReleaseMouseCapture();
                          OnPanned();
                      }))
                      .Repeat()
                      .Subscribe(OnPanning);

            //var traceM = MouseMoveObs
            //             .SkipUntil(MouseDownObs)
            //             .TakeUntil(MouseUpObs);
            ////.Repeat();

            //var trace_1 = ModeObs
            //              .StartWith(Mode)
            //              .SelectMany(traceM)
            //              //.Throttle(TimeSpan.FromMilliseconds(100))
            //              //.ObserveOnDispatcher()
            //              .Repeat()
            //              .Where(e => Mode == OperationMode.Trace)
            //              .Subscribe(OnTracing);



            //void OnTracing(EventPattern<MouseEventArgs> e) {
            //    var pt = e.EventArgs.GetPosition(this);
            //    var result = VisualTreeHelper.HitTest(this, pt);

            //    (e.Sender as ComplexWidget).TracePoint = ViewPort.ViewportToWorld(pt);

            //    if (result.VisualHit is SimplexWidget s) {
            //        HitElement = s;
            //    }
            //    else if (result.VisualHit is VoronoiWidget v) {
            //        HitElement = v;
            //    }
            //    else {
            //        HitElement = null;
            //    }
            //    //if (result.VisualHit is VoronoiWidget v) {
            //    //    var args = new object[] {
            //    //        ViewPort.ViewportToWorld(pt),
            //    //        v.Id
            //    //    };

            //    //    InterpolateCommand.Execute(args);
            //    //}
            //    //else if (result.VisualHit is SimplexWidget s) {
            //    //    var args = new object[] {
            //    //        ViewPort.ViewportToWorld(pt),
            //    //        s.Id
            //    //    };

            //    //    InterpolateCommand.Execute(args);
            //    //}


            //    //var i = Children.OfType<IndicatorWidget>().First();

            //    //i.Location = e.EventArgs.GetPosition(this);

            //    InvalidateSKContext();
            //}

            //_topics.Add(add);
            //_topics.Add(pan);

            ViewPort = new ViewPort((float)ActualWidth, (float)ActualHeight);

            var indicator = new IndicatorWidget();

            indicator.Visibility = Visibility.Hidden;
            indicator.IsHitTestVisible = false;
            indicator.Width = 20;
            indicator.Height = 20;
            //SetTop(indicator, ActualHeight / 2);
            //SetLeft(indicator, ActualWidth / 2);
            SetZIndex(indicator, 10);

            Children.Add(indicator);

            //var cursor = new CursorWidget();

            //cursor.Width = 10;
            //cursor.Height = 10;

            //cursor.IsHitTestVisible = false;
            //SetZIndex(cursor, 10);

            //Children.Add(cursor);

            //var test = MouseMoveObs
            //    .Subscribe(e => {
            //        cursor.Location = e.EventArgs.GetPosition(this);

            //        InvalidateSKContext();
            //    });

            SizeChanged += (s, e) => {
                var i = Children.OfType<IndicatorWidget>().First();

                ViewPort.CreateContext((int)ActualWidth, (int)ActualHeight);
                //ViewPort.Clear();

                //InvalidateSKContext();
                //foreach (var item in Children.OfType<SKFrameworkElement>()) {
                //    item.InvalidateVisual();
                //}
            };

            //Cursor = Cursors.Pen;
        }

        private bool _isTracing = false;

        private void OnModeChanged() {
            switch (Mode) {
                case OperationMode.Trace:
                    MouseDown += TraceModeOnMouseDown;
                    MouseUp += TraceModeOnMouseUp;
                    MouseMove += TraceModeOnMouseMove;
                    break;
            }
        }

        private void OnBeforeModeChanged() {
            switch(Mode) {
                case OperationMode.Trace:
                    MouseDown -= TraceModeOnMouseDown;
                    MouseUp -= TraceModeOnMouseUp;
                    MouseMove -= TraceModeOnMouseMove;
                    break;
            }
        }

        private void TraceModeOnMouseDown(object sender, MouseButtonEventArgs e) {
            _isTracing = true;
        }

        private void TraceModeOnMouseUp(object sender, MouseButtonEventArgs e) {
            _isTracing = false;
            HitElement = null;
        }

        private void TraceModeOnMouseMove(object sender, MouseEventArgs e) {
            if (_isTracing) {
                var pt = e.GetPosition(this);
                var result = VisualTreeHelper.HitTest(this, pt);

                (sender as ComplexWidget).TracePoint = ViewPort.ViewportToWorld(pt);

                if (result.VisualHit is SimplexWidget s) {
                    HitElement = s;
                }
                else if (result.VisualHit is VoronoiWidget v) {
                    HitElement = v;
                }
                else {
                    HitElement = null;
                }
            }
        }

        public void ResetSelection(ISelectableWidget exception = null) {
            foreach (var widget in Children.OfType<ISelectableWidget>().Except(new[] { exception })) {
                widget.IsSelected = false;
            }
        }

        private void OnClicked(EventPattern<MouseButtonEventArgs> args) {
            if (Mode == OperationMode.Default) {
                InspectedWidget = null;

                ResetSelection();

                args.EventArgs.Handled = true;
            }
        }

        public void InvalidateSKContext() {
            ViewPort.Render(OrderByZIndex());
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            dc.DrawImage(ViewPort.GetContext(), new Rect(0, 0, ActualWidth, ActualHeight));
        }

        private void OnPanned() {
            ViewPort.Clear();
            //InvalidateVisual();
            InvalidateSKContext();
            //Children.OfType<FrameworkElement>().ToList().ForEach(e => e.InvalidateVisual());
        }

        private Point _last;

        private void OnPanning(EventPattern<MouseEventArgs> obj) {
            if (Mode != OperationMode.Panning)
                return;

            if (!IsMouseCaptured) return;

            var curr = obj.EventArgs.GetPosition(this);
            //Console.WriteLine($"Curr: {curr}");
            //Console.WriteLine($"Last: {_last}");
            var v = curr - _last;
            //Console.WriteLine($"V: {v}");
            //var t = Transform;

            ViewPort.Translate((float)v.X, (float)v.Y);
            //Transform = SKMatrix.CreateTranslation((float)-v.X, (float)-v.Y);
            //RenderTransform = new TranslateTransform(-v.X, -v.Y);
            //Children.OfType<FrameworkElement>().ToList().ForEach(e => e.InvalidateVisual());
            _last = curr;
            InvalidateSKContext();
        }

        private void OnKeyPressed(KeyEventArgs obj) {
            OnBeforeModeChanged();

            switch (obj.Key) {
                case Key.A:
                    Mode = OperationMode.Add;
                    break;
                case Key.B:
                    BuildCommand.Execute(null);
                    break;
                case Key.D1:
                    Mode = OperationMode.Add;

                    break;
                case Key.D2:
                    Mode = OperationMode.Panning;
                    break;
                case Key.D5:
                    Mode = OperationMode.Trace;
                    break;
                case Key.Escape:
                    Mode = OperationMode.Default;

                    break;
                case Key.Delete:
                    //OnRemoveNode(obj);
                    break;
                case Key.T:
                    break;
                case Key.S:
                    //OnSetValue();
                    OnSetValueNew();
                    break;
                default:
                    break;
            }

            OnModeChanged();
        }

        private void OnSetValueNew() {
            if (SelectedNodeWidget is null) return;

            SetValueCommand.Execute(null);
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

        internal IEnumerable<SKFrameworkElement> OrderByZIndex() {
            var widgets = Children.OfType<SKFrameworkElement>().ToList();

            return widgets.OrderBy(e => GetZIndex(e));
        }

        private void OnAddNode(EventPattern<MouseButtonEventArgs> e) {
            if (Mode != OperationMode.Add)
                return;

            //Console.WriteLine("Add");
            AddNodeCommand.Execute(e.EventArgs.GetPosition((IInputElement)e.Sender));

            // Exit to default
            Mode = OperationMode.Default;

            InvalidateSKContext();
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
