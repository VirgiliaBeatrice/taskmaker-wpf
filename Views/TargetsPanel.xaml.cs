using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for TargetsPanel.xaml
    /// </summary>
    public partial class TargetsPanel : UserControl {
        public TargetsPanel() {
            InitializeComponent();
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            var tb = sender as TextBox;

            if (e.Key == Key.Enter) {
                var binding = BindingOperations.GetBindingExpression(tb, TextBox.TextProperty);

                binding?.UpdateSource();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            System.Windows.MessageBox.Show("Updated.", "Notification");
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e) {
            var popup = FindName("popup0") as Popup;

            if (popup != null) {
                popup.IsOpen = !popup.IsOpen;
            }
        }

        private void lbMaps_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count != 0) {
                var selectedMap = e.AddedItems[0] as NLinearMapState;
                var vm = DataContext as RegionControlUIViewModel;

                vm.SelectedMap = selectedMap;
            }

            //if (e.RemovedItems.Count != 0) {
            //    var selectedMap = e.RemovedItems[0] as NLinearMapState;
            //    var vm = DataContext as RegionControlUIViewModel;

            //    vm.SelectedMap = null; ;
            //}
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e) {
            var vm = (sender as Button).DataContext as RegionControlUIViewModel;

            vm.Invalidate();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var state = (sender as Slider).DataContext as MotorState;
            var vm = DataContext as RegionControlUIViewModel;

            vm.UpdateMotor(state);
        }

        private void btnOpenCompactUi_Click(object sender, RoutedEventArgs e) {
            var parent = (StackPanel)VisualTreeHelper.GetParent(sender as UIElement);
            var popup = parent.Children.OfType<Popup>().First();

            if (popup != null) {
                popup.IsOpen = !popup.IsOpen;

                popup.Focus();
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e) {
            var vm = DataContext as RegionControlUIViewModel;

            vm?.InvalidateValidPlugs();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            var state = (sender as Button).DataContext as MotorState;

            state.Value = new double[1] { 0 };

            var vm = DataContext as RegionControlUIViewModel;

            vm.UpdateMotor(state);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            var checkbox = sender as CheckBox;

            if (checkbox.IsChecked == null) {
                return;
            }
            else if (checkbox.IsChecked == true) {
                var vm = DataContext as RegionControlUIViewModel;

                //vm.SelectedInplugs.Concat(checkbox.DataContext)
                //vm?.InvalidateValidPorts();
            }
            else {
                //var vm
            }

        }

        private void btnUpdateSockets_Clicked(object sender, RoutedEventArgs e) {
            var vm = DataContext as RegionControlUIViewModel;

            var lbInPlugs = FindName("lbInPlugs") as ListBox;
            var inPlugs = lbInPlugs.SelectedItems.Cast<InPlug>().ToArray();

            vm.UpdateSocket(vm.SelectedMap, inPlugs);

            var lbOutPlugs = FindName("lbOutPlugs") as ListBox;
            var outPlugs = lbOutPlugs.SelectedItems.Cast<OutPlug>().ToArray();

            vm.UpdateSocket(vm.SelectedMap, outPlugs);
        }
    }
}
