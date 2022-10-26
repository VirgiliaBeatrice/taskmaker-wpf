using System;
using System.Collections;
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

namespace taskmaker_wpf.Views {
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

                if (binding != null) {
                    binding.UpdateSource();
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }
    }

    public class ISelectableTargetConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return targetType + $"[{value}]";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
