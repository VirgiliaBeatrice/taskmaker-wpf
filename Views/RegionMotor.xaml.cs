using System;
using System.Collections.Generic;
using System.Globalization;
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
using taskmaker_wpf.Services;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Views {
    public class MotorInfo {
        public string BoardId { get; set; }
        public string MotorId { get; set; }
    }

    public class MotorInfoConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values[0] != null && values[1] != null && values.Length == 2) {
                var boardId = values[0].ToString();
                var motorId = values[1].ToString();

                return new MotorInfo { BoardId = boardId, MotorId = motorId };
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindowNew.xaml
    /// </summary>
    public partial class RegionMotor : UserControl {
        private readonly EvaluationService _evaSrv;

        public RegionMotor(EvaluationService evaSrv) {
            InitializeComponent();

            _evaSrv = evaSrv;
        }


        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var state = (sender as Slider).DataContext as MotorState;
            var vm = DataContext as RegionMotorViewModel;

            e.Handled = true;
            if (e.NewValue != e.OldValue)
                vm.UpdateMotorValue(state, e.NewValue);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var state = (sender as Button).DataContext as MotorState;
            var vm = DataContext as RegionMotorViewModel;

            vm.UpdateMotor(state);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var state = (sender as ComboBox).DataContext as MotorState;
            var cb = sender as ComboBox;

            if (cb.Name == "cbBoardId")
                state.NuibotBoardId = (int)e.AddedItems[0];
            else if (cb.Name == "cbMotorId")
                state.NuibotMotorId = (int)e.AddedItems[0];
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            var vm = DataContext as RegionMotorViewModel;
            var dg = FindName("dgMotors") as DataGrid;

            if (dg.SelectedItem is MotorState state) {
                vm.RemoveMotor(state);
            }
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e) {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            var vm = DataContext as RegionMotorViewModel;
            var dg = FindName("dgMotors") as DataGrid;

            foreach(var state in vm.MotorStates) {
                vm.UpdateMotor(state);
            }
        }

        private void root_PreviewKeyUp(object sender, KeyEventArgs e) {
            var vm = DataContext as RegionMotorViewModel;

            if (e.Key == Key.F1) {
                _evaSrv.Initialize();

                // Popup a message box to inform succss
                MessageBox.Show("Motors initialized successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            if (Focus()) {
                e.Handled = true;
            }

            base.OnMouseLeftButtonDown(e);
        }
    }
}
