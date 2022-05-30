﻿using Prism.Mvvm;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
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
        Panning
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


        public SKPoint VCenter {
            get => _vCenter;
            private set { SetProperty(ref _vCenter, value); }
        }

        private SKPoint _vCenter;
        private SKPoint _wCenter;

        public ViewPort(float width, float height) {
            _bound = new SKRect() {
                Size = new SKSize(width, height),
            };

            _vCenter = new SKPoint() {
                X = _bound.MidX,
                Y = _bound.MidY
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
            var newX = _vCenter.X + x;
            var newY = _vCenter.Y + y;

            VCenter = new SKPoint(newX, newY);

            Transform = SKMatrix.CreateTranslation(_vCenter.X, _vCenter.Y);
            //_translate.PreConcat(SKMatrix.CreateTranslation(x, y));
        }

        public SKMatrix GetTranslate() {
            return _translate;
        }

        public SKPoint ViewportToWorld(SKPoint vPt) {
            return _translate.MapPoint(vPt);
        }

        public SKPoint WorldToViewport(SKPoint wPt) {
            return _translate.Invert().MapPoint(wPt);
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
                    Width = complex.ActualWidth,
                    Height = complex.ActualHeight,
                };

                //newVoronoi.UpdateLayout();

                SetTop(newVoronoi, 0);
                SetLeft(newVoronoi,0);
                SetZIndex(newVoronoi, 1);

                complex.Children.Add(newVoronoi);

                BindingOperations.SetBinding(
                    newVoronoi,
                    VoronoiWidget.PointsProperty,
                    new Binding {
                        Source = item,
                        Path = new PropertyPath("Points")
                    });


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
                    Width = complex.ActualWidth,
                    Height = complex.ActualHeight,
                };

                SetTop(complex, 0);
                SetLeft(complex, 0);
                SetZIndex(newSimplex, 2);

                complex.Children.Add(newSimplex);

                BindingOperations.SetBinding(
                    newSimplex,
                    SimplexWidget.PointsProperty,
                    new Binding {
                        Source = item,
                        Path = new PropertyPath("Points")
                    });

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
                            Width = ActualWidth,
                            Height = ActualHeight
                            //Location = node.Location.ToSKPoint()
                        };
                        newNode.Style = ItemStyle;
                        newNode.Click += OnClick;

                        SetTop(newNode, 0);
                        SetLeft(newNode, 0);
                        SetZIndex(newNode, 5);

                        Children.Add(newNode);

                        BindingOperations.SetBinding(
                            newNode,
                            NodeWidget.LocationProperty,
                            new Binding {
                                //Source = node,
                                Path = new PropertyPath("Location")});
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove) {
                foreach (var item in e.OldItems.OfType<Node>()) {
                    var target = Children.OfType<NodeWidget>()
                        .Where(x => x.Id == item.Uid)
                        .First();

                    Children.Remove(target);
                }
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



        public SKMatrix Transform {
            get { return (SKMatrix)GetValue(TransformProperty); }
            set { SetValue(TransformProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Transform.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransformProperty =
            DependencyProperty.Register("Transform", typeof(SKMatrix), typeof(ComplexWidget), new FrameworkPropertyMetadata(SKMatrix.Identity, flags: FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnPropertyChanged_Transform)));

        private static void OnPropertyChanged_Transform(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            (d as ComplexWidget).InvalidateVisual();
        }

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
                    
                

            _topics.Add(add);
            _topics.Add(pan);

            ViewPort = new ViewPort((float)ActualWidth, (float)ActualHeight);

            var indicator = new IndicatorWidget();

            indicator.Visibility = Visibility.Hidden;
            indicator.Width = 20;
            indicator.Height = 20;
            //SetTop(indicator, ActualHeight / 2);
            //SetLeft(indicator, ActualWidth / 2);
            SetZIndex(indicator, 10);

            Children.Add(indicator);

            SizeChanged += (s, e) => {
                var i = Children.OfType<IndicatorWidget>().First();

                SetTop(i, ActualHeight / 2);
                SetLeft(i, ActualWidth / 2);

                ViewPort.CreateContext((int)ActualWidth, (int)ActualHeight);
                InvalidateVisual();

                ViewPort.Clear();

                foreach (var item in Children.OfType<SKFrameworkElement>()) {
                    item.InvalidateVisual();
                }
            };
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            dc.DrawImage(ViewPort.GetContext(), new Rect(0, 0, ActualWidth, ActualHeight));
        }

        private void OnPanned() {
            ViewPort.Clear();
            //InvalidateVisual();
            Children.OfType<FrameworkElement>().ToList().ForEach(e => e.InvalidateVisual());
        }

        private Point _last;

        private void OnPanning(EventPattern<MouseEventArgs> obj) {
            if (Mode != OperationMode.Panning)
                return;

            if (!IsMouseCaptured) return;

            var curr = obj.EventArgs.GetPosition(this);
            //Console.WriteLine($"Curr: {curr}");
            //Console.WriteLine($"Last: {_last}");
            var v = _last - curr;
            //Console.WriteLine($"V: {v}");
            //var t = Transform;

            ViewPort.Translate((float)-v.X, (float)-v.Y);
            //Transform = SKMatrix.CreateTranslation((float)-v.X, (float)-v.Y);
            //RenderTransform = new TranslateTransform(-v.X, -v.Y);
            //Children.OfType<FrameworkElement>().ToList().ForEach(e => e.InvalidateVisual());
            _last = curr;
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

            if (obj.EventArgs.Key == Key.I) {
                var indicator = Children.OfType<IndicatorWidget>().First();
                if (indicator.IsVisible) {
                    indicator.Visibility = Visibility.Hidden;
                } else {
                    indicator.Visibility = Visibility.Visible;
                }
                //indicator.InvalidateVisual();
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
