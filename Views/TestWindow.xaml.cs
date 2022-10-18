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
        private SystemInteractorBus _systemBus;

        public TestWindow(IEventAggregator @event, SystemInteractorBus systemBus) {
            //_eventAggregator = @event;
            _systemBus = systemBus;
            InitializeComponent();
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.S & Keyboard.Modifiers == ModifierKeys.Control) {
                // Configure save file dialog box
                var dlg = new Microsoft.Win32.SaveFileDialog {
                    FileName = "Project", // Default file name
                    DefaultExt = ".xml", // Default file extension
                    Filter = "Xml documents (.xml)|*.xml" // Filter files by extension
                };

                // Show save file dialog box
                bool? result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true) {
                    // Save document
                    string filename = dlg.FileName;
                    _systemBus.Handle(new SaveRequest() { FileName = filename }, (bool res) => { });
                }

                //_eventAggregator.GetEvent<SystemSaveEvent>().Publish();
                e.Handled = true;
            }
            else if (e.Key == Key.O & Keyboard.Modifiers == ModifierKeys.Control) {
                //_save.Handle
                // Configure open file dialog box
                var dialog = new Microsoft.Win32.OpenFileDialog {
                    FileName = "Project", // Default file name
                    DefaultExt = ".xml", // Default file extension
                    Filter = "Xml files (.xml)|*.xml" // Filter files by extension
                };

                // Show open file dialog box
                bool? result = dialog.ShowDialog();

                // Process open file dialog box results
                if (result == true) {
                    // Open document
                    string filename = dialog.FileName;
                    _systemBus.Handle(new LoadRequest() { FileName = filename }, (bool res) => { });

                }

                e.Handled = true;
            }
        }
    }

    public class SystemSaveEvent : PubSubEvent {

    }
}
