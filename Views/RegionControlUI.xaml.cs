using SkiaSharp;
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
using System.Windows.Threading;
using taskmaker_wpf.ViewModels;
using Numpy;
using taskmaker_wpf.Model.Data;
using System.Windows.Controls.Primitives;
using NLog;
using taskmaker_wpf.Views.Widget;
using CommunityToolkit.Mvvm.Messaging;

namespace taskmaker_wpf.Views {
    public static class Icons {
        public static string Select => "\ue7c9";
        public static string Control => "\uec61";
        public static string Add => "\uecc8";
        public static string Remove => "\uecc9";
        public static string Move => "\ue759";
        public static string Assign => "\ue71b";
        public static string Pan => "\uece9";
        public static string Zoom => "\uece8";
        public static string Reset => "\ue777";
    }

    /// <summary>
    /// Interaction logic for RegionControlUI.xaml
    /// </summary>
    public partial class RegionControlUI : UserControl {

        public UiMode Mode { get; set; } = UiMode.Default;
        public string UiStatus { get; set; } = "";
        public List<ControlUiState> Uis { get; set; } = new List<ControlUiState>();
        private ILogger logger => LogManager.GetCurrentClassLogger();

        public RegionControlUI() {
            InitializeComponent();

            //_timer = new DispatcherTimer();
            //_timer.Interval = TimeSpan.FromMilliseconds(16);
            //_timer.Tick += _timer_Tick;

            //_viewModel = DataContext as RegionControlUIViewModel;

            //Console.WriteLine(np.pi);
            logger.Info(np.pi);

            //var task = new Task<bool>(() => {
            //    Console.WriteLine(np.pi);

            //    return true;
            //});

            //task.Start();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            //Console.WriteLine(skElement.Focusable);
            //Keyboard.Focus(skElement);
            //Console.WriteLine(Keyboard.FocusedElement);
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e) {
            var popup = FindName("popup0") as Popup;

            if (popup != null) {
                popup.IsOpen = !popup.IsOpen;
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e) {
            var vm = DataContext as RegionControlUIViewModel;

            vm?.Invalidate();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            // find all selected uis
            var vm = DataContext as RegionControlUIViewModel;

            // find listbox according to the x:Name
            var lb = FindName("lbUiStates") as ListBox;

            vm.SelectedUis = lb.SelectedItems.Cast<ControlUiState>().ToArray();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            var uiState = (sender as CheckBox).DataContext as ControlUiState;

            var lb = FindName("lbUiStates") as ListBox;

            lb.SelectedItems.Add(uiState);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            var uiState = (sender as CheckBox).DataContext as ControlUiState;

            var lb = FindName("lbUiStates") as ListBox;

            lb.SelectedItems.Remove(uiState);
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e) {
            var br = (sender as Border);

            br.Opacity = 1;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e) {
            var br = sender as Border;

            br.Opacity = 0.4;
        }

        private void ChangeMode(UiMode mode) {
            var tbUiStatus = FindName("uiStatus") as TextBlock;

            var ui = multiView.Controllers.FirstOrDefault();

            if (ui != null) {
                //var ui = view.Controllers.First();
                ui.UiMode = mode;
                //ui?.GoToState(mode);

                Mode = mode;

                UiStatus = mode.ToString();
                tbUiStatus.Text = UiStatus;
            }
            else {
                // send no selected ui message
                var msg = new ShowMessageBoxMessage() {
                    Caption = "Error",
                    Message = "No selected UI",
                    Icon = MessageBoxImage.Error
                };

                WeakReferenceMessenger.Default.Send(msg);
            }
        }

        private void ToolBar_Click(object sender, RoutedEventArgs e) {
            var btn = sender as Button;

            // TODO: add all modes.
            switch (btn.Name) {
                case "tbBtnSelect":
                    ChangeMode(UiMode.Default);
                    snackbar.Icon = Icons.Select;
                    snackbar.SupportingText = "Select";
                    break;
                case "tbBtnAdd":
                    ChangeMode(UiMode.Add);
                    snackbar.Icon = Icons.Add;
                    snackbar.SupportingText = "Add";
                    break;
                case "tbBtnRm":
                    ChangeMode(UiMode.Remove);
                    snackbar.Icon = Icons.Remove;
                    snackbar.SupportingText = "Remove";
                    break;
                case "tbBtnPan":
                    ChangeMode(UiMode.Pan);
                    snackbar.Icon = Icons.Pan;
                    snackbar.SupportingText = "Pan";
                    break;
                case "tbBtnZoom":
                    ChangeMode(UiMode.Zoom);
                    snackbar.Icon = Icons.Zoom;
                    snackbar.SupportingText = "Zoom";
                    break;
                case "tbBtnMove":
                    snackbar.Icon = Icons.Move;
                    snackbar.SupportingText = "Move";
                    ChangeMode(UiMode.Move);
                    break;
                case "tbBtnReset":
                    snackbar.Icon = Icons.Reset;
                    snackbar.SupportingText = "Reset Pan/Zoom";
                    ChangeMode(UiMode.Reset);
                    break;
                default:
                    break;
            }

            snackbar.Visibility = Visibility.Visible;
        }

        private void lbUiStates_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var list = sender as ListBox;

            if (e.LeftButton == MouseButtonState.Pressed) {
                multiView.OpenUiController(list.SelectedItem as ControlUiState);

                // Send a ShowMessageBoxMessage to inform success
                var msg = new ShowMessageBoxMessage() {
                    Caption = "Success",
                    Message = "Selected UIs are shown in the view."
                };

                WeakReferenceMessenger.Default.Send(msg);
            }
        }

        private void tabUiBtnAdd_Click(object sender, RoutedEventArgs e) {
            var ui = new ControlUiState {
                Id = Uis.Count + 1,
                Name = $"ControlUi_{Uis.Count + 1}",
            };

            Uis.Add(ui);

            tabUiLbControllers.ItemsSource = Uis;
        }
    }
}
