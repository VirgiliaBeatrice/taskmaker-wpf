using CommunityToolkit.Mvvm.Messaging;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using taskmaker_wpf.ViewModels;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace taskmaker_wpf.Views.Widget {
    public class UiController : UserControl {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private Border _container;
        private bool _isControlStarted = false;
        private Point _moveStartMousePosition;
        private Point _moveStartTargetPostion;
        private NodeShape _moveTarget;
        private Point _originalContentOffset;
        // Fields to store the start position for panning
        private Point? _panStartPoint = null;

        //private TextBlock billboard;
        //private Rectangle box;
        //private Ellipse circle;
        private UiMode uiMode = UiMode.Default;
        public TranslateTransform CenteringT = new();
        public ScaleTransform FlipYT = new(1, -1);
        public Canvas Canvas { get; set; }
        public List<NodeShape> NodeShapes { get; set; } = new();
        public IEnumerable<NodeViewModel> NodeVMS => UiViewModel?.NodeStates;
        public IEnumerable<BaseRegionState> RegionStates => UiViewModel?.RegionStates;
        public ScaleTransform ScaleT { get; set; } = new();
        public NodeShape SelectedNode { get; set; }
        public List<SimplexShape> SimpliceShapes { get; set; } = new();
        public UiMode UiMode {
            get => uiMode;
            set {
                uiMode = value;

                ChangeUiMode(UiMode);
            }
        }

        public ControlUiViewModel UiViewModel => DataContext as ControlUiViewModel;
        public List<VoronoiShape> VoronoiShapes { get; set; } = new();

        public UiController() {
            //IsManipulationEnabled = true;
            // UserControl
            Background = new SolidColorBrush(Colors.Transparent);

            // Grid container
            _container = new Border() {
                IsManipulationEnabled = true,
                CornerRadius = new CornerRadius(16),
                Background = new SolidColorBrush(Colors.Transparent),
                //BorderBrush = new SolidColorBrush(M3ColorManager.GetColor("surface")),
            };

            // add a circle sign at origin
            //circle = new Ellipse() {
            //    Width = 50,
            //    Height = 50,
            //    Stroke = Brushes.Gray,
            //    Visibility = Visibility.Hidden,
            //    StrokeThickness = 1,
            //};

            //box = new Rectangle {
            //    Width = 100,
            //    Height = 200,
            //    Fill = Brushes.AliceBlue,
            //    Visibility = Visibility.Hidden,
            //    Opacity = 0.5
            //};
            //billboard = new TextBlock {
            //    Text = "Y+",
            //    Visibility = Visibility.Hidden
            //};

            // Canvas to hold shapes
            Canvas = new Canvas {
                //Background = new SolidColorBrush(M3ColorManager.GetColor("onSurface")),
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

            //Canvas.Children.Add(circle);
            //Canvas.Children.Add(box);
            //Canvas.Children.Add(billboard);

            _container.Child = Canvas;
            Content = _container;

            // Register event handlers
            MouseWheel += UiController_MouseWheel;
            MouseLeftButtonDown += UiController_MouseLeftButtonDown;
            MouseLeftButtonUp += UiController_MouseLeftButtonUp;
            MouseMove += UiController_MouseMove;

            DataContextChanged += UiController_DataContextChanged;

            //_container.IsManipulationEnabled = true;
            ManipulationStarting += UiController_ManipulationStarting;
            ManipulationDelta += UiController_ManipulationDelta;

        }

        // https://www.andrewhoefling.com/Blog/Post/wpf-multitouch-gestures-translation-scale-and-rotate
        private void UiController_ManipulationDelta(object sender, ManipulationDeltaEventArgs e) {
            Point center = new Point(
                RenderSize.Width / 2.0,
                RenderSize.Height / 2.0);

            ScaleT.CenterX = center.X;
            ScaleT.CenterY = center.Y;

            // DeltaManipulation stores the delta data for us so it allows us to
            // focus on the scale gesture
            ScaleT.ScaleX *= e.DeltaManipulation.Scale.X;
            ScaleT.ScaleY *= e.DeltaManipulation.Scale.Y;

            // rotation code
            //rotation.CenterX = center.X;
            //rotation.CenterY = center.Y;
            //rotation.Angle += e.DeltaManipulation.Rotation;

            // translation code
            CenteringT.X += e.DeltaManipulation.Translation.X * FlipYT.ScaleX;
            CenteringT.Y += e.DeltaManipulation.Translation.Y * FlipYT.ScaleY;

            e.Handled = true;
        }

        private void UiController_ManipulationStarting(object sender, ManipulationStartingEventArgs e) {
            e.ManipulationContainer = _container;
            e.Handled = true;
        }

        private void UiController_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            switch (UiMode) {
                case UiMode.Default:
                    break;
                case UiMode.Add:
                    PerformAdd(sender, e);
                    e.Handled = true;
                    break;
                case UiMode.Remove:
                    break;
                case UiMode.Move:
                    StartMove(sender, e);
                    e.Handled = true;
                    break;
                case UiMode.Assign:
                    break;
                case UiMode.Build:
                    break;
                case UiMode.Control:
                    StartControl(sender, e);
                    e.Handled = true;
                    break;
                case UiMode.Drag:
                    break;
                case UiMode.Pan:
                    StartPan(sender, e);
                    e.Handled = true;
                    break;
                case UiMode.Zoom:
                    break;
                default:
                    break;
            }
        }

        private void UiController_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
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
                    EndControl(sender, e);
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

        private void UiController_MouseMove(object sender, MouseEventArgs e) {
            switch (UiMode) {
                case UiMode.Default:

                    break;
                case UiMode.Add:
                    //MovePointer(sender, e);
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
                    PerformControl(sender, e);
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

        private void UiController_MouseWheel(object sender, MouseWheelEventArgs e) {
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
            // default actions
            EnableNodeHitTest(true);
            EnableManipulation(true);
            ShowPointer(false);
            ShowRegions(true);

            switch (mode) {
                case UiMode.Default:
                    break;
                case UiMode.Add:
                    EnableManipulation(false);
                    ShowPointer(true);
                    ShowRegions(false);
                    break;
                case UiMode.Remove:
                    EnableManipulation(false);
                    ShowRegions(false);
                    break;
                case UiMode.Move:
                    EnableManipulation(false);
                    ShowRegions(false);
                    break;
                case UiMode.Assign:
                    EnableManipulation(false);
                    ShowRegions(false);
                    break;
                case UiMode.Build:
                    break;
                case UiMode.Control:
                    EnableManipulation(false);
                    EnableNodeHitTest(false);
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
        }

        private void EnableManipulation(bool v) {
            if (v) {
                _container.IsManipulationEnabled = true;
            }
            else {
                _container.IsManipulationEnabled = false;

            }
        }

        private void EnableNodeHitTest(bool v) {
            foreach (var node in NodeShapes) {
                node.IsHitTestVisible = v;
            }
        }

        private void EndControl(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);
            if (_isControlStarted && e.ChangedButton == MouseButton.Left) {
                _isControlStarted = false;
                //TestHitRegion(currentPosition);
            }
            else if (e.ChangedButton == MouseButton.Right) {

            }
        }

        private void EndMove(object sender, MouseButtonEventArgs e) {
            if (_moveTarget != null) {
                var id = _moveTarget.NodeId;
                var position = _moveTarget.Position;

                // Command: Update node
                _moveTarget.Position = position;
                _moveTarget.State.Value = position;
                //ViewModel.UpdateNode(_moveTarget.State);

                _moveTarget = null;
            }
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

            NodeShapes.ForEach(e => e.SetPosition());
            //SetPosition(circle, 0, 0);
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

            UiViewModel?.AddNode(currentPosition);
        }

        private void PerformControl(object sender, MouseEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);
            if (_isControlStarted && e.LeftButton == MouseButtonState.Pressed) {

                TestHitRegion(currentPosition);

                WeakReferenceMessenger.Default.Send(new UiControllerControlledMessage {
                    Sender = this
                });
            }
        }

        private void PerformDelete(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);

            var result = VisualTreeHelper.HitTest(Canvas, currentPosition);

            if (result != null) {
                var parent = VisualTreeHelperExtensions.FindParentOfType<NodeShape>(result.VisualHit);

                if (parent != null) {
                    // Command: Remove node
                    UiViewModel.DeleteNode(parent.State);
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

        private void PerformSelect(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);

            var result = VisualTreeHelper.HitTest(Canvas, currentPosition);

            if (result != null) {
                var parent = VisualTreeHelperExtensions.FindParentOfType<NodeShape>(result.VisualHit);

                if (parent != null) {
                    SelectNode(parent);
                }
                else {
                    UnselectAll();
                }
            }
            else {
                UnselectAll();
            }
        }

        private void SelectNode(NodeShape node) {
            UnselectAll();

            SelectedNode = node;
            node.Select(true);

            WeakReferenceMessenger.Default.Send(new UiSelectedNodeChangedMessage() { Sender = this, Node = SelectedNode });
        }

        private void UnselectAll() {
            foreach (var nodeShape in NodeShapes) {
                nodeShape.Select(false);
            }

            SelectedNode = null;
            WeakReferenceMessenger.Default.Send(new UiSelectedNodeChangedMessage() { Sender = this, Node = null });
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

        private void ShowPointer(bool v) {
            if (v) {
                Cursor = Cursors.Cross;
            }
            else {
                Cursor = Cursors.Arrow;
            }
        }

        private void ShowRegions(bool v) {
            if (v) {
                foreach (var item in SimpliceShapes) {
                    item.Visibility = Visibility.Visible;
                }

                foreach (var item in VoronoiShapes) {
                    item.Visibility = Visibility.Visible;
                }
            }
            else {
                foreach (var item in SimpliceShapes) {
                    item.Visibility = Visibility.Hidden;
                }

                foreach (var item in VoronoiShapes) {
                    item.Visibility = Visibility.Hidden;
                }
            }
        }

        private void StartControl(object sender, MouseButtonEventArgs e) {
            var currentPosition = e.GetPosition(Canvas);
            // if left button, then start control


            if (e.ChangedButton == MouseButton.Left) {
                _isControlStarted = true;
            }
            else if (!_isControlStarted && e.ChangedButton == MouseButton.Right) {
                var result = VisualTreeHelper.HitTest(Canvas, currentPosition);

                if (result != null) {
                    var parent = VisualTreeHelperExtensions.FindParentOfType<NodeShape>(result.VisualHit);

                    if (parent != null) {
                        UiViewModel.Input = parent.Position;

                        WeakReferenceMessenger.Default.Send(new UiControllerControlledMessage {
                            Sender = this
                        });
                    }
                }
            }
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

        private void StartPan(object sender, MouseButtonEventArgs e) {
            _panStartPoint = e.GetPosition(Canvas);
            _originalContentOffset = new Point(ScaleT.ScaleX, ScaleT.ScaleY);
            CaptureMouse(); // Ensure we get the MouseMove even if the cursor goes out of the Canvas
            Cursor = Cursors.Hand;
        }

        private void TestHitRegion(Point p) {
            UiViewModel.Input = p;
            UiViewModel.HitRegion = null;

            var result = VisualTreeHelper.HitTest(Canvas, p);
            if (result != null) {
                var parent = VisualTreeHelperExtensions.FindParentOfType<SimplexShape>(result.VisualHit);

                if (parent != null) {
                    // Command: Control node
                    UiViewModel.HitRegion = parent.DataContext as SimplexState;
                    //ViewModel.ControlNode(parent.State);
                }
            }
        }

        private void UiController_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != null && e.NewValue is ControlUiViewModel newVM) {
                newVM.PropertyChanged += ViewModel_PropertyChanged;

                newVM.NodeStates.CollectionChanged += (s, e) => {
                    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                        InvalidateNodeShapes();
                    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                        InvalidateNodeShapes();
                };

                newVM.Fetch();
            }

            if (e.OldValue != null && e.OldValue is ControlUiViewModel oldVM) {
                oldVM.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var vm = sender as ControlUiViewModel;

            if (e.PropertyName == "RegionStates") {
                InvalidateRegion();
            }
            else if (e.PropertyName == nameof(ControlUiViewModel.Mode)) {
                UiMode = vm.Mode;
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

        public void ClearRegions() {
            foreach (var region in SimpliceShapes) {
                Canvas.Children.Remove(region);
            }
            foreach (var region in VoronoiShapes) {
                Canvas.Children.Remove(region);
            }

            SimpliceShapes.Clear();
            VoronoiShapes.Clear();
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
            foreach (var node in NodeVMS) {
                var nodeShape = new NodeShape() {
                    Position = node.Value,
                };

                Binding binding;
                binding = new Binding() {
                    Source = node,
                };
                nodeShape.SetBinding(DataContextProperty, binding);

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
                        Visibility = Visibility.Hidden,
                    };

                    Binding binding;
                    binding = new Binding() {
                        Source = region,
                    };
                    simplex.SetBinding(DataContextProperty, binding);

                    SimpliceShapes.Add(simplex);
                    Canvas.Children.Add(simplex);
                }
                else if (region is VoronoiState v) {
                    var voronoi = new VoronoiShape(region.Id, region.Vertices.Select(e => e.Value).ToArray()) {
                        Visibility = Visibility.Hidden

                    };

                    Binding binding;
                    binding = new Binding() {
                        Source = region,
                    };
                    voronoi.SetBinding(DataContextProperty, binding);

                    VoronoiShapes.Add(voronoi);
                    Canvas.Children.Add(voronoi);
                }
                else
                    throw new InvalidOperationException();
            }
        }
    }

    public class UiMessage {
        public object Sender { get; init; }
    }

    public class UiSelectedNodeChangedMessage : UiMessage {
        public NodeShape Node { get; init; }
    }

    public class UiControllerControlledMessage : UiMessage { }
}