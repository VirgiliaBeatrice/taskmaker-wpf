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

namespace taskmaker_wpf.Views {
    /// <summary>
    /// Interaction logic for MotorControllerRegion.xaml
    /// </summary>
    public partial class MotorItem : UserControl {
        public static readonly DependencyProperty LabelNameProperty = DependencyProperty.Register(
            "LabelName",
            typeof(string),
            typeof(MotorItem),
            new PropertyMetadata("Null"));
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                "Minimum",
                typeof(int),
                typeof(MotorItem),
                new PropertyMetadata(0));
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                "Maximum",
                typeof(int),
                typeof(MotorItem),
                new PropertyMetadata(10));
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(int),
                typeof(MotorItem),
                new PropertyMetadata(5));
        public static readonly DependencyProperty BoardIdProperty =
            DependencyProperty.Register(
                "BoardId",
                typeof(int),
                typeof(MotorItem),
                new PropertyMetadata(-1));
        public static readonly DependencyProperty MotorIdProperty =
                    DependencyProperty.Register(
                        "MotorId",
                        typeof(int),
                        typeof(MotorItem),
                        new PropertyMetadata(-1));

        public string LabelName {
            get { return (string)GetValue(LabelNameProperty); }
            set { SetValue(LabelNameProperty, value); }
        }

        public int Minimum {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public int Maximum {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public int Value {
            get { return (int)GetValue(ValueProperty); }
            set { 
                SetValue(ValueProperty, value);
            }
        }

        public int BoardId {
            get { return (int)GetValue(BoardIdProperty); }
            set { SetValue(BoardIdProperty, value); }
        }

        public int MotorId {
            get { return (int)GetValue(MotorIdProperty); }
            set { SetValue(MotorIdProperty, value); }
        }

        public event EventHandler<RoutedPropertyChangedEventArgs<double>> ValueChanged;

        public MotorItem() {
            InitializeComponent();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            ValueChanged?.Invoke(this, e);
        }
    }
}
