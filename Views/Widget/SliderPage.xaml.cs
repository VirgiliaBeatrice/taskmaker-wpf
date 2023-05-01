using NLog;
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
        private Logger logger = LogManager.GetCurrentClassLogger();

        private int _finger = -1;


        public bool IsVertical {
            get { return (bool)GetValue(IsVerticalProperty); }
            set { SetValue(IsVerticalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsVertical.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsVerticalProperty =
            DependencyProperty.Register("IsVertical", typeof(bool), typeof(SliderController), new PropertyMetadata(false));




        public RegionMotorViewModel ViewModel {
            get { return (RegionMotorViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(RegionMotorViewModel), typeof(SliderController), new PropertyMetadata(null));


        public MotorState State {
            get { return (MotorState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for State.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(MotorState), typeof(SliderController), new PropertyMetadata(null, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is SliderController controller) {
                controller.InvalidateMotorState();
            }
        }

        private void InvalidateMotorState() {
            SetVerticalLayouts();
        }


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


            TextBlock min, max;
            Slider slider;

            if (State != null) {
                min = new TextBlock() {
                    Foreground = Brushes.Black,
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                min.SetBinding(TextBlock.TextProperty, new Binding("Min") {
                    Source = State
                });

                max = new TextBlock() {
                    Foreground = Brushes.Black,
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                max.SetBinding(TextBlock.TextProperty, new Binding("Max") {
                    Source = State
                });

                slider = new Slider() {
                    MinWidth = 400,
                    MinHeight = 50,
                };

                slider.SetBinding(Slider.ValueProperty, new Binding("Value[0]") {
                    Source = State
                });
                slider.SetBinding(Slider.MaximumProperty, new Binding("Max") {
                    Source = State
                });
                slider.SetBinding(Slider.MinimumProperty, new Binding("Min") {
                    Source = State
                });
            }
            else {
                min = new TextBlock() {
                    Foreground = Brushes.Black,
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = "0",
                };
                max = new TextBlock() {
                    Foreground = Brushes.Black,
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = "100",
                };
                slider = new Slider() {
                    Minimum = 0,
                    Maximum = 100,
                    Value = 50,
                    MinWidth = 400,
                    MinHeight = 50,
                };
            }

            var box = new Viewbox() {
            };

            box.Child = slider;

            grid.Children.Add(min);
            grid.Children.Add(box);
            grid.Children.Add(max);

            Grid.SetColumn(min, 0);
            Grid.SetColumn(box, 1);
            Grid.SetColumn(max, 2);

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


            TextBlock label, min, max;
            Slider slider;

            if (State != null) {
                label = new TextBlock {
                    Foreground = Brushes.White,
                    FontSize = 18,
                    Margin = new Thickness(2),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                label.SetBinding(TextBlock.TextProperty, new Binding("Name") {
                    Source = State
                });
                label.SetBinding(TextBlock.BackgroundProperty, new Binding("Color") {
                    Source = State
                });

                min = new TextBlock() {
                    Foreground = Brushes.Black,
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                min.SetBinding(TextBlock.TextProperty, new Binding("Min") {
                    Source = State
                });

                max = new TextBlock() {
                    Foreground = Brushes.Black,
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                max.SetBinding(TextBlock.TextProperty, new Binding("Max") {
                    Source = State
                });

                slider = new Slider() {
                    Height = 200,
                    Orientation = Orientation.Vertical,
                    //IsDirectionReversed = true,
                };

                slider.SetBinding(Slider.ValueProperty, new Binding("Value[0]") {
                    Source = State
                });
                slider.SetBinding(Slider.MaximumProperty, new Binding("Max") {
                    Source = State
                });
                slider.SetBinding(Slider.MinimumProperty, new Binding("Min") {
                    Source = State
                });

                slider.ValueChanged += Slider_ValueChanged;
            }
            else {
                label = new TextBlock {
                    Text = "Motor0",
                    Foreground = Brushes.Black,
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                min = new TextBlock() {
                    Text = "0",
                    Foreground = Brushes.Black,
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                max = new TextBlock() {
                    Text = "100",
                    Foreground = Brushes.Black,
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                slider = new Slider() {
                    Minimum = 0,
                    Maximum = 100,
                    Value = 50,
                    Height = 200,
                    //VerticalAlignment = VerticalAlignment.Stretch,
                    Orientation = Orientation.Vertical,
                    //Background = Brushes.Red,
                    Margin = new Thickness(0, 0, 0, 0),
                    //IsDirectionReversed = true,
                };
            }

            var border = new Border {
                //Background = Brushes.Green,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1)
            };

            border.TouchDown += Box_TouchDown;
            border.TouchMove += Box_TouchMove;
            border.TouchUp += Box_TouchUp;

            var box = new Viewbox() {
                StretchDirection = StretchDirection.UpOnly,
                Stretch = Stretch.Uniform,
                Width = 80
            };

            border.Child = box;
            box.Child = slider;

            grid.Children.Add(label);
            grid.Children.Add(min);
            grid.Children.Add(border);
            grid.Children.Add(max);

            Grid.SetRow(label, 0);
            Grid.SetRow(max, 1);
            Grid.SetRow(border, 2);
            Grid.SetRow(min, 3);

            Content = grid;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var slider = sender as Slider;
            var vm = ViewModel;

            var state = slider.DataContext as MotorState;

            vm.UpdateMotorValue(state, e.NewValue);
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
                    var value = Math.Clamp(1.0 - touchPoint.Position.Y / box.ActualHeight, 0.0, 1.0);
                    var slider = (box.Child as Viewbox).Child as Slider;

                    //slider.Value = 
                    var motorValue = value * (slider.Maximum - slider.Minimum) + slider.Minimum;
                    
                    ViewModel.UpdateMotorValue(State, motorValue);

                    //logger.Debug("Percentage: {0}; Value: {1}", value.ToString("P"), slider.Value.ToString("F2"));
                }
                else {
                    //var touchPoint = e.GetTouchPoint(box);
                    //var value = touchPoint.Position.X / box.ActualWidth;
                    //var slider = box.Child as Slider;

                    //slider.Value = value * (slider.Maximum - slider.Minimum) + slider.Minimum;
                }


                e.Handled = true;
            }
        }

        private void Box_TouchDown(object sender, TouchEventArgs e) {
            _finger = e.TouchDevice.Id;

            (sender as Border).CaptureTouch(e.TouchDevice);

            var box = sender as Viewbox;
        }
    }

    /// <summary>
    /// Interaction logic for SliderPage.xaml
    /// </summary>
    public partial class SliderPage : UserControl {
        public SliderPage() {
            InitializeComponent();

            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            var vm = DataContext as RegionMotorViewModel;

            if (e.Key == Key.F5) {
                vm.InvalidateMotorState();
            }
        }

        private void page_Loaded(object sender, RoutedEventArgs e) {
            var vm = DataContext as RegionMotorViewModel;
            
            if (vm.MotorStates.Length == 0) {
                vm.AddMotor();
                vm.AddMotor();
                vm.AddMotor();

                var idx = 0;

                foreach(var motor in vm.MotorStates) {
                    motor.NuibotBoardId = 0;
                    motor.NuibotMotorId = idx;

                    motor.Max = 10000;
                    motor.Min = -10000;
                    idx++;

                    vm.UpdateMotor(motor);
                }
            }

        }
    }
}
