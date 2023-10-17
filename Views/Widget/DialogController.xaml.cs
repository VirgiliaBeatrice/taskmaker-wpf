using CommunityToolkit.Mvvm.Messaging;
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
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Views.Widget
{
    /// <summary>
    /// Interaction logic for DialogController.xaml
    /// </summary>
    public partial class DialogController : UserControl
    {
        public MotorState[] Motors {
            get { return (MotorState[])GetValue(MotorsProperty); }
            set { SetValue(MotorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Motors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MotorsProperty =
            DependencyProperty.Register("Motors", typeof(MotorState[]), typeof(DialogController), new PropertyMetadata(default));



        public DialogController()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            WeakReferenceMessenger.Default.Send(new CloseDialogMessage());
        }
    }
}
