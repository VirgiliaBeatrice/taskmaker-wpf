using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Messaging;
using Numpy;
using SharpVectors.Converters;
using SharpVectors.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using taskmaker_wpf.ViewModels;
using Unity;
using Point = System.Windows.Point;

namespace taskmaker_wpf.Views.Widget {

    public enum UiElementState {
        Default = 0,
        Hover,
        Focus,
        Selected,
        Activated,
        Pressed,
        Dragged
    }

    public enum UiMode {
        Default = 0,
        Add,
        Remove,
        Move,
        Assign,
        Build,
        Control,
        Drag,
        Pan,
        Zoom,
        Reset,
    }

    public struct MapEntry {
        public MapEntry() { }

        public int[] Indices { get; set; } = new int[0];
        public int[] IDs { get; set; } = new int[0];
        public double[] Value { get; set; } = new double[0];

        public bool IsInvalid => Value.Any(double.IsNaN);

        public override string ToString() {
            return $"[({string.Join(",", IDs)}) -> [{(IsInvalid ? "NaN" : "Set")}]]";
        }

        public override bool Equals(object obj) {
            if (obj == null || this.GetType() != obj.GetType()) {
                return false;
            }

            var other = (MapEntry)obj;
            return Enumerable.SequenceEqual(this.Indices, other.Indices);
        }

        public override int GetHashCode() {
            return (Indices != null) ? Indices.Aggregate(0, (acc, val) => acc ^ val) : 0;
        }

        public static bool operator ==(MapEntry left, MapEntry right) {
            return left.Equals(right);
        }

        public static bool operator !=(MapEntry left, MapEntry right) {
            return !(left == right);
        }
    }

    public struct StateInfo<T> {
        public int Id { get; set; }
        public T Value { get; set; }
    }

    public static class Evelations {
        public static DropShadowEffect Lv1 = new DropShadowEffect {
            Color = Colors.Black,
            Direction = 270,
            ShadowDepth = 1,
            BlurRadius = 8,
            Opacity = 0.32,
        };

        public static DropShadowEffect Lv3 = new DropShadowEffect {
            Color = Colors.Black,
            Direction = 270,
            ShadowDepth = 6,
            BlurRadius = 8,
            Opacity = 0.32,
        };
    }
    public static class LogicalTreeHelperExtensions {

        public static T FindAncestor<T>(DependencyObject dependencyObject) where T : class {
            var target = dependencyObject;

            do {
                target = LogicalTreeHelper.GetParent(target);
            }
            while (target != null && !(target is T));

            return target as T;
        }
    }

    public static class VisualTreeHelperExtensions {

        public static T FindAncestor<T>(DependencyObject dependencyObject)
            where T : class {
            DependencyObject target = dependencyObject;
            do {
                target = VisualTreeHelper.GetParent(target);
            }
            while (target != null && !(target is T));
            return target as T;
        }

        public static T FindParentByName<T>(DependencyObject child, string name) where T : FrameworkElement {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // If we reach the top of the visual tree, return null
            if (parentObject == null) return null;

            // Check if the parent object is of the specified type and matches the name
            if (parentObject is T parent && parent.Name == name) {
                return parent;
            }
            else {
                // Recursively call this function to check the next level up
                return FindParentByName<T>(parentObject, name);
            }
        }

        public static T FindParentOfType<T>(DependencyObject child) where T : DependencyObject {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // Return null if reached the end of the visual tree
            if (parentObject == null) return null;

            // Check if the parent is of the desired type
            if (parentObject is T parent) {
                return parent;
            }
            else {
                // Recursively call the function to move further up the visual tree
                return FindParentOfType<T>(parentObject);
            }
        }
    }

    public class Arrow : Shape {

        // Using a DependencyProperty as the backing store for End.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End", typeof(Point), typeof(Arrow), new PropertyMetadata(new Point(), OnPropertyChanged));

        // Using a DependencyProperty as the backing store for Start.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start", typeof(Point), typeof(Arrow), new PropertyMetadata(new Point(), (PropertyChangedCallback)OnPropertyChanged));

        // Using a DependencyProperty as the backing store for Transform.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransformProperty =
            DependencyProperty.Register("Transform", typeof(Matrix), typeof(Arrow), new PropertyMetadata(Matrix.Identity, OnPropertyChanged));

        public Arrow() {
            Stroke = Brushes.Green;
            //SnapsToDevicePixels = false;
            //UseLayoutRounding = false;
        }

