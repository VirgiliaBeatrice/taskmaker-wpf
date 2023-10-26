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
        private string _uiStatusText;

        public string UiStatusText {
            get { return _uiStatusText; }
            set { _uiStatusText = value;

                uiStatus.Text = _uiStatusText;
            }
        }

        public InfoPanel InfoPanel => infoPanel;
        private ILogger logger => LogManager.GetCurrentClassLogger();

        public RegionControlUI() {
            InitializeComponent();
            logger.Info(np.pi);

            PreviewKeyUp += RegionControlUI_PreviewKeyUp;

            var vm = DataContext as RegionControlUIViewModel;

            vm.PropertyChanged += Vm_PropertyChanged;

            ChangeColor(evaluationSign, false);
            ChangeColor(practiceSign, false);
            ChangeColor(creationSign, false);
            ChangeColor(performSign, false);

            //DataContextChanged += RegionControlUI_DataContextChanged;
        }

        private void RegionControlUI_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != null && e.NewValue is RegionControlUIViewModel vm) {
                vm.PropertyChanged += Vm_PropertyChanged;
            }

            if (e.OldValue != null && e.OldValue is RegionControlUIViewModel oldVM) {
                oldVM.PropertyChanged -= Vm_PropertyChanged;
            }
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var vm = sender as RegionControlUIViewModel;

            if (e.PropertyName == nameof(RegionControlUIViewModel.IsEvaluationStarted)) {
                ChangeColor(evaluationSign, vm.IsEvaluationStarted);
            }
            else if (e.PropertyName == nameof(RegionControlUIViewModel.IsPracticeStarted)) {
                ChangeColor(practiceSign, vm.IsPracticeStarted);
            }
            else if (e.PropertyName == nameof(RegionControlUIViewModel.IsCreationStarted)) {
                ChangeColor(creationSign, vm.IsCreationStarted);
            }
            else if (e.PropertyName == nameof(RegionControlUIViewModel.IsPerformStarted)) {
                ChangeColor(performSign, vm.IsPerformStarted);
            }
            //if (e.PropertyName == nameof(RegionControlUIViewModel.Mode)) {
            //    UiStatusText = multiView.UiMode.ToString();
            //    ChangeMode(multiView.UiMode);
            //}
        }

        private void ChangeColor(Ellipse shape, bool v) {
            if (v) {
                shape.Fill = new SolidColorBrush(Colors.Green);
            }
            else {
                shape.Fill = new SolidColorBrush(Colors.Red);
            }
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
                var vm = DataContext as RegionControlUIViewModel;

                ChangeSnackbar(mode);

                if (vm.SelectedEvaluation != null) {
                    if (vm.SelectedEvaluation.SelectedSession != null) {
                        vm.SelectedEvaluation.SelectedSession.Mode = mode;
                    }
                }
            }
        }

        private void ToolBar_Click(object sender, RoutedEventArgs e) {
            var btn = sender as Button;

            // TODO: add all modes.
            switch (btn.Name) {
                case "tbBtnSelect":
                    ChangeMode(UiMode.Default);
                    break;
                case "tbBtnControl":
                    ChangeMode(UiMode.Control);
                    break;
                case "tbBtnAdd":
                    ChangeMode(UiMode.Add);
                    break;
                case "tbBtnRm":
                    ChangeMode(UiMode.Remove);
                    break;
                case "tbBtnPan":
                    ChangeMode(UiMode.Pan);
                    break;
                case "tbBtnZoom":
                    ChangeMode(UiMode.Zoom);
                    break;
                case "tbBtnMove":
                    ChangeMode(UiMode.Move);
                    break;
                case "tbBtnAssign":
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
        }

        private void ChangeSnackbar(UiMode mode) {
            switch (mode) {
                case UiMode.Default:
                    snackbar.Icon = Icons.Select;
                    snackbar.SupportingText = "Select";
                    break;
                case UiMode.Control:
                    snackbar.Icon = Icons.Control;
                    snackbar.SupportingText = "Select";
                    break;
                case UiMode.Add:
                    snackbar.Icon = Icons.Add;
                    snackbar.SupportingText = "Add";
                    break;
                case UiMode.Remove:
                    snackbar.Icon = Icons.Remove;
                    snackbar.SupportingText = "Remove";
                    break;
                case UiMode.Pan:
                    snackbar.Icon = Icons.Pan;
                    snackbar.SupportingText = "Pan";
                    break;
                case UiMode.Zoom:
                    snackbar.Icon = Icons.Zoom;
                    snackbar.SupportingText = "Zoom";
                    break;
                case UiMode.Move:
                    snackbar.Icon = Icons.Move;
                    snackbar.SupportingText = "Move";
                    break;
                case UiMode.Assign:
                    snackbar.Icon = Icons.Assign;
                    snackbar.SupportingText = "Assign";
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
