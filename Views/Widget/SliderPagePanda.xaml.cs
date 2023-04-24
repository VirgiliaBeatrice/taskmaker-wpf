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

            if (vm.MotorStates.Length == 0) {
                vm.AddMotor();
                vm.AddMotor();
                vm.AddMotor();
                vm.AddMotor();
                vm.AddMotor();
                vm.AddMotor();



                vm.MotorStates[0].NuibotBoardId = 0;
                vm.MotorStates[0].NuibotMotorId = 1;
                vm.MotorStates[1].NuibotBoardId = 0;
                vm.MotorStates[1].NuibotMotorId = 2;
                vm.MotorStates[2].NuibotBoardId = 0;
                vm.MotorStates[2].NuibotMotorId = 3;
                vm.MotorStates[3].NuibotBoardId = 1;
                vm.MotorStates[3].NuibotMotorId = 1;
                vm.MotorStates[4].NuibotBoardId = 1;
                vm.MotorStates[4].NuibotMotorId = 2;
                vm.MotorStates[5].NuibotBoardId = 1;
                vm.MotorStates[5].NuibotMotorId = 3;

                foreach (var state in vm.MotorStates) {
                    state.Min = -10000;
                    state.Max = 10000;
                    
                    vm.UpdateMotor(state);
                }
            }
        }

        private void UserControl_Focused(object sender, RoutedEventArgs e) {
            var vm = DataContext as RegionMotorViewModel;

            vm.InvalidateMotorState();
        }
    }
}
