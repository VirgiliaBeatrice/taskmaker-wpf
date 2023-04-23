using SharpVectors.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Views.Widget {

    public class SliderController : UserControl {
        private int _finger = -1;


        public bool IsVertical {
            get { return (bool)GetValue(IsVerticalProperty); }
            set { SetValue(IsVerticalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsVertical.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsVerticalProperty =
            DependencyProperty.Register("IsVertical", typeof(bool), typeof(SliderController), new PropertyMetadata(false));



        public SliderController() {
            //SetHorizontalLayouts();
            SetVerticalLayouts();
        }

        public void SetHorizontalLayouts() {
            VerticalAlignment = VerticalAlignment.Stretch;

            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition() {
                Width = new GridLength(1, GridUnitType.Star),
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition() {
                Width = new GridLength(8, GridUnitType.Star),
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition() {
                Width = new GridLength(1, GridUnitType.Star),
            });

            var min = new TextBlock() {
                Text = "0",
                Foreground = Brushes.Black,
                FontSize = 18,
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Grid.SetColumn(min, 0);

            var max = new TextBlock() {
                Text = "100",
                Foreground = Brushes.Black,
                FontSize = 18,
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Grid.SetColumn(max, 2);


            var slider = new Slider() {
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                MinWidth = 400,
                MinHeight = 50,
            };
            var box = new Viewbox() {
            };

            box.Child = slider;

            Grid.SetColumn(box, 1);

            grid.Children.Add(min);
            grid.Children.Add(box);
            grid.Children.Add(max);

            Content = grid;
        }

        public void SetVerticalLayouts() {
            HorizontalAlignment = HorizontalAlignment.Center;
            MinHeight = 400;
            MaxWidth = 80;

            IsVertical = true;


            var grid = new Grid();

            grid.RowDefinitions.Add(new RowDefinition() {
                Height = new GridLength(0.5, GridUnitType.Star),
            });
            grid.RowDefinitions.Add(new RowDefinition() {
                Height = new GridLength(1, GridUnitType.Star),
            });
            grid.RowDefinitions.Add(new RowDefinition() {
                Height = new GridLength(8, GridUnitType.Star),
            });
            grid.RowDefinitions.Add(new RowDefinition() {
                Height = new GridLength(1, GridUnitType.Star),
            });

            var label = new TextBlock {
                Text = "Motor0",
                Foreground = Brushes.Black,
                FontSize = 18,
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Grid.SetRow(label, 0);

            var min = new TextBlock() {
                Text = "0",
                Foreground = Brushes.Black,
                FontSize = 18,
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Grid.SetRow(min, 3);

            var max = new TextBlock() {
                Text = "100",
                Foreground = Brushes.Black,
                FontSize = 18,
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Grid.SetRow(max, 1);


            var slider = new Slider() {
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                Height = 200,
                //VerticalAlignment = VerticalAlignment.Stretch,
                Orientation = Orientation.Vertical,
                //Background = Brushes.Red,
                Margin = new Thickness(0, 0, 0, 0),
                IsDirectionReversed = true,
            };

            //slider.TouchDown += Slider_TouchDown;
            //slider.TouchMove += Slider_TouchMove;
            //slider.TouchUp += Slider_TouchUp;

            var border = new Border {
                //Background = Brushes.Green,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1)
            };
            var box = new Viewbox() {
                StretchDirection = StretchDirection.UpOnly,
                Stretch = Stretch.Uniform,
                Width = 80
            };
            border.Child = box;
            box.Child = slider;

            border.TouchDown += Box_TouchDown;
            border.TouchMove += Box_TouchMove;
            border.TouchUp += Box_TouchUp;

            Grid.SetRow(border, 2);

            grid.Children.Add(label);
            grid.Children.Add(min);
            grid.Children.Add(border);
            grid.Children.Add(max);

            Content = grid;
        }

        private void Box_TouchUp(object sender, TouchEventArgs e) {
            if (_finger != -1) {
                (sender as Border).ReleaseTouchCapture(e.TouchDevice);
                _finger = -1;
                //e.Handled = true;
            }
        }

        private void Box_TouchMove(object sender, TouchEventArgs e) {
            var box = sender as Border;

            if (_finger != -1) {
                if (IsVertical) {
                    var touchPoint = e.GetTouchPoint(box);
                    var value = touchPoint.Position.Y / box.ActualHeight;
                    var slider = (box.Child as Viewbox).Child as Slider;

                    slider.Value = value * (slider.Maximum - slider.Minimum) + slider.Minimum;
                }
                else {
                    var touchPoint = e.GetTouchPoint(box);
                    var value = touchPoint.Position.X / box.ActualWidth;
                    var slider = box.Child as Slider;

                    slider.Value = value * (slider.Maximum - slider.Minimum) + slider.Minimum;
                }


                //e.Handled = true;
            }
        }

        private void Box_TouchDown(object sender, TouchEventArgs e) {
            _finger = e.TouchDevice.Id;

            (sender as Border).CaptureTouch(e.TouchDevice);

            var box = sender as Viewbox;
        }

        private void Slider_TouchDown(object sender, TouchEventArgs e) {
            var slider = sender as Slider;

            _finger = e.TouchDevice.Id;

            //slider.CaptureTouch(e.TouchDevice);

            //e.Handled = true;
        }

        private void Slider_TouchMove(object sender, TouchEventArgs e) {
            var slider = sender as Slider;

            if (_finger != -1) {
                if (IsVertical) {
                    var touchPoint = e.GetTouchPoint(slider);
                    var value = touchPoint.Position.Y / slider.ActualHeight;
                    slider.Value = value * (slider.Maximum - slider.Minimum) + slider.Minimum;
                }
                else {
                    var touchPoint = e.GetTouchPoint(slider);
                    var value = touchPoint.Position.X / slider.ActualWidth;
                    slider.Value = value * (slider.Maximum - slider.Minimum) + slider.Minimum;
                }


                //e.Handled = true;
            }
        }

        private void Slider_TouchUp(object sender, TouchEventArgs e) {
            var slider = sender as Slider;

            if (_finger != -1) {
                //slider.ReleaseTouchCapture(e.TouchDevice);
                _finger = -1;
                //e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Interaction logic for SliderPage.xaml
    /// </summary>
    public partial class SliderPage : UserControl {
        public SliderPage() {
            InitializeComponent();
        }

        private void page_Loaded(object sender, RoutedEventArgs e) {
            var vm = DataContext as RegionMotorViewModel;

            vm.AddMotor();
            vm.AddMotor();
            vm.AddMotor();

            var idx = 0;

            foreach(var motor in vm.MotorStates) {
                motor.NuibotBoardId = 0;
                motor.NuibotMotorId = 1 + idx;

                motor.Max = 10000;
                motor.Min = -10000;
                idx++;

                vm.UpdateMotor(motor);
            }
        }
    }
}
