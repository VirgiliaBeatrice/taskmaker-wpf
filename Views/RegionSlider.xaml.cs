using Prism.Regions;
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

namespace taskmaker_wpf.Views {
    /// <summary>
    /// Interaction logic for RegionSlider.xaml
    /// </summary>
    public partial class RegionSlider : UserControl, INavigationAware {
        public RegionSlider() {
            InitializeComponent();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            navigationContext.Parameters.TryGetValue("sub", out string message);

            if (message == "Arm") {
                var vm = DataContext as RegionMotorViewModel;

                if (vm.MotorStates.Length == 0) {
                    vm.AddMotor();
                    vm.AddMotor();
                    vm.AddMotor();

                    var idx = 0;

                    foreach (var motor in vm.MotorStates) {
                        motor.NuibotBoardId = 0;
                        motor.NuibotMotorId = 1 + idx;

                        motor.Max = 10000;
                        motor.Min = -10000;
                        idx++;

                        vm.UpdateMotor(motor);
                    }
                }
            }
            else {
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
        }
    }
}
