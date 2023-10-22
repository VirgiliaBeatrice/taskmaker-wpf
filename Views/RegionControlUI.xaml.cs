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
        public UiMode UiMode => multiView.UiMode;

        //public string UiStatus

        private string _uiStatusText;

        public string UiStatusText {
            get { return _uiStatusText; }
            set { _uiStatusText = value;

                uiStatus.Text = _uiStatusText;
            }
        }

        public InfoPanel InfoPanel => infoPanel;

        private ControlUiViewModel _selectedUiState;


        public List<ControlUiViewModel> Uis { get; set; } = new List<ControlUiViewModel>();
        private ILogger logger => LogManager.GetCurrentClassLogger();

        public RegionControlUI() {
            InitializeComponent();
            logger.Info(np.pi);

            PreviewKeyUp += RegionControlUI_PreviewKeyUp;

        }

        private void RegionControlUI_PreviewKeyUp(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Escape:
                    ChangeMode(UiMode.Default);
                    break;
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e) {
            var vm = DataContext as RegionControlUIViewModel;

            //vm?.Invalidate();
        }


        private void ChangeMode(UiMode mode) {
            if (multiView.Controllers.Count == 0) {
                // send no selected ui message
                var msg = new ShowMessageBoxMessage() {
                    Caption = "Error",
                    Message = "No selected UI",
                    Icon = MessageBoxImage.Error
                };

                WeakReferenceMessenger.Default.Send(msg);
            }
            else {
                multiView.UiMode = mode;

                switch (mode) {
                    case UiMode.Default:
                        break;
                    case UiMode.Add:
                        break;
                    case UiMode.Remove:
                        break;
                    case UiMode.Move:
                        break;
                    case UiMode.Assign:
                        break;
                    case UiMode.Build:
                        break;
                    case UiMode.Trace:
                        break;
                    case UiMode.Drag:
                        break;
                    case UiMode.Pan:
                        break;
                    case UiMode.Zoom:
                        break;
                    case UiMode.Reset:
                        break;
                    default:
                        break;
                }
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
                case "tbBtnAssign":
                    snackbar.Icon = Icons.Assign;
                    snackbar.SupportingText = "Assign";
                    ChangeMode(UiMode.Assign);
                    break;
                case "tbBtnReset":
                    //snackbar.Icon = Icons.Reset;
                    //snackbar.SupportingText = "Reset Pan/Zoom";
                    ChangeMode(UiMode.Reset);
                    break;
                case "tbBtnShowInfoPanel":
                    if (infoPanel.Visibility == Visibility.Visible)
                        infoPanel.Visibility = Visibility.Collapsed;
                    else
                        infoPanel.Visibility = Visibility.Visible;

                    break;
                default:
                    break;
            }

            snackbar.Visibility = Visibility.Visible;
        }

        private void lbUiStates_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var list = sender as ListBox;

            if (e.LeftButton == MouseButtonState.Pressed) {
                //multiView.Close();
                //multiView.Open(list.SelectedItem as ControlUiViewModel);
                //_selectedUiState = list.SelectedItem as ControlUiViewModel;

                // Send a ShowMessageBoxMessage to inform success
                var msg = new ShowMessageBoxMessage() {
                    Caption = "Success",
                    Message = "Selected UIs are shown in the view."
                };

                WeakReferenceMessenger.Default.Send(msg);
            }
        }

        private void tabMapLbMapStates_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

        }
    }
}
