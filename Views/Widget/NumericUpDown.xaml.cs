using CommunityToolkit.Mvvm.Input;
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
    /// Interaction logic for NumericUpDwon.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl {
        public NumericUpDown() {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(int), typeof(NumericUpDown), new PropertyMetadata(default(int)));

        public int Value {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        [RelayCommand]
        private void IncreaseValue() {
            Value++;
        }

        [RelayCommand]
        private void DecreaseValue() {
            Value--;
        }

        private void ValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            // Ensure only numeric input
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
}
