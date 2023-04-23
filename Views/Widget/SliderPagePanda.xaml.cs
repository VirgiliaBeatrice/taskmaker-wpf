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
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Views.Widget {
    /// <summary>
    /// Interaction logic for SliderPagePanda.xaml
    /// </summary>
    public partial class SliderPagePanda : UserControl {
        public SliderPagePanda() {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            var vm = DataContext as RegionMotorViewModel;

            vm.AddMotor();
            vm.AddMotor();
            vm.AddMotor();
            vm.AddMotor();
            vm.AddMotor();
            vm.AddMotor();
        }
    }
}
