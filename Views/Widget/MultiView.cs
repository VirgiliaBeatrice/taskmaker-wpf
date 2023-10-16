using NLog;
using SharpVectors.Converters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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
using Path = System.Windows.Shapes.Path;
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
        Zoom
    }

    public interface IRegionShape {
        BaseRegionState State { get; set; }
    }

    public interface IState {

        void SetContainer();

        void SetFlag();

        void SetOverlay();
    }

    public struct NodeInfo {
        public Point Location { get; set; }
        public int NodeId { get; set; }
        public int UiId { get; set; }
    }

    public static class Helper {

        public static NodeInfo[][] Calc(NodeInfo[] a, NodeInfo[] b) {
            var result = new List<NodeInfo[]>();

            for (int i = 0; i < a.Length; i++) {
                for (int j = 0; j < b.Length; j++) {
                    result.Add(new NodeInfo[] { a[i], b[j] });
                }
            }

            return result.ToArray();
        }

        public static NodeInfo[][] Calc(NodeInfo[] a, NodeInfo[][] b) {
            var result = new List<NodeInfo[]>();

            for (int i = 0; i < a.Length; i++) {
                for (int j = 0; j < b.Length; j++) {
                    result.Add(new NodeInfo[] { a[i] }.Concat(b[j]).ToArray());
                }
            }

            return result.ToArray();
        }

        public static double Cross(this Vector a, Vector b) {
            return a.X * b.Y - a.Y * b.X;
        }

        public static NodeInfo[][] GetCombinations(IEnumerable<NodeInfo[]> nodeSets) {
            if (nodeSets.Count() == 1)
                return nodeSets.ToArray();
            else {
                var a = nodeSets.Take(1).First();
                var b = nodeSets.Skip(1).ToArray();

                NodeInfo[][] result;

                if (b.Count() > 1) {
                    result = Calc(a, GetCombinations(b));
                }
                else {
                    result = Calc(a, b.First());
                }

                return result;
            }
        }

        public static (double, double) Intersect(Point[] lineA, Point[] lineB) {
            var a = lineA[0];
            var b = lineA[1];
            var c = lineB[0];
            var d = lineB[1];

            var dir0 = b - a;
            var dir1 = d - c;

            if (dir0.Cross(dir1) == 0)
                return (double.NaN, double.NaN);

            // p = a + dir0 * t1 = c + dir1 * t2
            // A * T = B
            //var A = new Matrix() {
            //}(2, 2, new float[] { dir0.X, dir1.X, dir0.Y, dir1.Y });
            //var B = (c - a).ToVector();
            //var T = A.Solve(B);

            //return (T[0], T[1]);
            return default;
        }
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

    }
    public class ActivedState : BaseState {

        public ActivedState(StatefulWidget parent) : base(parent) {
        }

        public override void SetContainer() {
        }

        public override void SetFlag() {
        }

        public override void SetOverlay() {
            var grid = Parent.Overlay as Grid;
            var overlay = grid.Children.OfType<Rectangle>().First();
            var icon = grid.Children.OfType<SvgIcon>().First();

            // Set color
            overlay.Fill = Brushes.DarkRed;
            // Set opacity
            overlay.Opacity = 0.12;
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

    public abstract class BaseState : IState {
        public BaseState(StatefulWidget parent) {
            Parent = parent;
        }

        public StatefulWidget Parent { get; set; }
        public abstract void SetContainer();

        public abstract void SetFlag();

        public abstract void SetOverlay();
    }

    public abstract class BaseUiState {
        public BaseUiState(UiController parent) {
            Parent = parent;
        }

        public UiController Parent { get; set; }
        public virtual void SetFlags() {
        }
    }

    public class CommandParameter {
        public object[] Payload { get; set; }
        public string Type { get; set; }
    }

    public class DefaultUiState : BaseUiState {

        public DefaultUiState(UiController parent) : base(parent) {
        }

        public override void SetFlags() {
            //Parent.Pointer.Visibility = Visibility.Collapsed;
        }
    }

    public class Snackbar : UserControl {
        private string icon = "";
        private string supportingText = "";
        private Label _text;
        private Label _icon;

        public string Icon {
            get => icon;
            set {
                icon = value;

                _icon.Content = icon;
            }
        }
        public string SupportingText { get => supportingText; set {
                supportingText = value;

                _text.Content = supportingText;
            } }

        public Snackbar() {
            var invSurfaceColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5EFF7"));
            var invPrimary = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#322F35"));

            invSurfaceColor.Freeze();
            invPrimary.Freeze();

            Effect = Evelations.Lv3;

            var container = new StackPanel {
                Orientation = Orientation.Horizontal,
                Height = 48
            };
            var shape = new Border {
                CornerRadius = new CornerRadius(4),
                Background = invPrimary,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            _text = new Label {
                Content = "Supporting text",
                Margin = new Thickness(12, 0, 12, 0),
                Foreground = invSurfaceColor,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            _icon = new Label {
                Content = "\ue7c9",
                Margin = new Thickness(16, 0, 0, 0),
                FontFamily = new FontFamily("Segoe Fluent Icons"),
                FontSize = 24,
                Foreground = invSurfaceColor,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var close = new Label {
                Content = "Hide",
                Margin = new Thickness(12, 0, 12, 0),
                Foreground = invSurfaceColor,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            close.MouseLeftButtonUp += (_, e) => {
                Visibility = Visibility.Hidden;
            };

            Content = shape;
            shape.Child = container;
            container.Children.Add(_icon);
            container.Children.Add(_text);
            container.Children.Add(close);
        }
    }


    public class MultiView : UserControl {


        // Using a DependencyProperty as the backing store for BindCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindCommandProperty =
            DependencyProperty.Register("BindCommand", typeof(ICommand), typeof(MultiView), new PropertyMetadata(default));

        // Using a DependencyProperty as the backing store for MaxRowCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxColumnCountProperty =
            DependencyProperty.Register("MaxColumnCount", typeof(int), typeof(MultiView), new FrameworkPropertyMetadata(2));

        public List<UiController> Controllers { get; set; } = new List<UiController>();

        public MultiView() : base() {
            var grid = new Grid() {
                Name = "Multiview_Grid",
            };

            Scroll = new ScrollViewer() {
                Focusable = false,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
            };

            grid.Children.Add(Scroll);

            Content = grid;
        }

        public ICommand BindCommand {
            get { return (ICommand)GetValue(BindCommandProperty); }
            set { SetValue(BindCommandProperty, value); }
        }
        public int MaxColumnCount {
            get { return (int)GetValue(MaxColumnCountProperty); }
            set { SetValue(MaxColumnCountProperty, value); }
        }
        public ScrollViewer Scroll { get; set; }

        public ControlUiState[] UiStates { get; set; }

        //public Dictionary<int, NodeInfo[]> Candidates { get; set; } = new Dictionary<int, NodeInfo[]>();
        //public NodeRelationViewer Viewer { get; set; }

        public void Bind(int[] index) {
            BindCommand.Execute(index);

            InvalidateViewer();
            //Console.WriteLine("A binding data has prepared.");
        }

        public void OpenUiController(ControlUiState ui) {
            Layout(ui);
        }

        public void InvalidateViewer() { }

        public void Layout(ControlUiState state) {
            var grid = new Grid() {
                Name = "Multiview_SubGrid",
                Visibility = Visibility.Visible
            };

            Scroll.Content = grid;

            var ui = new UiController {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(16, 16, 8, 16),
            };

            var textblock = new TextBlock {
                Text = state.Name,
                FontSize = 42,
                Foreground = Brushes.DimGray,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            grid.Children.Add(ui);
            grid.Children.Add(textblock);

            Controllers.Clear();
            Controllers.Add(ui);
        }

        public void Layout() {
            //if (ItemsSource == null) return;
            if (UiStates == null) return;

            //var items = ItemsSource.Cast<object>().ToList();
            var grid = new Grid() {
                Name = "Multiview_SubGrid",
                Visibility = Visibility.Visible
            };

            var vm = DataContext as RegionControlUIViewModel;

            Scroll.Content = grid;
            Controllers.Clear();

            if (UiStates.Count() == 1) {
                var ui = new UiController {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(16, 16, 8, 16),
                };

                var textblock = new TextBlock {
                    Text = UiStates[0].ToString(),
                    FontSize = 42,
                    Foreground = Brushes.DimGray,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Right,
                };

                grid.Children.Add(ui);
                grid.Children.Add(textblock);

                Controllers.Add(ui);
            }
            else if (UiStates.Count() == 0) {
                var textblock = new TextBlock {
                    Text = "NULL",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(2),
                };

                grid.Children.Add(textblock);
            }
            else {
                int remainder, quotient = Math.DivRem(UiStates.Count(), MaxColumnCount, out remainder);

                for (int i = 0; i < (remainder == 0 ? quotient : quotient + 1); i++) {
                    grid.RowDefinitions.Add(new RowDefinition() { MinHeight = 400 });
                }

                for (int i = 0; i < MaxColumnCount; i++) {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                for (int i = 0; i < UiStates.Count(); i++) {
                    int r, q = Math.DivRem(i, MaxColumnCount, out r);

                    //var item = new Button() {
                    //    Content = $"TEST{q}{r}",
                    //    Margin = new Thickness(2),

                    //};

                    var ui = new UiController {
                        Margin = new Thickness(2),
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                    };
                    var textblock = new TextBlock {
                        Text = UiStates[i].ToString(),
                        FontSize = 42,
                        Foreground = Brushes.DimGray,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = HorizontalAlignment.Right,
                    };

                    Grid.SetColumn(ui, r);
                    Grid.SetRow(ui, q);

                    Grid.SetColumn(textblock, r);
                    Grid.SetRow(textblock, q);

                    grid.Children.Add(ui);
                    grid.Children.Add(textblock);

                    Controllers.Add(ui);
                }
            }
        }
    }

    public class NodeShape : ContentControl {
        public UIElement Container { get; set; }
        public Ellipse StateLayer { get; set; }
        public Image checkIcon;
        public Color PrimaryColor;
        public UiElementState _state = UiElementState.Default;
        public Point Position {
            get => position;
            set {
                position = value;

                SetPosition(position.X, position.Y);
            }
        }

        public NodeShape(int nodeId) {
            NodeId = nodeId;
            PrimaryColor = ColorManager.GetTintedColor(ColorManager.Palette[0], 2);

            Container = new Grid();
            var shape = new Ellipse {
                Width = 40,
                Height = 40,
                Fill = new SolidColorBrush(PrimaryColor),
                Stroke = new SolidColorBrush(Colors.DarkGray),
                StrokeThickness = 1,
            };

            StateLayer = new Ellipse {
                Width = 40,
                Height = 40,
                Fill = new SolidColorBrush(Colors.Black),
                Visibility = Visibility.Hidden,
                Opacity = 0.08
            };

            (Container as Grid).Children.Add(shape);
            (Container as Grid).Children.Add(StateLayer);

            Content = Container;

            //InitializeComponents();
            //Invalidate();
        }

        public bool IsSelected { get; set; } = false;

        public int NodeId { get; set; }

        private void SetPosition(double x, double y) {
            double halfWidth = 40 / 2;
            double halfHeight = 40 / 2;

            Canvas.SetLeft(this, x - halfWidth);
            Canvas.SetTop(this, y - halfHeight);
        }

        public void GoToState(UiElementState state) {
            _state = state;

            switch (state) {
                case UiElementState.Default:
                    StateLayer.Visibility = Visibility.Hidden;
                    Effect = null;
                    break;
                case UiElementState.Hover:
                    HandleHover();
                    break;
                case UiElementState.Focus:
                    break;
                case UiElementState.Selected:
                    break;
                case UiElementState.Activated:
                    break;
                case UiElementState.Pressed:
                    HandlePressed();
                    break;
                case UiElementState.Dragged:
                    HandleDragged();
                    break;
                default:
                    break;
            }
        }

        private void HandleDragged() {
            StateLayer.Visibility = Visibility.Visible;
            StateLayer.Fill = new SolidColorBrush(Colors.White);
            StateLayer.Opacity = 0.16;
            Effect = Evelations.Lv3;
        }

        private void HandleHover() {
            StateLayer.Visibility = Visibility.Visible;
            StateLayer.Fill = new SolidColorBrush(Colors.Black);
            StateLayer.Opacity = 0.08;
            Effect = Evelations.Lv1;
        }

        private void HandlePressed() {
            StateLayer.Visibility = Visibility.Visible;
            StateLayer.Fill = new SolidColorBrush(Colors.White);
            StateLayer.Opacity = 0.12;
            Effect = null;
            // Create a scale transform and assign it to the Ellipse's RenderTransform
            ScaleTransform transform = new ScaleTransform();
            RenderTransform = transform;
            RenderTransformOrigin = new Point(0.5, 0.5); // Center the transform

            // Create the animation
            DoubleAnimation scaleAnimation = new DoubleAnimation {
                From = 1,
                To = 1.2,  // Scale factor (1.2 means 120% of original size)
                Duration = TimeSpan.FromMilliseconds(100), // Duration to scale up
                AutoReverse = true // Reverse the animation back to original size
            };

            // Apply the animation to the ScaleX and ScaleY properties
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }


        protected override void OnMouseEnter(MouseEventArgs e) {
            base.OnMouseEnter(e);

            GoToState(UiElementState.Hover);
        }

        protected override void OnMouseLeave(MouseEventArgs e) {
            base.OnMouseLeave(e);

            GoToState(UiElementState.Default);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);

            GoToState(UiElementState.Pressed);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            if (_state == UiElementState.Pressed) {
                GoToState(UiElementState.Dragged);
            }
            else if (_state == UiElementState.Dragged) {
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);

            GoToState(UiElementState.Default);
        }

        //private void InitializeComponents() {
        //    InitializeOverlay();
        //    InitializeContent();

        //    Content = Container;
        //}


        private Point position;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    }

    public class PointerWidget : UserControl {
        private Matrix _transform = Matrix.Identity;
        private Point location = new Point();

        public PointerWidget() {
        }

        public Point Location {
            get => location;
            set {
                location = value;

                Invalidate();
            }
        }

        public Matrix Transform {
            get => _transform;
            set {
                var prevT = _transform;
                _transform = value;

                if (prevT != _transform) {
                    Invalidate();
                }
            }
        }
        public void Invalidate() {
            InvalidateContent();
            InvalidateTransform();
        }

        public void InvalidateContent() {
            var circle = new Ellipse {
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.Blue),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 1,
            };

            Content = circle;
        }

        public void InvalidateTransform() {
            var p = Location;
            var tP = Transform.Transform(p);

            Canvas.SetLeft(this, tP.X - 10 / 2);
            Canvas.SetTop(this, tP.Y - 10 / 2);
        }
    }

    public class SimplexShape : UserControl, IRegionShape {
        // Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(IEnumerable<Point>), typeof(SimplexShape), new FrameworkPropertyMetadata(null, OnPointsPropertyChanged));

        private Matrix _transform = Matrix.Identity;
        public SimplexShape(int uiId, BaseRegionState state) {
            UiId = uiId;
            State = state;

            MouseEnter += (s, e) => {
                var v = s as SimplexShape;
                var path = v.Content as Path;

                path.Opacity = 0.1;
            };

            MouseLeave += (s, e) => {
                var v = s as SimplexShape;
                var path = v.Content as Path;

                path.Opacity = 0.3;
            };
        }

        public IEnumerable<Point> Points {
            get { return (IEnumerable<Point>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        public BaseRegionState State { get; set; }
        public Matrix Transform {
            get => _transform;
            set {
                var prevT = _transform;
                _transform = value;

                if (prevT != _transform) {
                    Invalidate();
                }
            }
        }

        public int UiId { get; set; }
        public void Invalidate() {
            var points = Points.Select(e => Transform.Transform(e)).ToArray();

            // 3-simplex
            if (points.Length == 3) {
                var pathGeo = new PathGeometry();
                var pathFig = new PathFigure {
                    StartPoint = points[0],
                };

                pathGeo.Figures.Add(pathFig);
                pathFig.Segments.Add(new LineSegment { Point = points[1] });
                pathFig.Segments.Add(new LineSegment { Point = points[2] });
                pathFig.Segments.Add(new LineSegment { Point = points[0] });

                var fill = ColorManager.GetTintedColor(ColorManager.Palette[UiId], 2);

                var path = new Path {
                    Fill = new SolidColorBrush(fill),
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    Stretch = Stretch.None,
                    StrokeThickness = 1.0,
                    Data = pathGeo
                };

                path.Opacity = 0.3;

                Content = path;
            }
            // 2-simplex
            else if (points.Length == 2) {
                var pathGeo = new PathGeometry();
                var pathFig = new PathFigure {
                    StartPoint = points[0],
                };

                pathGeo.Figures.Add(pathFig);
                pathFig.Segments.Add(new LineSegment { Point = points[1] });
                pathFig.Segments.Add(new LineSegment { Point = points[0] });

                var path = new Path {
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    Stretch = Stretch.None,
                    StrokeThickness = 2.0,
                    Data = pathGeo
                };

                path.Opacity = 0.3;

                Content = path;
            }
        }

        private static void OnPointsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var shape = (SimplexShape)d;

            shape.Invalidate();
        }
    }

    public abstract class StatefulWidget : ContentControl {
        protected IState _state;
        public UIElement Container { get; set; }
        public UIElement Overlay { get; set; }
        public UiElementState State { get; set; }
        public virtual void GoToState(UiElementState state) {
            State = state;

            InvalidateCustomVisual();
        }

        protected void InvalidateCustomVisual() {
            _state.SetOverlay();
            _state.SetContainer();
            _state.SetFlag();
        }
    }


    public class UiController : UserControl {

        // Fields to store the start position for panning
        private Point? _panStartPoint = null;
        private Point _originalContentOffset;

        public ScrollViewer Viewer { get; set; }
        public Canvas Canvas { get; set; }
        public Grid Indicator { get; set; }
        public ScaleTransform ScaleT { get; set; } = new ScaleTransform();
        public TranslateTransform CenteringT = new TranslateTransform();
        public ScaleTransform FlipYT = new ScaleTransform(1, -1);
        public UiMode UiMode {
            get => uiMode;
            set {
                uiMode = value;
                ChangeUiMode(UiMode);
            }
        }
        private Ellipse circle;
        private Rectangle box;
        private TextBlock billboard;
        private UiMode uiMode = UiMode.Default;
        private NodeShape _moveTarget;
        private Point _moveStartMousePosition;
        private Point _moveStartTargetPostion;

        public List<NodeShape> Nodes { get; set; } = new List<NodeShape>();

        public UiController() {
            //Background = new SolidColorBrush(Colors.LightBlue);
            IsHitTestVisible = true;

            Viewer = new ScrollViewer {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };

            Canvas = new Canvas {
                //Background = new SolidColorBrush(Colors.LightGray),
            };

            Indicator = new Grid {
                Visibility = Visibility.Hidden
            };
            Indicator.Children.Add(new Ellipse {
                Width = 20,
                Height = 20,
                Fill = Brushes.White,
                Stroke = Brushes.Gray,
                Opacity = 0.32,
            });

            PreviewMouseWheel += Canvas_PreviewMouseWheel;
            PreviewMouseLeftButtonDown += Canvas_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += Canvas_PreviewMouseLeftButtonUp;
            PreviewMouseMove += Canvas_PreviewMouseMove;

            // add a circle sign at origin
            circle = new Ellipse() {
                Width = 50,
                Height = 50,
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
            };

            box = new Rectangle {
                Width = 100,
                Height = 200,
                Fill = Brushes.AliceBlue,
                Opacity = 0.5
            };
            billboard = new TextBlock {
                Text = "Y+"
            };

            var axisX = new Arrow {
                Name = "AxisX",
                Start = new Point(0, 0),
                End = new Point(100, 0),
                Stroke = Brushes.Red
            };

            var axisY = new Arrow {
                Name = "AxisY",
                Start = new Point(0, 0),
                End = new Point(0, 100),
                Stroke = Brushes.Green
            };

            Canvas.Children.Add(axisX);
            Canvas.Children.Add(axisY);

            Canvas.Children.Add(circle);
            Canvas.Children.Add(box);
            Canvas.Children.Add(billboard);

            Canvas.Children.Add(Indicator);

            Viewer.Content = Canvas;
            Content = Viewer;

        }

        public void ChangeUiMode(UiMode mode) {
            switch (mode) {
                case UiMode.Default:
                    break;
                case UiMode.Add:
                    Indicator.Visibility = Visibility.Visible;
                    break;
                case UiMode.Remove:
                    break;
                case UiMode.Move:
                    break;
                case UiMode.Assign:
                    break;
                case UiMode.Build:
                    break;
                case UiMode.Trace:
                    break;
                case UiMode.Drag:
                    break;
                case UiMode.Pan:
                    break;
                case UiMode.Zoom:
                    break;
                default:
                    break;
            }
        }

        private void Canvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            switch (UiMode) {
                case UiMode.Default:
                    break;
                case UiMode.Add:
                    Indicator.Visibility = Visibility.Hidden;
                    break;
                case UiMode.Remove:
                    PerformRemove(sender, e);
                    break;
                case UiMode.Move:
                    EndMove(sender, e);
                    break;
                case UiMode.Assign:
                    break;
                case UiMode.Build:
                    break;
                case UiMode.Trace:
                    break;
                case UiMode.Drag:
                    break;
                case UiMode.Pan:
                    EndPan(sender, e);
                    break;
                case UiMode.Zoom:
                    break;
                default:
                    break;
            }
        }

        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            switch (UiMode) {
                case UiMode.Default:
                    break;
                case UiMode.Add:
                    PerformAdd(sender, e);
                    break;
                case UiMode.Remove:
                    break;
                case UiMode.Move:
                    StartMove(sender, e);
                    break;
                case UiMode.Assign:
                    break;
                case UiMode.Build:
                    break;
                case UiMode.Trace:
                    break;
                case UiMode.Drag:
                    break;
                case UiMode.Pan:
                    StartPan(sender, e);
                    break;
                case UiMode.Zoom:
                    break;
                default:
                    break;
            }
        }

        private void Canvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            switch (UiMode) {
                case UiMode.Default:
                    break;
                case UiMode.Add:
                    break;
                case UiMode.Remove:
                    break;
                case UiMode.Move:
                    break;
                case UiMode.Assign:
                    break;
                case UiMode.Build:
                    break;
                case UiMode.Trace:
                    break;
                case UiMode.Drag:
                    break;
                case UiMode.Pan:
                    break;
                case UiMode.Zoom:
                    HandleZoom(sender, e);
                    break;
                default:
                    break;
            }
        }

        private void Canvas_PreviewMouseMove(object sender, MouseEventArgs e) {
            switch (UiMode) {
                case UiMode.Default:

                    break;
                case UiMode.Add:
                    ShowAddPointer(sender, e);
                    break;
                case UiMode.Remove:
                    break;
                case UiMode.Move:
                    PerformMove(sender, e);
                    break;
                case UiMode.Assign:
                    break;
                case UiMode.Build:
                    break;
                case UiMode.Trace:
                    break;
                case UiMode.Drag:
                    break;
                case UiMode.Pan:
                    PerformPan(sender, e);
                    break;
                case UiMode.Zoom:
                    break;
                default:
                    break;
            }
        }

        private void StartMove(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);
            var result = VisualTreeHelper.HitTest(Canvas, currentPosition);

            if (result != null && result.VisualHit is Ellipse hit) {
                var parent = VisualTreeHelperExtensions.FindParentOfType<NodeShape>(hit);

                if (parent != null) {
                    _moveTarget = parent;
                    _moveStartMousePosition = currentPosition;
                    _moveStartTargetPostion = parent.Position;
                }
            }
        }

        private void PerformMove(object sender, MouseEventArgs e) {
            if (_moveTarget != null) {
                var currentPosition = e.GetPosition(Canvas);
                var offset = currentPosition - _moveStartMousePosition;

                _moveTarget.Position = _moveStartTargetPostion + offset;
            }
        }

        private void EndMove(object sender, MouseButtonEventArgs e) {
            _moveTarget = null;
        }

        private void PerformRemove(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);

            var result = VisualTreeHelper.HitTest(Canvas, currentPosition);

            if (result != null && result.VisualHit is Ellipse hit) {
                var parent = VisualTreeHelperExtensions.FindParentOfType<NodeShape>(hit);

                if (parent != null) {
                    Canvas.Children.Remove(parent);
                    Nodes.Remove(parent);
                }
            }
        }

        private void SetPosition(UIElement element, double x, double y) {
            double halfWidth = element.DesiredSize.Width / 2;
            double halfHeight = element.DesiredSize.Height / 2;

            if (element is TextBlock)
                element.RenderTransform = new ScaleTransform(1, -1) {
                    CenterX = halfWidth,
                    CenterY = halfHeight
                };

            Canvas.SetLeft(element, x - halfWidth);
            Canvas.SetTop(element, y - halfHeight);

        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            CenteringT = new TranslateTransform(sizeInfo.NewSize.Width / 2, sizeInfo.NewSize.Height / 2);
            FlipYT.CenterX = sizeInfo.NewSize.Width / 2;
            FlipYT.CenterY = sizeInfo.NewSize.Height / 2;

            InvalidateTransform();
            InvaldatePosition();

            base.OnRenderSizeChanged(sizeInfo);
        }

        private void InvalidateTransform() {
            var g = new TransformGroup();

            g.Children.Add(CenteringT);
            g.Children.Add(FlipYT);
            g.Children.Add(ScaleT);

            Canvas.RenderTransform = g;
        }

        private void InvaldatePosition() {

            Nodes.ForEach(e => SetPosition(e, e.Position.X, e.Position.Y));
            SetPosition(circle, 0, 0);
            SetPosition(box, 0, 0);
            SetPosition(billboard, 0, 100);
        }

        private void ShowAddPointer(object sender, MouseEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);

            SetPosition(Indicator, currentPosition.X, currentPosition.Y);
        }

        private void PerformAdd(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);
            var newNode = new NodeShape(Nodes.Count + 1) {
                Position = currentPosition
            };

            Nodes.Add(newNode);
            Canvas.Children.Add(newNode);
        }

        private void HandleZoom(object sender, MouseWheelEventArgs e) {
            // You can adjust these factors as you see fit
            double zoomFactor = 0.001;
            double zoomMax = 2.0;
            double zoomMin = 0.2;

            double scaleFactor = 1.0 + e.Delta * zoomFactor;
            double newScaleX = ScaleT.ScaleX * scaleFactor;
            double newScaleY = ScaleT.ScaleY * scaleFactor;

            // Clamp the values to a min/max value
            newScaleX = Math.Min(zoomMax, Math.Max(zoomMin, newScaleX));
            newScaleY = Math.Min(zoomMax, Math.Max(zoomMin, newScaleY));

            ScaleT.ScaleX = newScaleX;
            ScaleT.ScaleY = newScaleY;
            ScaleT.CenterX = Canvas.RenderSize.Width / 2;
            ScaleT.CenterY = Canvas.RenderSize.Height / 2;

            InvalidateTransform();
        }

        private void StartPan(object sender, MouseButtonEventArgs e) {
            _panStartPoint = e.GetPosition(Canvas);
            _originalContentOffset = new Point(ScaleT.ScaleX, ScaleT.ScaleY);
            CaptureMouse(); // Ensure we get the MouseMove even if the cursor goes out of the Canvas
            Cursor = Cursors.Hand;
        }

        private void PerformPan(object sender, MouseEventArgs e) {
            if (_panStartPoint.HasValue) {
                var currentPosition = e.GetPosition(Canvas);
                var delta = currentPosition - _panStartPoint.Value;

                // Transform the delta using the current zoom
                delta = new Vector(delta.X / ScaleT.ScaleX, delta.Y / ScaleT.ScaleY);

                // Apply the delta to the Canvas offset
                var newOffset = new Point(_originalContentOffset.X - delta.X, _originalContentOffset.Y - delta.Y);

                CenteringT.X -= newOffset.X;
                CenteringT.Y -= newOffset.Y;

                InvalidateTransform();

                //Canvas.SetLeft(Canvas, newOffset.X);
                //Canvas.SetTop(Canvas, newOffset.Y);
            }
        }

        private void EndPan(object sender, MouseButtonEventArgs e) {
            ReleaseMouseCapture();
            _panStartPoint = null;
            Cursor = Cursors.Arrow;
        }
    }

    public class VoronoiShape : UserControl, IRegionShape {
        // Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(IEnumerable<Point>), typeof(VoronoiShape), new FrameworkPropertyMetadata(null, OnPointsPropertyChanged));

        private Matrix _transform = Matrix.Identity;
        public VoronoiShape(int uiId, BaseRegionState state) {
            UiId = uiId;
            State = state;

            MouseEnter += (s, e) => {
                var v = s as VoronoiShape;
                var path = v.Content as Path;

                path.Opacity = 0.1;
            };

            MouseLeave += (s, e) => {
                var v = s as VoronoiShape;
                var path = v.Content as Path;

                path.Opacity = 1;
            };
        }

        public IEnumerable<Point> Points {
            get { return (IEnumerable<Point>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        public BaseRegionState State { get; set; }
        public Matrix Transform {
            get => _transform;
            set {
                var prevT = _transform;
                _transform = value;

                if (prevT != _transform) {
                    Invalidate();
                }
            }
        }

        public int UiId { get; set; }
        public void Invalidate() {
            var points = Points.Select(e => Transform.Transform(e)).ToArray();

            if (points.Length == 3) {
                var radius = (points[1] - points[0]).Length;
                var o = points[1];
                var p0 = points[0];
                var p1 = points[2];

                var p0o = (p0 - o);
                var p1o = (p1 - o);
                var dotProd = (p0o.X * p1o.X) + (p0o.Y * p1o.Y);
                var alpha = Math.Abs(Math.Acos(dotProd / (p0o.Length * p1o.Length)));

                var midLen = (float)Math.Tan(alpha / 2.0f) * Math.Abs(p0o.Length);

                var op0 = o - p0;

                op0.Normalize();
                //var op0 = Point.Normalize(o - p0);
                var transform = Matrix.Identity;
                transform.Rotate(Math.PI * 90.0 / 180.0);
                var midP0 = transform.Transform(op0);
                //var midP0 = SKMatrix.CreateRotation((float)(Math.PI * 90.0 / 180.0)).MapVector(op0);
                midP0 *= midLen;

                var mid = p0 + midP0;

                var pathGeo = new PathGeometry();
                var pathFig = new PathFigure {
                    StartPoint = o,
                };

                pathGeo.Figures.Add(pathFig);

                pathFig.Segments.Add(new LineSegment { Point = p1 });
                pathFig.Segments.Add(new ArcSegment { Point = p0, Size = new Size(radius, radius), SweepDirection = SweepDirection.Counterclockwise });
                pathFig.Segments.Add(new LineSegment { Point = o });

                var fill = ColorManager.GetTintedColor(ColorManager.Palette[UiId], 2);
                var radial = new RadialGradientBrush();
                var radialRadius = (p0 - o).Length;

                radial.MappingMode = BrushMappingMode.Absolute;
                radial.GradientOrigin = o;
                radial.Center = o;
                radial.RadiusX = radialRadius;
                radial.RadiusY = radialRadius;
                radial.GradientStops.Add(new GradientStop(fill, 0.0));
                radial.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
                radial.Freeze();

                var path = new Path {
                    Fill = radial,
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    Stretch = Stretch.None,
                    StrokeThickness = 1.0,
                    Data = pathGeo
                };

                Content = path;
            }
            else {
                var pathGeo = new PathGeometry();
                var pathFig = new PathFigure {
                    StartPoint = points[0],
                };

                pathGeo.Figures.Add(pathFig);

                pathFig.Segments.Add(new LineSegment { Point = points[1] });
                pathFig.Segments.Add(new LineSegment { Point = points[2] });
                pathFig.Segments.Add(new LineSegment { Point = points[3] });
                pathFig.Segments.Add(new LineSegment { Point = points[0] });

                var fill = ColorManager.GetTintedColor(ColorManager.Palette[UiId], 2);
                var linear = new LinearGradientBrush();

                linear.MappingMode = BrushMappingMode.Absolute;
                linear.StartPoint = points[1];
                linear.EndPoint = points[2];
                linear.GradientStops.Add(new GradientStop(fill, 0.0));
                linear.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
                linear.Freeze();

                var path = new Path {
                    Fill = linear,
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    Stretch = Stretch.None,
                    StrokeThickness = 1.0,
                    Data = pathGeo
                };

                Content = path;
            }
        }

        private static void OnPointsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var shape = (VoronoiShape)d;

            shape.Invalidate();
        }
    }
}