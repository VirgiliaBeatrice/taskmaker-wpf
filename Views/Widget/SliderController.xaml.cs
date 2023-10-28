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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace taskmaker_wpf.Views.Widget
{
    /// <summary>
    /// Interaction logic for SliderController.xaml
    /// </summary>
    public partial class SliderController : UserControl
    {
        private TouchDevice _touchDevice;
        private StylusDevice _stylusDevice;

        public SliderController()
        {
            InitializeComponent();
        }

        private void Button_Click_Increase(object sender, RoutedEventArgs e) {
            slider.Value += slider.LargeChange;
        }

        private void Button_Click_Decrease(object sender, RoutedEventArgs e) {
            slider.Value -= slider.LargeChange;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            slider.Value = 0;
        }

        private void Border_TouchMove(object sender, TouchEventArgs e) {
            if (_touchDevice != null) {
                var touchPt = e.GetTouchPoint(slider);

                slider.Value = CalculateValue(touchPt.Position);

                e.Handled = true;
            }
        }


        private void Border_StylusDown(object sender, StylusDownEventArgs e) {
            var b = sender as Border;

            // handle stylusDown event
            b.CaptureStylus();
            _stylusDevice = e.StylusDevice;
        }


        private void Border_StylusMove(object sender, StylusEventArgs e) {
            if (_stylusDevice != null) {
                var point = e.GetPosition(sender as Border);

                slider.Value = CalculateValue(point);

                e.Handled = true;
            }
        }

        private void Border_StylusUp(object sender, StylusEventArgs e) {
            var b = sender as Border;

            if (_stylusDevice != null) {
                b.ReleaseStylusCapture();
                _stylusDevice = null;
            }
        }

        private void Border_TouchDown(object sender, TouchEventArgs e) {
            var b = sender as Border;

            b.CaptureTouch(e.TouchDevice);
            _touchDevice = e.TouchDevice;
        }

        private void Border_TouchUp(object sender, TouchEventArgs e) {
            var b = sender as Border;

            if (_touchDevice != null) {
                b.ReleaseTouchCapture(_touchDevice);
                _touchDevice = null;
            }
        }


        private double CalculateValue(Point point) {
            var percent = 1 - (point.Y / slider.ActualHeight);
            var value = slider.Minimum + percent * (slider.Maximum - slider.Minimum);

            return value;
        }
    }
}
