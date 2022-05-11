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
        public RegionMotor() {
            InitializeComponent();
        }
    }
}
