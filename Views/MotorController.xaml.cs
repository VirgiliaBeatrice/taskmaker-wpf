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
    public partial class MotorController : UserControl {
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                "Minimum",
                typeof(int),
                typeof(MotorController),
                new PropertyMetadata(0));
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                "Maximum",
                typeof(int),
                typeof(MotorController),
                new PropertyMetadata(10));
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(int),
                typeof(MotorController),
                new PropertyMetadata(5));


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
            set { SetValue(ValueProperty, value); }
        }

    public MotorController() {
            InitializeComponent();
        }
    }
}
