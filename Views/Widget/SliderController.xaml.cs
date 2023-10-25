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
        public SliderController()
        {
            InitializeComponent();
        }

        private void Button_Click_Increase(object sender, RoutedEventArgs e) {
            slider.Value++;
        }

        private void Button_Click_Decrease(object sender, RoutedEventArgs e) {
            slider.Value--;
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e) {

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            slider.Value = 0;
        }

        private void Border_TouchMove(object sender, TouchEventArgs e) {
            var touchPt = e.GetTouchPoint(slider);

            var percent = 1- (touchPt.Position.Y / slider.ActualHeight);
            var value = slider.Minimum + percent * (slider.Maximum - slider.Minimum);

            slider.Value = value;
            e.Handled = true;
        }
    }
}
