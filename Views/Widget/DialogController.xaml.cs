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
using taskmaker_wpf.Services;
using taskmaker_wpf.ViewModels;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;


namespace taskmaker_wpf.Views.Widget
{
    /// <summary>
    /// Interaction logic for DialogController.xaml
    /// </summary>
    public partial class DialogController : UserControl
    {


        public ICommand CommitCommand {
            get { return (ICommand)GetValue(CommitCommandProperty); }
            set { SetValue(CommitCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommitCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommitCommandProperty =
            DependencyProperty.Register("CommitCommand", typeof(ICommand), typeof(DialogController), new PropertyMetadata(null));


        private TestWindow parent;

        public DialogController() {
            InitializeComponent();

            Loaded += DialogController_Loaded;
            Unloaded += DialogController_Unloaded;
        }

        private void DialogController_Unloaded(object sender, RoutedEventArgs e) {
            CommitCommand?.Execute(null);
        }

        private void DialogController_Loaded(object sender, RoutedEventArgs e) {
            parent = VisualTreeHelperExtensions.FindParentOfType<TestWindow>(this);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e) {
            parent.DialogTCS.SetResult(new DialogResult() {
                Result = MessageBoxResult.OK,
            });
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            parent.DialogTCS.SetResult(new DialogResult() {
                Result = MessageBoxResult.Cancel
            });
        }
    }
}
