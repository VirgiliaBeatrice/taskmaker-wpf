using Prism.Events;
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
using System.Windows.Shapes;
using taskmaker_wpf.Domain;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Views {
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window {
        private IEventAggregator _eventAggregator;
        private SaveInteractor _save;

        public TestWindow(IEventAggregator @event, SaveInteractor save) {
            //_eventAggregator = @event;
            _save = save;
            InitializeComponent();
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.S & Keyboard.Modifiers == ModifierKeys.Control) {
                _save.Handle(new SaveRequest(), (bool res) => { });
                //_eventAggregator.GetEvent<SystemSaveEvent>().Publish();
                e.Handled = true;
            }
            else if (e.Key == Key.S & Keyboard.Modifiers == ModifierKeys.Shift & Keyboard.Modifiers == ModifierKeys.Control) {
                //_save.Handle
            }
        }
    }

    public class SystemSaveEvent : PubSubEvent {

    }
}
