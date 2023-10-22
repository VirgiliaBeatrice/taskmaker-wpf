using AutoMapper;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
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
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Services;
using taskmaker_wpf.ViewModels;
using taskmaker_wpf.Views.Widget;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace taskmaker_wpf.Views {
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window, IRecipient<ShowMessageBoxMessage>, IRecipient<DialogRequestMessage>{
        private readonly MotorService _motorSrv;
        private IEventAggregator _eventAggregator;
        private static readonly FieldInfo _menuDropAlignmentField;
        public TaskCompletionSource<DialogResult> DialogTCS;

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

        public TestWindow(IEventAggregator @event, MotorService motorSrv) {
            _motorSrv = motorSrv;
            _eventAggregator = @event;
            InitializeComponent();

            // Register ShowMessageBox message
            Messenger.Default.Register<ShowMessageBoxMessage>(this);
            Messenger.Default.Register<DialogRequestMessage>(this);
            //Messenger.Default.Register<DisplayDialogMessage>(this);
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
                    _eventAggregator.GetEvent<SystemLoadedEvent>().Publish();
                }

                e.Handled = true;
            }
        }

        public void Receive(ShowMessageBoxMessage msg) {
            var result = MessageBox.Show(msg.Message, msg.Caption, msg.Button, msg.Icon);

            msg.Reply(result);
        }

        public void Receive(DialogRequestMessage msg) {
            msg.Reply(ShowDialogAsync());
        }

        private async Task<DialogResult> ShowDialogAsync() {
            ShowModal();

            var result = await WaitForDialogResult();

            if (result.Result == MessageBoxResult.OK) {
                result.Value = _motorSrv.GetAll().Select(e => e.Value).ToArray();
            }

            CloseModal();

            return result;
        }

        public void ShowModal() {
            Scrim.Visibility = Visibility.Visible;
            Dialog.Visibility = Visibility.Visible;

            var motors = _motorSrv.GetAll().Select(e => new MotorState {
                Id = e.Id,
                NuiBoardId = e.NuiBoardId,
                NuiMotorId = e.NuiMotorId,
                Name = e.Name,
                Max = e.Max,
                Min = e.Min,
                Value = e.Value,
            }).ToArray();

            var motorDialogViewModel = (Application.Current as App).Container.Resolve<MotorDialogViewModel>();

            var dialog = new DialogController() {
                DataContext = motorDialogViewModel,
            };

            Binding binding;
            binding = new Binding("CommitCommand") {
                Source = motorDialogViewModel,
            };
            dialog.SetBinding(DialogController.CommitCommandProperty, binding);

            Dialog.Child = dialog;
        }

        public void CloseModal() {
            Scrim.Visibility = Visibility.Hidden;
            Dialog.Visibility = Visibility.Hidden;

            Dialog.Child = null;
        }

        public Task<DialogResult> WaitForDialogResult() {
            DialogTCS = new TaskCompletionSource<DialogResult>();

            return DialogTCS.Task;
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

    public class DialogRequestMessage : AsyncRequestMessage<DialogResult> {

    }

    public class DialogResult {
        public MessageBoxResult Result { get; set; }
        public object Value { get; set; }
    }
}