        public Point End {
            get { return (Point)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }

        public Point Start {
            get { return (Point)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }

        public Matrix Transform {
            get { return (Matrix)GetValue(TransformProperty); }
            set { SetValue(TransformProperty, value); }
        }
        protected override Geometry DefiningGeometry => Generate();

        public static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            (d as Arrow).InvalidateVisual();
        }
        private Geometry Generate() {
            var start = Transform.Transform(Start);
            var end = Transform.Transform(End);

            var arrowLine = new PathFigure();

            if (start == end)
                return new LineGeometry();

            arrowLine.StartPoint = start;

            arrowLine.Segments.Add(new LineSegment(end, true));

            var vector = (start - end);

            vector.Normalize();

            var rotate = Matrix.Identity;

            rotate.Rotate(15);

            var e0 = rotate.Transform(vector * 10.0) + end;

            rotate.SetIdentity();
            rotate.Rotate(-15);

            var e1 = rotate.Transform(vector * 10.0) + end;

            var arrowTip = new PathFigure();

            arrowTip.StartPoint = end;

            arrowTip.Segments.Add(new LineSegment(e0, true));
            arrowTip.Segments.Add(new LineSegment(e1, true));
            arrowTip.Segments.Add(new LineSegment(end, true));

            var geometry = new PathGeometry();

            geometry.Figures.Add(arrowLine);
            geometry.Figures.Add(arrowTip);

            return geometry;
        }
    }

    public class ArrowLine : Shape {

        // Using a DependencyProperty as the backing store for End.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End", typeof(Point), typeof(ArrowLine), new PropertyMetadata(new Point(0, 0), OnPropertyChanged));

        // Using a DependencyProperty as the backing store for Start.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start", typeof(Point), typeof(ArrowLine), new PropertyMetadata(new Point(0, 0), OnPropertyChanged));

        public ArrowLine(Point start, Point end) {
            Start = start;
            End = end;
            //Stroke = Brushes.Black;
            StrokeThickness = 2;
            StrokeLineJoin = PenLineJoin.Bevel;
        }

        public Point End {
            get { return (Point)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }

        public Point Start {
            get { return (Point)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }
        protected override Geometry DefiningGeometry {
            get {
                return GetGeometry();
            }
        }

        protected Geometry Geometry { get; set; }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var arrow = (ArrowLine)d;
            arrow.Invalidate();
        }

        private Geometry GetGeometry() {
            var start = Start;
            var end = End;

            // Calculate the line segment vector
            Vector lineSegment = end - start;

            // Normalize the line segment vector
            Vector lineDirection = lineSegment;
            lineDirection.Normalize();

            // Calculate the perpendicular vector to the line segment vector
            Vector perpendicularVector = new Vector(-lineSegment.Y, lineSegment.X);
            perpendicularVector.Normalize();

            // Define the length and width of the arrowhead
            double arrowheadLength = 20;
            double arrowheadWidth = 8;

            // Calculate the endpoints of the arrowhead
            Point arrowheadEndpoint1 = end - (lineDirection * arrowheadLength) + (perpendicularVector * arrowheadWidth);
            Point arrowheadEndpoint2 = end - (lineDirection * arrowheadLength) - (perpendicularVector * arrowheadWidth);

            // Define the points of the arrow line
            PointCollection points = new PointCollection {
                start,
                end,
                arrowheadEndpoint1,
                arrowheadEndpoint2,
                end,
            };

            // Create PathFigure with points
            PathFigure figure = new PathFigure {
                StartPoint = start,
                Segments = new PathSegmentCollection {
                    new PolyLineSegment(points, true)
                },
            };

            // Create PathGeometry with PathFigure
            PathGeometry geometry = new PathGeometry {
                Figures = new PathFigureCollection {
                    figure
                }
            };

            return geometry;
        }

        private void Invalidate() {
            var geometry = GetGeometry();
            Geometry = geometry;

            InvalidateVisual();
        }
    }
    public class MultiView : UserControl {



