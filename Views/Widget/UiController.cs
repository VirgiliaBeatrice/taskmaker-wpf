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
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;
using CommunityToolkit.Mvvm.Messaging;

namespace taskmaker_wpf.Views.Widget {
    public class UiController : UserControl {


        public IEnumerable<BaseRegionState> RegionStates {
            get { return (IEnumerable<BaseRegionState>)GetValue(RegionStatesProperty); }
            set { SetValue(RegionStatesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RegionStates.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty regionStatesProperty =
            DependencyProperty.Register("RegionStates", typeof(IEnumerable<BaseRegionState>), typeof(UiController), new PropertyMetadata(Array.Empty<BaseRegionState>(), OnRegionStatesChanged
                ));

        private static void OnRegionStatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = d as UiController;

            // handle DependencyPropertyChangedEventArgs
            control.InvalidateRegion();
        }

        // Using a DependencyProperty as the backing store for AddNodeCommand.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty AddNodeCommandProperty =
        //    DependencyProperty.Register("AddNodeCommand", typeof(ICommand), typeof(UiController), new PropertyMetadata(null));

        //// Using a DependencyProperty as the backing store for DeleteNodeCommand.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty DeleteNodeCommandProperty =
        //    DependencyProperty.Register("DeleteNodeCommand", typeof(ICommand), typeof(UiController), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for Nodes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NodeStatesProperty =
            DependencyProperty.Register("NodeStates", typeof(IEnumerable<NodeState>), typeof(UiController), new PropertyMetadata(Array.Empty<NodeState>(), OnNodeStatesChanged));

        // Using a DependencyProperty as the backing store for UpdateNodeCommand.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty UpdateNodeCommandProperty =
        //    DependencyProperty.Register("UpdateNodeCommand", typeof(ICommand), typeof(UiController), new PropertyMetadata(null));

        public ControlUiViewModel ViewModel => DataContext as ControlUiViewModel;
        public TranslateTransform CenteringT = new();

        public ScaleTransform FlipYT = new(1, -1);
        public NodeShape SelectedNode { get; set; }

        private Grid _grid;

        private Point _moveStartMousePosition;

        private Point _moveStartTargetPostion;

        private NodeShape _moveTarget;

        private Point _originalContentOffset;

        // Fields to store the start position for panning
        private Point? _panStartPoint = null;

        private TextBlock billboard;

        private Rectangle box;

        private Ellipse circle;

        private UiMode uiMode = UiMode.Default;

        public UiController() {
            Loaded += (_, e) => {
                RegionUi = VisualTreeHelperExtensions.FindParentOfType<RegionControlUI>(this);
                MultiView = VisualTreeHelperExtensions.FindParentOfType<MultiView>(this);

                RegionUi.UiStatusText = UiMode.ToString();
            };

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

        //public ICommand AddNodeCommand {
        //    get { return (ICommand)GetValue(AddNodeCommandProperty); }
        //    set { SetValue(AddNodeCommandProperty, value); }
        //}

        public Canvas Canvas { get; set; }

        //public ICommand DeleteNodeCommand {
        //    get { return (ICommand)GetValue(DeleteNodeCommandProperty); }
        //    set { SetValue(DeleteNodeCommandProperty, value); }
        //}

        public Grid Indicator { get; set; }

        public MultiView MultiView { get; set; }

        public List<NodeShape> NodeShapes { get; set; } = new List<NodeShape>();

        public IEnumerable<NodeState> NodeStates {
            get { return (IEnumerable<NodeState>)GetValue(NodeStatesProperty); }
            set { SetValue(NodeStatesProperty, value); }
        }
        public RegionControlUI RegionUi { get; set; }

        public ScaleTransform ScaleT { get; set; } = new ScaleTransform();

        public List<SimplexShape> Simplices { get; set; } = new List<SimplexShape>();

        public UiMode UiMode {
            get => uiMode;
            set {
                uiMode = value;

                ChangeUiMode(UiMode);
            }
        }

        public ControlUiViewModel UiState => (ControlUiViewModel)DataContext;

        //public ICommand UpdateNodeCommand {
        //    get { return (ICommand)GetValue(UpdateNodeCommandProperty); }
        //    set { SetValue(UpdateNodeCommandProperty, value); }
        //}

        public ScrollViewer Viewer { get; set; }

        public List<VoronoiShape> Voronois { get; set; } = new List<VoronoiShape>();

        public static DependencyProperty RegionStatesProperty => regionStatesProperty;

        public void ClearRegions() {
            foreach (var region in Simplices) {
                Canvas.Children.Remove(region);
            }
            foreach (var region in Voronois) {
                Canvas.Children.Remove(region);
            }

            Simplices.Clear();
            Voronois.Clear();
        }

        public void Invalidate() {
            InvalidateNodeShapes();
            InvalidateRegion();
        }

        public void InvalidateNodeShapes() {
            // Clear all new nodes.
            foreach (var node in NodeShapes) {
                Canvas.Children.Remove(node);
            }

            NodeShapes.Clear();

            // Add nodes
            foreach (var nodeState in NodeStates) {
                var nodeShape = new NodeShape(nodeState.Id) {
                    Position = nodeState.Value,
                };

                Canvas.SetZIndex(nodeShape, 20);

                NodeShapes.Add(nodeShape);
                Canvas.Children.Add(nodeShape);
            }
        }

        public void InvalidateRegion() {
            // clear all regions
            ClearRegions();

            // add regions
            foreach (var region in RegionStates) {
                if (region is SimplexState s) {
                    var simplex = new SimplexShape(region.Id, region.Vertices.Select(e => e.Value).ToArray()) {
                        Visibility = Visibility.Hidden
                    };

                    Simplices.Add(simplex);
                    Canvas.Children.Add(simplex);
                }
                else if (region is VoronoiState v) {
                    var voronoi = new VoronoiShape(region.Id, region.Vertices.Select(e => e.Value).ToArray()) {
                        Visibility = Visibility.Hidden

                    };

                    Voronois.Add(voronoi);
                    Canvas.Children.Add(voronoi);
                }
                else
                    throw new InvalidOperationException();
            }
        }

        private void ShowRegions(bool v) {
            if (v) {
                foreach (var item in Simplices) {
                    item.Visibility = Visibility.Visible;
                }

                foreach (var item in Voronois) {
                    item.Visibility = Visibility.Visible;
                }
            }
            else {
                foreach (var item in Simplices) {
                    item.Visibility = Visibility.Hidden;
                }

                foreach (var item in Voronois) {
                    item.Visibility = Visibility.Hidden;
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            CenteringT = new TranslateTransform(sizeInfo.NewSize.Width / 2, sizeInfo.NewSize.Height / 2);
            FlipYT.CenterX = sizeInfo.NewSize.Width / 2;
            FlipYT.CenterY = sizeInfo.NewSize.Height / 2;

            InvalidateTransform();
            InvaldatePosition();

            base.OnRenderSizeChanged(sizeInfo);
        }

        private static void OnNodeStatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = d as UiController;

            if (e.NewValue == null)
                control.NodeStates = Array.Empty<NodeState>();

            // handle DependencyPropertyChangedEventArgs
            control.InvalidateNodeShapes();
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
                case UiMode.Control:
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

        private void Canvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            switch (UiMode) {
                case UiMode.Default:
                    break;
                case UiMode.Add:
                    break;
                case UiMode.Remove:
                    PerformDelete(sender, e);
                    break;
                case UiMode.Move:
                    EndMove(sender, e);
                    break;
                case UiMode.Assign:
                    PerformSelect(sender, e);
                    break;
                case UiMode.Build:
                    break;
                case UiMode.Control:
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

        private void Canvas_PreviewMouseMove(object sender, MouseEventArgs e) {
            switch (UiMode) {
                case UiMode.Default:

                    break;
                case UiMode.Add:
                    MovePointer(sender, e);
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
                case UiMode.Control:
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
                case UiMode.Control:
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

        private void ChangeUiMode(UiMode mode) {
            switch (mode) {
                case UiMode.Default:
                    ShowPointer(false);
                    ShowRegions(true);
                    break;
                case UiMode.Add:
                    ShowPointer(true);
                    ShowRegions(false);
                    break;
                case UiMode.Remove:
                    ShowRegions(false);
                    break;
                case UiMode.Move:
                    ShowRegions(false);
                    break;
                case UiMode.Assign:
                    ShowRegions(false);
                    break;
                case UiMode.Build:
                    break;
                case UiMode.Control:
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

        private void ShowPointer(bool v) {
            if (v) {
                Indicator.Visibility = Visibility.Visible;
            }
            else {
                Indicator.Visibility = Visibility.Hidden;
            }
        }

        private void EndMove(object sender, MouseButtonEventArgs e) {
            if (_moveTarget != null) {
                var id = _moveTarget.NodeId;
                var position = _moveTarget.Position;

                // Command: Update node
                _moveTarget.Position = position;
                ViewModel.UpdateNode(_moveTarget.State);

                _moveTarget = null;
            }
        }

        private void EndControl(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);
            TestHitRegion(currentPosition);
        }

        private void EndPan(object sender, MouseButtonEventArgs e) {
            ReleaseMouseCapture();
            _panStartPoint = null;
            Cursor = Cursors.Arrow;
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

        private void InvaldatePosition() {

            NodeShapes.ForEach(e => SetPosition(e, e.Position.X, e.Position.Y));
            SetPosition(circle, 0, 0);
            //SetPosition(box, 0, 0);
            //SetPosition(billboard, 0, 100);
        }

        private void InvalidateTransform() {
            var g = new TransformGroup();

            g.Children.Add(CenteringT);
            g.Children.Add(FlipYT);
            g.Children.Add(ScaleT);

            Canvas.RenderTransform = g;
        }

        private void PerformAdd(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);

            ViewModel.AddNode(currentPosition);
        }

        private void PerformMove(object sender, MouseEventArgs e) {
            if (_moveTarget != null) {
                var currentPosition = e.GetPosition(Canvas);
                var offset = currentPosition - _moveStartMousePosition;

                _moveTarget.Position = _moveStartTargetPostion + offset;
            }
        }

        private void PerformControl(object sender, MouseEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);
            TestHitRegion(currentPosition);

            //ViewModel.Input = currentPosition;

            //var result = VisualTreeHelper.HitTest(Canvas, currentPosition);
            //if (result != null) {
            //    var parent = VisualTreeHelperExtensions.FindParentOfType<SimplexShape>(result.VisualHit);

            //    if (parent != null) {
            //        // Command: Control node
            //        ViewModel.HitRegion = parent.State;
            //        //ViewModel.ControlNode(parent.State);
            //    }
            //}
        }

        private void TestHitRegion(Point p) {
            ViewModel.Input = p;

            var result = VisualTreeHelper.HitTest(Canvas, p);
            if (result != null) {
                var parent = VisualTreeHelperExtensions.FindParentOfType<SimplexShape>(result.VisualHit);

                if (parent != null) {
                    // Command: Control node
                    ViewModel.HitRegion = parent.State;
                    //ViewModel.ControlNode(parent.State);
                }
            }
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

        private void PerformDelete(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);

            var result = VisualTreeHelper.HitTest(Canvas, currentPosition);

            if (result != null) {
                var parent = VisualTreeHelperExtensions.FindParentOfType<NodeShape>(result.VisualHit);

                if (parent != null) {
                    // Command: Remove node
                    ViewModel.DeleteNode(parent.State);
                }
            }
        }

        private void PerformSelect(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);

            var result = VisualTreeHelper.HitTest(Canvas, currentPosition);

            if (result != null) {
                var parent = VisualTreeHelperExtensions.FindParentOfType<NodeShape>(result.VisualHit);

                if (parent != null) {
                    // Command: Remove node
                    if (SelectedNode == parent) {
                        SelectedNode = null;
                        parent.Select(false);
                    }
                    else {
                        foreach (var nodeShape in NodeShapes) {
                            nodeShape.Select(false);
                        }

                        SelectedNode = parent;
                        parent.Select(true);

                        WeakReferenceMessenger.Default.Send(new UiControllerSelectedMessage() { Sender = this });

                        //MultiView.RequestMotorDialog();
                    }
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

        private void MovePointer(object sender, MouseEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);

            SetPosition(Indicator, currentPosition.X, currentPosition.Y);
        }

        private void StartMove(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);
            var result = VisualTreeHelper.HitTest(Canvas, currentPosition);

            if (result != null) {
                var parent = VisualTreeHelperExtensions.FindParentOfType<NodeShape>(result.VisualHit);

                if (parent != null) {
                    _moveTarget = parent;
                    _moveStartMousePosition = currentPosition;
                    _moveStartTargetPostion = parent.Position;
                }
            }
        }

        private void StartControl(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);
            TestHitRegion(currentPosition);
        }

        private void StartPan(object sender, MouseButtonEventArgs e) {
            _panStartPoint = e.GetPosition(Canvas);
            _originalContentOffset = new Point(ScaleT.ScaleX, ScaleT.ScaleY);
            CaptureMouse(); // Ensure we get the MouseMove even if the cursor goes out of the Canvas
            Cursor = Cursors.Hand;
        }
    }

    public class UiControllerSelectedMessage {
        public object Sender { get; init; }
    }
}