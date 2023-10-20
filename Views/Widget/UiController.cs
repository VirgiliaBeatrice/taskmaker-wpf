using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using taskmaker_wpf.ViewModels;
using static System.Windows.Forms.AxHost;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace taskmaker_wpf.Views.Widget {
    public class UiController : UserControl {

        // Fields to store the start position for panning
        private Point? _panStartPoint = null;
        private Point _originalContentOffset;
        private Grid _grid;

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

        public RegionControlUI RegionUi { get; set; }
        public MultiView MultiView { get; set; }

        public List<NodeShape> Nodes { get; set; } = new List<NodeShape>();
        public List<SimplexShape> Simplices { get; set; } = new List<SimplexShape>();
        public List<VoronoiShape> Voronois { get; set; } = new List<VoronoiShape>();

        public ControlUiState UiState { get; set; }

        public UiController(ControlUiState uiState) {
            UiState = uiState;

            Loaded += (_, e) => {
                RegionUi = VisualTreeHelperExtensions.FindParentOfType<RegionControlUI>(this);
                MultiView = VisualTreeHelperExtensions.FindParentOfType<MultiView>(this);

                RegionUi.UiStatusText = UiMode.ToString();
            };
            //_regionUi = VisualTreeHelperExtensions.FindParentOfType<RegionControlUI>(this);

            //Background = new SolidColorBrush(Colors.LightBlue);
            IsHitTestVisible = true;

            _grid = new Grid();

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

            //box = new Rectangle {
            //    Width = 100,
            //    Height = 200,
            //    Fill = Brushes.AliceBlue,
            //    Opacity = 0.5
            //};
            //billboard = new TextBlock {
            //    Text = "Y+"
            //};

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
            //Canvas.Children.Add(box);
            //Canvas.Children.Add(billboard);

            Canvas.Children.Add(Indicator);

            Viewer.Content = Canvas;

            _grid.Children.Add(Viewer);

            Content = _grid;

        }

        public void Invalidate() {
            InvalidateNodes();
            InvalidateRegion();
        }

        public void InvalidateNodes() {
            // Clear all new nodes.
            foreach (var node in Nodes) {
                Canvas.Children.Remove(node);
            }

            Nodes.Clear();

            // Add nodes
            foreach (var node in UiState.Nodes) {
                var nodeShape = new NodeShape(node.Id) {
                    Position = node.Value,
                };

                Nodes.Add(nodeShape);
                Canvas.Children.Add(nodeShape);
            }
        }

        public void ClearRegion() {
            foreach (var region in Simplices) {
                Canvas.Children.Remove(region);
            }
            foreach (var region in Voronois) {
                Canvas.Children.Remove(region);
            }

            Simplices.Clear();
            Voronois.Clear();
        }

        public void InvalidateRegion() {
            // clear all regions
            ClearRegion();

            // add regions
            foreach (var region in UiState.Regions) {
                if (region is SimplexState s) {
                    var simplex = new SimplexShape(region.Id, region.Vertices);

                    Simplices.Add(simplex);
                    Canvas.Children.Add(simplex);
                }
                else if (region is VoronoiState v) {
                    var voronoi = new VoronoiShape(region.Id, region.Vertices);

                    Voronois.Add(voronoi);
                    Canvas.Children.Add(voronoi);
                }
                else
                    throw new InvalidOperationException();
            }
        }

        // Single controller
        public double[][] GetActuationValues() {
            return Nodes.Select(e => e.ActuationValue).ToArray();
        }

        public double[][] GetPositionValues() {
            return Nodes.Select(e => new double[] { e.Position.X, e.Position.Y }).ToArray();
        }

        private void ChangeUiMode(UiMode mode) {
            switch (mode) {
                case UiMode.Default:
                    break;
                case UiMode.Add:
                    Indicator.Visibility = Visibility.Visible;
                    ClearRegion();
                    break;
                case UiMode.Remove:
                    ClearRegion();
                    break;
                case UiMode.Move:
                    ClearRegion();
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
                case UiMode.Reset:
                    ScaleT.ScaleX = 1;
                    ScaleT.ScaleY = 1;
                    CenteringT.X = RenderSize.Width / 2;
                    CenteringT.Y = RenderSize.Height / 2;
                    InvalidateTransform();
                    break;
                default:
                    break;
            }

            if (RegionUi != null)
                RegionUi.UiStatusText = mode.ToString();
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
            if (_moveTarget != null) {
                var id = _moveTarget.NodeId;
                var position = _moveTarget.Position;

                UiState.Nodes = UiState.Nodes.Select(e => {
                    if (e.Id == id)
                        return new NodeState { Id = id, Value = position };
                    else
                        return e;
                }).ToArray();


                _moveTarget = null;
            }
        }

        private void PerformRemove(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);

            var result = VisualTreeHelper.HitTest(Canvas, currentPosition);

            if (result != null && result.VisualHit is Ellipse hit) {
                var parent = VisualTreeHelperExtensions.FindParentOfType<NodeShape>(hit);

                if (parent != null) {
                    var id = parent.NodeId;

                    UiState.Nodes = UiState.Nodes.Where(e => e.Id != id).ToArray();

                    //Canvas.Children.Remove(parent);
                    //Nodes.Remove(parent);
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
            //SetPosition(box, 0, 0);
            //SetPosition(billboard, 0, 100);
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

            var nodes = UiState.Nodes;

            UiState.Nodes = nodes.Append(new NodeState { Id = nodes.Length + 1, Value = currentPosition }).ToArray();

            //Nodes.Add(newNode);
            //Canvas.Children.Add(newNode);
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
}