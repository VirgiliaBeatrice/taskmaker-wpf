using CommunityToolkit.Mvvm.Messaging;
using NLog;
using SharpVectors.Converters;
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
        public SvgViewbox CheckIcon { get; set; }
        public Color PrimaryColor;
        public UiElementState _state = UiElementState.Default;
        public Point Position {
            get => position;
            set {
                position = value;

                SetPosition();
            }
        }

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public UiController Ui { get; set; }

        public NodeViewModel State => DataContext as NodeViewModel;

        public NodeShape() {
            //PrimaryColor = ColorManager.GetTintedColor(ColorManager.Palette[0], 2);
            PrimaryColor = M3ColorManager.GetColor("primary");

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

            CheckIcon = new SvgViewbox() {
                Width = 24,
                Height = 24,
                Source = new Uri("pack://application:,,,/icons/check.svg"),
                Visibility = Visibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            
            (Container as Grid).Children.Add(shape);
            (Container as Grid).Children.Add(StateLayer);
            (Container as Grid).Children.Add(CheckIcon);


            Content = Container;



            Loaded += (_, _) => {
                Ui = VisualTreeHelperExtensions.FindParentOfType<UiController>(this);
            };

            RenderTransform = new ScaleTransform(1, -1);
            //InitializeComponents();
            //Invalidate();
        }

        private void UpdateInfo() {
            var sb = new StringBuilder();

            sb.AppendLine("Id:");
            sb.AppendLine($"  {NodeId}");
            sb.AppendLine($"Position:");
            sb.AppendLine($"  {Position}");

            Ui.RegionUi.infoPanel.Info = sb.ToString();
            //Ui.RegionUi.infoPanel.Visibility = Visibility.Visible;
        }

        public bool IsSelected { get; set; } = false;

        public int NodeId => (State == null) ? -1 : State.Id;

        public void SetPosition() {
            double halfWidth = 40 / 2;
            double halfHeight = 40 / 2;

            Canvas.SetLeft(this, Position.X - halfWidth);
            Canvas.SetTop(this, Position.Y + halfHeight);
        }

        public void Select(bool flag) {
            if (flag) {
                CheckIcon.Visibility = Visibility.Visible;
            }
            else {
                CheckIcon.Visibility = Visibility.Hidden;

            }
        }

        public void GoToState(UiElementState state) {
            _state = state;

            switch (state) {
                case UiElementState.Default:
                    StateLayer.Visibility = Visibility.Hidden;
                    //CheckIcon.Visibility = Visibility.Hidden;
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
                    //HandleActivated();
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
            //if (Ui.UiMode == UiMode.Assign) {
            //    // send dialog message to window
            //    var msg = new ShowDialogMessage();

            //    WeakReferenceMessenger.Default.Register<AssignActuationValuesMessage>(this, (r, m) => {
            //        ActuationValue = m.Values;

            //        WeakReferenceMessenger.Default.Unregister<AssignActuationValuesMessage>(r);
            //    });

            //    WeakReferenceMessenger.Default.Send(msg);
            //}
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