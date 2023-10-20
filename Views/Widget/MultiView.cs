using CommunityToolkit.Mvvm.Collections;
using SharpVectors.Converters;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
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
using Rectangle = System.Windows.Shapes.Rectangle;

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

    public static class Helper {

        //public static NodeInfo[][] Calc(NodeInfo[] a, NodeInfo[] b) {
        //    var result = new List<NodeInfo[]>();

        //    for (int i = 0; i < a.Length; i++) {
        //        for (int j = 0; j < b.Length; j++) {
        //            result.Add(new NodeInfo[] { a[i], b[j] });
        //        }
        //    }

        //    return result.ToArray();
        //}

        //public static NodeInfo[][] Calc(NodeInfo[] a, NodeInfo[][] b) {
        //    var result = new List<NodeInfo[]>();

        //    for (int i = 0; i < a.Length; i++) {
        //        for (int j = 0; j < b.Length; j++) {
        //            result.Add(new NodeInfo[] { a[i] }.Concat(b[j]).ToArray());
        //        }
        //    }

        //    return result.ToArray();
        //}

        //public static double Cross(this Vector a, Vector b) {
        //    return a.X * b.Y - a.Y * b.X;
        //}

        //public static NodeInfo[][] GetCombinations(IEnumerable<NodeInfo[]> nodeSets) {
        //    if (nodeSets.Count() == 1)
        //        return nodeSets.ToArray();
        //    else {
        //        var a = nodeSets.Take(1).First();
        //        var b = nodeSets.Skip(1).ToArray();

        //        NodeInfo[][] result;

        //        if (b.Count() > 1) {
        //            result = Calc(a, GetCombinations(b));
        //        }
        //        else {
        //            result = Calc(a, b.First());
        //        }

        //        return result;
        //    }
        //}

        //public static (double, double) Intersect(Point[] lineA, Point[] lineB) {
        //    var a = lineA[0];
        //    var b = lineA[1];
        //    var c = lineB[0];
        //    var d = lineB[1];

        //    var dir0 = b - a;
        //    var dir1 = d - c;

        //    if (dir0.Cross(dir1) == 0)
        //        return (double.NaN, double.NaN);

        //    // p = a + dir0 * t1 = c + dir1 * t2
        //    // A * T = B
        //    //var A = new Matrix() {
        //    //}(2, 2, new float[] { dir0.X, dir1.X, dir0.Y, dir1.Y });
        //    //var B = (c - a).ToVector();
        //    //var T = A.Solve(B);

        //    //return (T[0], T[1]);
        //    return default;
        //}
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


    public class MultiView : UserControl {
        public IEnumerable<ControlUiState> UiStates {
            get { return (IEnumerable<ControlUiState>)GetValue(UiStatesProperty); }
            set { SetValue(UiStatesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UiStates.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UiStatesProperty =
            DependencyProperty.Register("UiStates", typeof(IEnumerable<ControlUiState>), typeof(MultiView), new PropertyMetadata(null, OnUiStatesChanged));

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
                var ui = e.NewItems[0] as ControlUiState;

                ui.PropertyChanged += Ui_PropertyChanged;
            }

        }

        private void Ui_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var uiState = sender as ControlUiState;
            var control = Controllers[uiState.Id];

            if (e.PropertyName == nameof(ControlUiState.Nodes)) {

                // Clear all new nodes.
                foreach(var node in control.Nodes) {
                    control.Canvas.Children.Remove(node);
                }

                control.Nodes.Clear();

                // Add nodes
                foreach (var node in uiState.Nodes) {
                    var nodeShape = new NodeShape(node.Id) {
                        Position = node.Value,
                    };

                    control.Nodes.Add(nodeShape);
                    control.Canvas.Children.Add(nodeShape);
                }
            }
        }


        // Using a DependencyProperty as the backing store for MaxRowCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxColumnCountProperty =
            DependencyProperty.Register("MaxColumnCount", typeof(int), typeof(MultiView), new FrameworkPropertyMetadata(2));

        public Dictionary<int, UiController> Controllers { get; set; } = new Dictionary<int, UiController>();

        public double[][] ActuationaValues => Controllers.Values.First()?.GetActuationValues();
        public double[][] PositionValues => Controllers.Values.First()?.GetPositionValues();

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
        }

        public int MaxColumnCount {
            get { return (int)GetValue(MaxColumnCountProperty); }
            set { SetValue(MaxColumnCountProperty, value); }
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

            Layout(state);
        }

        public void Close() {
            _scroll.Content = null;
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

        //public void Layout() {
        //    //if (ItemsSource == null) return;
        //    if (UiStates == null) return;

        //    //var items = ItemsSource.Cast<object>().ToList();
        //    var grid = new Grid() {
        //        Name = "Multiview_SubGrid",
        //        Visibility = Visibility.Visible
        //    };

        //    var vm = DataContext as RegionControlUIViewModel;

        //    Scroll.Content = grid;
        //    Controllers.Clear();

        //    if (UiStates.Count() == 1) {
        //        var ui = new UiController {
        //            VerticalAlignment = VerticalAlignment.Stretch,
        //            HorizontalAlignment = HorizontalAlignment.Stretch,
        //            Margin = new Thickness(16, 16, 8, 16),
        //        };

        //        var textblock = new TextBlock {
        //            Text = UiStates[0].ToString(),
        //            FontSize = 42,
        //            Foreground = Brushes.DimGray,
        //            VerticalAlignment = VerticalAlignment.Bottom,
        //            HorizontalAlignment = HorizontalAlignment.Right,
        //        };

        //        grid.Children.Add(ui);
        //        grid.Children.Add(textblock);

        //        Controllers.Add(ui);
        //    }
        //    else if (UiStates.Count() == 0) {
        //        var textblock = new TextBlock {
        //            Text = "NULL",
        //            VerticalAlignment = VerticalAlignment.Center,
        //            HorizontalAlignment = HorizontalAlignment.Center,
        //            Margin = new Thickness(2),
        //        };

        //        grid.Children.Add(textblock);
        //    }
        //    else {
        //        int remainder, quotient = Math.DivRem(UiStates.Count(), MaxColumnCount, out remainder);

        //        for (int i = 0; i < (remainder == 0 ? quotient : quotient + 1); i++) {
        //            grid.RowDefinitions.Add(new RowDefinition() { MinHeight = 400 });
        //        }

        //        for (int i = 0; i < MaxColumnCount; i++) {
        //            grid.ColumnDefinitions.Add(new ColumnDefinition());
        //        }

        //        for (int i = 0; i < UiStates.Count(); i++) {
        //            int r, q = Math.DivRem(i, MaxColumnCount, out r);

        //            //var item = new Button() {
        //            //    Content = $"TEST{q}{r}",
        //            //    Margin = new Thickness(2),

        //            //};

        //            var ui = new UiController(0) {
        //                Margin = new Thickness(2),
        //                VerticalAlignment = VerticalAlignment.Stretch,
        //                HorizontalAlignment = HorizontalAlignment.Stretch,
        //            };
        //            var textblock = new TextBlock {
        //                Text = UiStates[i].ToString(),
        //                FontSize = 42,
        //                Foreground = Brushes.DimGray,
        //                VerticalAlignment = VerticalAlignment.Bottom,
        //                HorizontalAlignment = HorizontalAlignment.Right,
        //            };

        //            Grid.SetColumn(ui, r);
        //            Grid.SetRow(ui, q);

        //            Grid.SetColumn(textblock, r);
        //            Grid.SetRow(textblock, q);

        //            grid.Children.Add(ui);
        //            grid.Children.Add(textblock);

        //            Controllers.Add(ui);
        //        }
        //    }
        //}
    }
}