using AutoMapper;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
using taskmaker_wpf.Services;
using taskmaker_wpf.ViewModels;
using taskmaker_wpf.Views.Widget;

namespace taskmaker_wpf.Views {
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window, IRecipient<ShowMessageBoxMessage>, IRecipient<ShowDialogMessage> {
        private readonly MotorService _motorSrv;
        private IEventAggregator _eventAggregator;
        private SystemInteractorBus _systemBus;
        private static readonly FieldInfo _menuDropAlignmentField;

        static TestWindow() {
            //フィールド情報を取得  
            _menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);

            //アサーション（なくても良い）  
            System.Diagnostics.Debug.Assert(_menuDropAlignmentField != null);

            EnsureStandardPopupAlignment();
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
        }

        private static void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e) {
            EnsureStandardPopupAlignment();
        }

        private static void EnsureStandardPopupAlignment() {
            //MenuDropAlignmentがTrueならFalseに書き換える  
            if (SystemParameters.MenuDropAlignment && _menuDropAlignmentField != null) {
                //_menuDropAlignmentField.SetValue(null, false);
            }
        }

        public TestWindow(IEventAggregator @event, SystemInteractorBus systemBus, MotorService motorSrv) {
            _motorSrv = motorSrv;
            _eventAggregator = @event;
            _systemBus = systemBus;
            InitializeComponent();

            // Register ShowMessageBox message
            WeakReferenceMessenger.Default.Register<ShowMessageBoxMessage>(this);
            WeakReferenceMessenger.Default.Register<ShowDialogMessage>(this);
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.S & Keyboard.Modifiers == ModifierKeys.Control) {
                // Configure save file dialog box
                var dlg = new Microsoft.Win32.SaveFileDialog {
                    FileName = $"Project_{DateTime.Now.ToString("yyyyMMddHHmmss")}", // Default file name
                    DefaultExt = ".xml", // Default file extension
                    Filter = "Xml documents (.xml)|*.xml" // Filter files by extension
                };

                // Show save file dialog box
                bool? result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true) {
                    // Save document
                    string filename = dlg.FileName;

                    _systemBus.Handle(new SaveRequest() { FileName = filename }, out bool res);
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
                    _systemBus.Handle(new LoadRequest() { FileName = filename }, out bool res);
                    _eventAggregator.GetEvent<SystemLoadedEvent>().Publish();
                }

                e.Handled = true;
            }
        }

        public void Receive(ShowMessageBoxMessage msg) {
            var result = MessageBox.Show(msg.Message, msg.Caption, msg.Button, msg.Icon);

            msg.Reply(result);
        }

        public void Receive(ShowDialogMessage msg) {
            Scrim.Visibility = Visibility.Visible;
            Dialog.Visibility = Visibility.Visible;

            var motors = _motorSrv.Motors.Select(e => new MotorState {
                Id = e.Id,
                NuibotBoardId = e.NuibotBoardId,
                NuibotMotorId = e.NuibotMotorId,
                Name = e.Name,
                Max = e.Max,
                Min = e.Min,
                Value = e.Value,
            }).ToArray();

            var dialog = new DialogController() {
                Motors = motors,
            };
            Dialog.Child = dialog;


            //msg.Reply(true);
        }
    }

    public class SystemSaveEvent : PubSubEvent {

    }

    public class SystemLoadedEvent : PubSubEvent {

    }

    public class ShowMessageBoxMessage : AsyncRequestMessage<MessageBoxResult> {
        public string Message { get; set; }
        public string Caption { get; set; }
        public MessageBoxButton Button { get; set; }
        public MessageBoxImage Icon { get; set; }
    }

    public class ShowDialogMessage : AsyncRequestMessage<bool> {
    }
}
