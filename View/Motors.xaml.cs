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
using taskmaker_wpf.Model;

namespace taskmaker_wpf.View {
    /// <summary>
    /// Interaction logic for Motors.xaml
    /// </summary>
    public partial class Motors : Page {
        public List<Slider> Sliders { get; set; } = new List<Slider>();

        public Motors(Model.Data.Motor[] motors) {
            //TestMotor = motor;

            InitializeComponent();

            foreach (var motor in motors) {
                var binding = new Binding("Value");
                var slider = new Slider();
                var sp = new StackPanel();
                var label = new Label();
                binding.Source = motor;

                sp.Orientation = Orientation.Horizontal;
                label.SetBinding(Label.ContentProperty, binding);
                label.HorizontalContentAlignment = HorizontalAlignment.Center;
                //slider.Minimum = motor.Entity.Minimum;
                //slider.Maximum = motor.Entity.Maximum;
                slider.Margin = new Thickness(20, 20, 20, 20);
                slider.Width = 200;

                slider.SetBinding(Slider.ValueProperty, binding);
                sp.Children.Add(slider);
                sp.Children.Add(label);
                stack0.Children.Add(sp);
            }
        }
    }
}
