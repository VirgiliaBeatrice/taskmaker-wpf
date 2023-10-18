using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
    /// Interaction logic for InfoPanel.xaml
    /// </summary>
    public partial class InfoPanel : UserControl
    {
        public string Info {
            get { return (string)GetValue(InfoProperty); }
            set { SetValue(InfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Info.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InfoProperty =
            DependencyProperty.Register("Info", typeof(string), typeof(InfoPanel), new PropertyMetadata(""));


        public InfoPanel()
        {
            InitializeComponent();

            container.Effect = Evelations.Lv3;
            Opacity = 0.32;
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e) {
            var br = (sender as Border);

            br.Opacity = 1;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e) {
            var br = sender as Border;

            br.Opacity = 0.32;
        }
    }
}
