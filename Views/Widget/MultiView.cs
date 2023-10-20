using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Messaging;
using Numpy;
using SharpVectors.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        Trace,
        Drag,
        Pan,
        Zoom,
        Reset,
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

    public struct MapEntry<TInput, TOutput> {
        public TInput Input { get; set; }
        public TOutput Output { get; set; }

        public override string ToString() {
            return $"[{Input} -> {Output}]";
        }
    }

    public class MultiView : UserControl {
        public IEnumerable<ControlUiState> UiStates {
            get { return (IEnumerable<ControlUiState>)GetValue(UiStatesProperty); }
            set { SetValue(UiStatesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UiStates.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UiStatesProperty =
            DependencyProperty.Register("UiStates", typeof(IEnumerable<ControlUiState>), typeof(MultiView), new PropertyMetadata(null, OnUiStatesChanged));



        public IEnumerable<NLinearMapState> NLinearMapStates {
            get { return (IEnumerable<NLinearMapState>)GetValue(NLinearMapStatesProperty); }
            set { SetValue(NLinearMapStatesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NLinearMapStates.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NLinearMapStatesProperty =
            DependencyProperty.Register("NLinearMapStates", typeof(IEnumerable<NLinearMapState>), typeof(MultiView), new PropertyMetadata(null, OnMapStatesChanged));

        private static void OnMapStatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = d as MultiView;
            var oldState = e.OldValue as ObservableCollection<ControlUiState>;
            var newState = e.NewValue as ObservableCollection<ControlUiState>;

            if (oldState != null) {
                // Detach from the old dictionary's events.
                oldState.CollectionChanged -= control.MapStates_CollectionChanged;
            }

            if (newState != null) {
                // Attach to the new dictionary's events.
                newState.CollectionChanged += control.MapStates_CollectionChanged;
            }
        }

        private void MapStates_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add) {
                foreach (var item in e.NewItems) {
                    var map = item as NLinearMapState;

                    map.PropertyChanged += Map_PropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove) {
                foreach (var item in e.OldItems) {
                    var map = item as NLinearMapState;

                    map.PropertyChanged -= Map_PropertyChanged;
                }
            }
        }

        private static void OnUiStatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = d as MultiView;
            var oldState = e.OldValue as ObservableCollection<ControlUiState>;
            var newState = e.NewValue as ObservableCollection<ControlUiState>;

            if (oldState != null) {
                // Detach from the old dictionary's events.
                oldState.CollectionChanged -= control.UiStates_CollectionChanged;
            }

            if (newState != null) {
                // Attach to the new dictionary's events.
                newState.CollectionChanged += control.UiStates_CollectionChanged;
            }

            // Optional: handle the change of the entire dictionary if needed.
        }

        private void UiStates_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            // Handle the change of the dictionary here.
            //var control = sender as MultiView;

            if (e.Action == NotifyCollectionChangedAction.Add) {
                foreach (var item in e.NewItems) {
                    var ui = item as ControlUiState;

                    ui.PropertyChanged += Ui_PropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove) {
                foreach (var item in e.OldItems) {
                    var ui = item as ControlUiState;

                    ui.PropertyChanged -= Ui_PropertyChanged;
                }
            }

        }


        private void Map_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var mapState = sender as NLinearMapState;
            var control = sender as MultiView;

            if (e.PropertyName == nameof(NLinearMapState.Entries)) {

            }
        }

        private void Ui_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var uiState = sender as ControlUiState;
            var control = sender as MultiView;

            if (e.PropertyName == nameof(ControlUiState.Nodes)) {
                if (Controllers.TryGetValue(uiState.Id, out var controller)) {
                    controller.InvalidateNodes();
                }
            }
            else if (e.PropertyName == nameof(ControlUiState.Regions)) {
                if (Controllers.TryGetValue(uiState.Id, out var controller)) {
                    //controller.InvalidateNodes();
                    controller.InvalidateRegion();
                }
            }
        }

        private UiMode uiMode;

        public Dictionary<int, UiController> Controllers { get; set; } = new Dictionary<int, UiController>();
        public Dictionary<int, NodeShape> SelectedNodes { get; set; } = new Dictionary<int, NodeShape>();
        public Dictionary<int[], MapEntry<double[], double[]>> MapEntries { get; set; } = new Dictionary<int[], MapEntry<double[], double[]>>();

        public UiMode UiMode {
            get => uiMode;
            set {
                uiMode = value;

                foreach (var controller in Controllers.Values) {
                    controller.UiMode = uiMode;
                }
            }
        }

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

            MouseDoubleClick += MultiView_MouseDoubleClick;
        }

        private void MultiView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            PerformSelection(e);

            if (SelectedNodes.Values.All(v => v != null)) {
                // Perform assignment with selected nodes
                WeakReferenceMessenger.Default.Send(new ShowDialogMessage());
            }
            else {

            }
        }

        private void PerformSelection(System.Windows.Input.MouseButtonEventArgs e) {
            var position = e.GetPosition(this);

            var result = VisualTreeHelper.HitTest(this, position);

            if (result != null) {
                var parent = VisualTreeHelperExtensions.FindParentOfType<NodeShape>(result.VisualHit);

                if (parent != null) {
                    // if node is already selected, deselect it
                    // only one node could be selected simutaneously
                    if (SelectedNodes[parent.Ui.UiState.Id] == parent) {
                        SelectedNodes[parent.Ui.UiState.Id] = null;
                        parent.Select(false);
                    }
                    else {
                        // deselect all nodes
                        foreach (var node in parent.Ui.Nodes) {
                            if (node != null) {
                                node.Select(false);
                            }
                        }

                        // select node
                        SelectedNodes[parent.Ui.UiState.Id] = parent;
                        parent.Select(true);
                    }


                }
            }
        }

        private readonly Grid _grid;

        private ScrollViewer _scroll;

        public void Open(ControlUiState state) {
            var uiController = new UiController(state) {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(16, 16, 16, 16),
            };

            Controllers[state.Id] = uiController;
            SelectedNodes[state.Id] = null;

            Layout(state);

            uiController.InvalidateNodes();
            uiController.Invalidate();
        }

        private void InitializeMapEntries() {

        }

        public void Close() {
            _scroll.Content = null;
            Controllers.Clear();
        }

        public void Layout(ControlUiState state) {
            var grid = new Grid() {
                Name = "Multiview_SubGrid",
                Visibility = Visibility.Visible
            };

            _scroll.Content = grid;

            var textblock = new TextBlock {
                Text = state.Name,
                FontSize = 48,
                Foreground = Brushes.DimGray,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            grid.Children.Add(Controllers[state.Id]);
            grid.Children.Add(textblock);
        }
    }
}