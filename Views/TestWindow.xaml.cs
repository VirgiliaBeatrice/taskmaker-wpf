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
using System.Windows.Shapes;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Views {
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window {
        public TestWindow() {
            InitializeComponent();
            UpdateComponents();
        }

        private void UpdateComponents() {
            var controller = new MotorItem();

            var context = ((TestWindowViewModel)DataContext).Motor;

            controller.DataContext = (TestWindowViewModel)DataContext;
            controller.SetBinding(MotorItem.ValueProperty, "Values[0]");

            stack.Children.Add(controller);
        }
    }
}
