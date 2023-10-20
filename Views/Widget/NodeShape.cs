using CommunityToolkit.Mvvm.Messaging;
using NLog;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using taskmaker_wpf.ViewModels;
using Point = System.Windows.Point;

namespace taskmaker_wpf.Views.Widget {
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

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public UiController Ui { get; set; }

        public double[] ActuationValue { get; set; } = Array.Empty<double>();

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

            Loaded += (_, _) => {
                Ui = VisualTreeHelperExtensions.FindParentOfType<UiController>(this);
            };

            // initialize ActuationValue to double[6] and fill with Nan
            ActuationValue = new double[6];

            for (int i = 0; i < ActuationValue.Length; i++) {
                ActuationValue[i] = double.NaN;
            }


            RenderTransform = new ScaleTransform(1, -1);
            //InitializeComponents();
            //Invalidate();
        }

        public NodeState ToState() {
            return new NodeState(NodeId, Position);
        }

        private void UpdateInfo() {
            var sb = new StringBuilder();

            sb.AppendLine("Id:");
            sb.AppendLine($"  {NodeId}");
            sb.AppendLine($"Position:");
            sb.AppendLine($"  {Position}");
            sb.AppendLine("Actuation:");
            sb.AppendLine($"  {string.Join(",", ActuationValue)}");

            Ui.RegionUi.infoPanel.Info = sb.ToString();
            //Ui.RegionUi.infoPanel.Visibility = Visibility.Visible;
        }

        private void HideInfo() {
            Ui.RegionUi.infoPanel.Visibility = Visibility.Collapsed;
        }

        public bool IsSelected { get; set; } = false;

        public int NodeId { get; set; }

        private void SetPosition(double x, double y) {
            double halfWidth = 40 / 2;
            double halfHeight = 40 / 2;

            Canvas.SetLeft(this, x - halfWidth);
            Canvas.SetTop(this, y + halfHeight);
        }

        public void GoToState(UiElementState state) {
            _state = state;

            switch (state) {
                case UiElementState.Default:
                    StateLayer.Visibility = Visibility.Hidden;
                    Effect = null;
                    //HideInfo();
                    break;
                case UiElementState.Hover:
                    HandleHover();
                    UpdateInfo();
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

            // Actions
            if (Ui.UiMode == UiMode.Assign) {
                // send dialog message to window
                var msg = new ShowDialogMessage();

                WeakReferenceMessenger.Default.Register<AssignActuationValuesMessage>(this, (r, m) => {
                    ActuationValue = m.Values;

                    WeakReferenceMessenger.Default.Unregister<AssignActuationValuesMessage>(r);
                });

                WeakReferenceMessenger.Default.Send(msg);
            }
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

        private Point position;
    }
}