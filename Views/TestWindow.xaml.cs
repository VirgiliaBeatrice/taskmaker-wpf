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
        private readonly EvaluationService _evaluationService;
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

        public TestWindow(MotorService motorSrv, EvaluationService evaluationService) {
            _motorSrv = motorSrv;
            _evaluationService = evaluationService;
            InitializeComponent();

            // Register ShowMessageBox message
            Messenger.Default.Register<ShowMessageBoxMessage>(this);
            Messenger.Default.Register<DialogRequestMessage>(this);
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e) {
            var vm = DataContext as TestWindowViewModel;

            switch (e.Key) {
                case Key.None:
                    break;
                case Key.Cancel:
                    break;
                case Key.Back:
                    break;
                case Key.Tab:
                    break;
                case Key.LineFeed:
                    break;
                case Key.Clear:
                    break;
                case Key.Enter:
                    break;
                case Key.Pause:
                    break;
                case Key.Capital:
                    break;
                case Key.HangulMode:
                    break;
                case Key.JunjaMode:
                    break;
                case Key.FinalMode:
                    break;
                case Key.HanjaMode:
                    break;
                case Key.Escape:
                    break;
                case Key.ImeConvert:
                    break;
                case Key.ImeNonConvert:
                    break;
                case Key.ImeAccept:
                    break;
                case Key.ImeModeChange:
                    break;
                case Key.Space:
                    break;
                case Key.PageUp:
                    break;
                case Key.Next:
                    break;
                case Key.End:
                    break;
                case Key.Home:
                    break;
                case Key.Left:
                    break;
                case Key.Up:
                    break;
                case Key.Right:
                    break;
                case Key.Down:
                    break;
                case Key.Select:
                    break;
                case Key.Print:
                    break;
                case Key.Execute:
                    break;
                case Key.PrintScreen:
                    break;
                case Key.Insert:
                    break;
                case Key.Delete:
                    break;
                case Key.Help:
                    break;
                case Key.D0:
                    break;
                case Key.D1:

                    break;
                case Key.D2:

                    break;
                case Key.D3:
                    break;
                case Key.D4:
                    break;
                case Key.D5:
                    break;
                case Key.D6:
                    break;
                case Key.D7:
                    break;
                case Key.D8:
                    break;
                case Key.D9:
                    break;
                case Key.A:
                    break;
                case Key.B:
                    break;
                case Key.C:
                    break;
                case Key.D:
                    break;
                case Key.E:
                    break;
                case Key.F:
                    break;
                case Key.G:
                    break;
                case Key.H:
                    break;
                case Key.I:
                    break;
                case Key.J:
                    break;
                case Key.K:
                    break;
                case Key.L:
                    break;
                case Key.M:
                    break;
                case Key.N:
                    break;
                case Key.O:
                    break;
                case Key.P:
                    break;
                case Key.Q:
                    break;
                case Key.R:
                    break;
                case Key.S:
                    break;
                case Key.T:
                    break;
                case Key.U:
                    break;
                case Key.V:
                    break;
                case Key.W:
                    break;
                case Key.X:
                    break;
                case Key.Y:
                    break;
                case Key.Z:
                    break;
                case Key.LWin:
                    break;
                case Key.RWin:
                    break;
                case Key.Apps:
                    break;
                case Key.Sleep:
                    break;
                case Key.NumPad0:
                    break;
                case Key.NumPad1:
                    break;
                case Key.NumPad2:
                    break;
                case Key.NumPad3:
                    break;
                case Key.NumPad4:
                    break;
                case Key.NumPad5:
                    break;
                case Key.NumPad6:
                    break;
                case Key.NumPad7:
                    break;
                case Key.NumPad8:
                    break;
                case Key.NumPad9:
                    break;
                case Key.Multiply:
                    break;
                case Key.Add:
                    break;
                case Key.Separator:
                    break;
                case Key.Subtract:
                    break;
                case Key.Decimal:
                    break;
                case Key.Divide:
                    break;
                case Key.F1:
                    vm.NavigateCommandExecute("RegionHome");
                    e.Handled = true;
                    break;
                case Key.F2:
                    vm.NavigateCommandExecute("RegionSettings");
                    e.Handled = true;
                    break;
                case Key.F3:
                    vm.NavigateCommandExecute("RegionControlUI");
                    e.Handled = true;
                    break;
                case Key.F4:
                    vm.NavigateCommandExecute("RegionMotor");
                    e.Handled = true;
                    break;
                case Key.F5:
                    break;
                case Key.F6:
                    break;
                case Key.F7:
                    break;
                case Key.F8:
                    _evaluationService.InitializeMotors();
                    // show messagebox
                    var msgD1 = new ShowMessageBoxMessage {
                        Message = "Motors have initialized.",
                        Caption = "Evaluation Init",
                        Button = MessageBoxButton.OK,
                        Icon = MessageBoxImage.Information,
                    };

                    MessageBox.Show(msgD1.Message, msgD1.Caption, msgD1.Button, msgD1.Icon);
                    break;
                case Key.F9:
                    _evaluationService.InitializeSurvey();
                    // show messagebox
                    var msgD2 = new ShowMessageBoxMessage {
                        Message = "Survey has initialized.",
                        Caption = "Evaluation Init",
                        Button = MessageBoxButton.OK,
                        Icon = MessageBoxImage.Information,
                    };

                    MessageBox.Show(msgD2.Message, msgD2.Caption, msgD2.Button, msgD2.Icon);
                    break;
                case Key.F10:
                    break;
                case Key.F11:
                    break;
                case Key.F12:
                    if (NavPanel.Visibility == Visibility.Visible)
                        NavPanel.Visibility = Visibility.Collapsed;
                    else 
                        NavPanel.Visibility = Visibility.Visible;
                    break;
                case Key.F13:
                    break;
                case Key.F14:
                    break;
                case Key.F15:
                    break;
                case Key.F16:
                    break;
                case Key.F17:
                    break;
                case Key.F18:
                    break;
                case Key.F19:
                    break;
                case Key.F20:
                    break;
                case Key.F21:
                    break;
                case Key.F22:
                    break;
                case Key.F23:
                    break;
                case Key.F24:
                    break;
                case Key.NumLock:
                    break;
                case Key.Scroll:
                    break;
                case Key.LeftShift:
                    break;
                case Key.RightShift:
                    break;
                case Key.LeftCtrl:
                    break;
                case Key.RightCtrl:
                    break;
                case Key.LeftAlt:
                    break;
                case Key.RightAlt:
                    break;
                case Key.BrowserBack:
                    break;
                case Key.BrowserForward:
                    break;
                case Key.BrowserRefresh:
                    break;
                case Key.BrowserStop:
                    break;
                case Key.BrowserSearch:
                    break;
                case Key.BrowserFavorites:
                    break;
                case Key.BrowserHome:
                    break;
                case Key.VolumeMute:
                    break;
                case Key.VolumeDown:
                    break;
                case Key.VolumeUp:
                    break;
                case Key.MediaNextTrack:
                    break;
                case Key.MediaPreviousTrack:
                    break;
                case Key.MediaStop:
                    break;
                case Key.MediaPlayPause:
                    break;
                case Key.LaunchMail:
                    break;
                case Key.SelectMedia:
                    break;
                case Key.LaunchApplication1:
                    break;
                case Key.LaunchApplication2:
                    break;
                case Key.Oem1:
                    break;
                case Key.OemPlus:
                    break;
                case Key.OemComma:
                    break;
                case Key.OemMinus:
                    break;
                case Key.OemPeriod:
                    break;
                case Key.Oem2:
                    break;
                case Key.Oem3:
                    break;
                case Key.AbntC1:
                    break;
                case Key.AbntC2:
                    break;
                case Key.Oem4:
                    break;
                case Key.Oem5:
                    break;
                case Key.Oem6:
                    break;
                case Key.Oem7:
                    break;
                case Key.Oem8:
                    break;
                case Key.Oem102:
                    break;
                case Key.ImeProcessed:
                    break;
                case Key.System:
                    break;
                case Key.DbeAlphanumeric:
                    break;
                case Key.DbeKatakana:
                    break;
                case Key.DbeHiragana:
                    break;
                case Key.DbeSbcsChar:
                    break;
                case Key.DbeDbcsChar:
                    break;
                case Key.DbeRoman:
                    break;
                case Key.Attn:
                    break;
                case Key.CrSel:
                    break;
                case Key.DbeEnterImeConfigureMode:
                    break;
                case Key.DbeFlushString:
                    break;
                case Key.DbeCodeInput:
                    break;
                case Key.DbeNoCodeInput:
                    break;
                case Key.DbeDetermineString:
                    break;
                case Key.DbeEnterDialogConversionMode:
                    break;
                case Key.OemClear:
                    break;
                case Key.DeadCharProcessed:
                    break;
            }


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
                    WeakReferenceMessenger.Default.Send(new SaveMessage {
                        Path = filename
                    });
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
                    WeakReferenceMessenger.Default.Send(new LoadMessage {
                        Path = filename
                    });
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

            var dialog = new DialogController();


            Binding binding;
            binding = new Binding("UpdateCommand") {
                Source = motorDialogViewModel,
            };
            dialog.SetBinding(DialogController.CommitCommandProperty, binding);

            binding = new Binding() {
                Source = motorDialogViewModel,
            };
            dialog.SetBinding(DataContextProperty, binding);

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

    public class SaveMessage { 
        public string Path { get; set; }
    }
    public class LoadMessage {
        public string Path { get; set; }

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
