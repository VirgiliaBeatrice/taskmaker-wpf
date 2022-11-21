﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Views {
    /// <summary>
    /// Interaction logic for TargetsPanel.xaml
    /// </summary>
    public partial class TargetsPanel : UserControl {
        public TargetsPanel() {
            InitializeComponent();
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            var tb = sender as TextBox;

            if (e.Key == Key.Enter) {
                var binding = BindingOperations.GetBindingExpression(tb, TextBox.TextProperty);

                if (binding != null) {
                    binding.UpdateSource();
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            System.Windows.MessageBox.Show("Updated.", "Notification");
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e) {
            var popup = FindName("popup0") as Popup;

            if (popup != null) {
                popup.IsOpen = !popup.IsOpen;
            }
        }

        private void lbMaps_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count != 0) {
                var selectedMap = e.AddedItems[0] as NLinearMapState_v1;
                var listbox = sender as ListBox;
                var vm = listbox.DataContext as RegionControlUIViewModel;

                vm.SelectedMap = selectedMap;
            }

            if (e.RemovedItems.Count != 0) {
                var selectedMap = e.RemovedItems[0] as NLinearMapState_v1;
                var listbox = sender as ListBox;
                var vm = listbox.DataContext as RegionControlUIViewModel;

                vm.SelectedMap = null; ;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            var vm = (sender as Button).DataContext as RegionControlUIViewModel;

            vm.InvalidateValidPorts();
        }
    }

    public class ISelectableTargetConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return targetType + $"[{value}]";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
