﻿using SkiaSharp;
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


namespace taskmaker_wpf.Views {
    /// <summary>
    /// Interaction logic for RegionControlUI.xaml
    /// </summary>
    public partial class RegionControlUI : UserControl {
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
    }
}