        public SessionViewModel SessionViewModel {
            get { return (SessionViewModel)GetValue(SessionViewModelProperty); }
            set { SetValue(SessionViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SessionViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SessionViewModelProperty =
            DependencyProperty.Register("SessionViewModel", typeof(SessionViewModel), typeof(MultiView), new PropertyMetadata(null, OnSessionViewModelChanged));

        private static void OnSessionViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = d as MultiView;

            if (e.NewValue != null) {
                var vm = e.NewValue as SessionViewModel;

                vm.UiViewModels.CollectionChanged += control.UiViewModels_CollectionChanged;
                vm.MapViewModel.PropertyChanged += control.MapViewModel_PropertyChanged;
            }

            if (e.OldValue != null) {
                var vm = e.OldValue as SessionViewModel;

                vm.UiViewModels.CollectionChanged -= control.UiViewModels_CollectionChanged;
                vm.MapViewModel.PropertyChanged -= control.MapViewModel_PropertyChanged;
            }
        }

        private void MapViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            //throw new NotImplementedException();
        }

        private void UiViewModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            Close();
            Open();
        }

        private readonly Grid _grid;

        private ScrollViewer _scroll;

        private UiMode uiMode;

        public MultiView() : base() {
            _grid = new Grid() {
                Name = "Multiview_Grid",
            };

            _scroll = new ScrollViewer() {
                Focusable = false,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
            };

            _grid.Children.Add(_scroll);

            Content = _grid;

            WeakReferenceMessenger.Default.Register<UiControllerSelectedMessage>(this, async (r, m) => {
                var controller = m.Sender as UiController;
                var view = r as MultiView;

                SessionViewModel.SelectedNodeStates = view.Controllers.Select(c => c.SelectedNode.State).ToArray();
                
                await view.RequestMotorDialog();
            });
        }

        public List<UiController> Controllers { get; set; } = new List<UiController>();

        //public Dictionary<NDKey, MapEntry> Entries { get; set; } = new Dictionary<NDKey, MapEntry>();

        public NodeShape[] SelectedNodes => Controllers.Select(c => c.SelectedNode).ToArray();

        public RegionControlUIViewModel RegionViewModel => DataContext as RegionControlUIViewModel;

        public UiMode UiMode {
            get => uiMode;
            set {
                uiMode = value;

                foreach (var controller in Controllers) {
                    controller.UiMode = uiMode;
                }
            }
        }

        public ControlUiViewModel UiViewModel { get; set; }
        public void Close() {
            _scroll.Content = null;
            Controllers.Clear();
        }

        public void Layout() {
            var grid = new Grid() {
                Name = "Multiview_SubGrid",
                Visibility = Visibility.Visible
            };

            _scroll.Content = grid;

            //var textblock = new TextBlock {
            //    Text = state.Name,
            //    FontSize = 48,
            //    Foreground = Brushes.DimGray,
            //    VerticalAlignment = VerticalAlignment.Bottom,
            //    HorizontalAlignment = HorizontalAlignment.Right,
            //};

            // single
            grid.Children.Add(Controllers[0]);
            //grid.Children.Add(textblock);
        }

        public void Open() {
            foreach(var uiViewModel in SessionViewModel.UiViewModels) {
                // 1. Open Ui
                var uiController = new UiController() {
                    DataContext = uiViewModel,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(16, 16, 16, 16),
                };

                // data binding
                Binding binding;

                binding = new Binding("NodeStates") {
                    Source = uiViewModel,
                };
                uiController.SetBinding(UiController.NodeStatesProperty, binding);

                binding = new Binding("RegionStates") {
                    Source = uiViewModel,
                };
                uiController.SetBinding(UiController.RegionStatesProperty, binding);

                // Register messages

                // TODO: potential memory leak because of unregistering

                Controllers.Add(uiController);

            }

            Layout();
        }


        public async Task RequestMotorDialog() {
            if (UiMode == UiMode.Assign && SelectedNodes.All(v => v != null)) {
                // Send dialog message
                var result = await WeakReferenceMessenger.Default.Send(new DialogRequestMessage());

                if (result.Result == MessageBoxResult.OK) {
                    // find index of selected node according to its ID
                    //var indices = new List<int>();

                    //foreach (var item in Controllers) {
                    //    var index = item.NodeStates.ToList().IndexOf(item.SelectedNode.State);

                    //    indices.Add(index);
                    //}

                    //var entry = new MapEntry {
                    //    IDs = SelectedNodes.Select(e => e.State.Id).ToArray(),
                    //    Indices = indices.ToArray(),
                    //    Value = result.Value as double[],
                    //};

                    // Commit map entry
                    //SessionViewModel.MapViewModel.SetValue(entry);
                    SessionViewModel.SetValue(result.Value as double[]);
                }
            }
        }
    }
}