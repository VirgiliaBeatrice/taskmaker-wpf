using System;
using System.Collections;
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
    /// Interaction logic for TargetsPanel.xaml
    /// </summary>
    public partial class TargetsPanel : UserControl {



        public IEnumerable SelectedTargets {
            get { return (IEnumerable)GetValue(SelectedTargetsProperty); }
            set { SetValue(SelectedTargetsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedTargets.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTargetsProperty =
            DependencyProperty.Register("SelectedTargets", typeof(IEnumerable), typeof(TargetsPanel), new PropertyMetadata(new object[0]));



        public IEnumerable ValidTargets {
            get { return (IEnumerable)GetValue(ValidTargetsProperty); }
            set { SetValue(ValidTargetsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValidTargets.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidTargetsProperty =
            DependencyProperty.Register("ValidTargets", typeof(IEnumerable), typeof(TargetsPanel), new PropertyMetadata(new object[0]));



        public TargetsPanel() {
            InitializeComponent();
        }
    }
}
