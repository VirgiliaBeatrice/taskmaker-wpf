using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using Numpy;
using SharpVectors.Converters;
using SharpVectors.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using taskmaker_wpf.ViewModels;
using Unity;

namespace taskmaker_wpf.Views.Widget {

    public enum UiElementState {
        Default = 0,
        Hover,
        Focus,
        Selected,
        Activated,
        Pressed,
        Dragged
    }

    public enum UiMode {
        Default = 0,
        Add,
        Remove,
        Move,
        Assign,
        Build,
        Control,
        Drag,
        Pan,
        Zoom,
        Reset,
    }

    [Serializable]
    public struct MapEntry {
        public string FormattedIdsString => $"({string.Join(",", IDs)})";

        public int[] IDs { get; set; } = new int[0];

        public int[] Indices { get; set; } = new int[0];

        public bool IsInvalid => Value.Any(double.IsNaN);

        public double[] Value { get; set; } = new double[0];

        public MapEntry() { }
        public static bool operator !=(MapEntry left, MapEntry right) {
            return !(left == right);
        }

        public static bool operator ==(MapEntry left, MapEntry right) {
            return left.Equals(right);
        }

        public override bool Equals(object obj) {
            if (obj == null || this.GetType() != obj.GetType()) {
                return false;
            }

            var other = (MapEntry)obj;
            return Enumerable.SequenceEqual(this.Indices, other.Indices);
        }

        public override int GetHashCode() {
            return (Indices != null) ? Indices.Aggregate(0, (acc, val) => acc ^ val) : 0;
        }

        public override string ToString() {
            return $"[({string.Join(",", IDs)}) -> [{(IsInvalid ? "NaN" : "Set")}]]";
        }
    }

    public struct StateInfo<T> {
        public int Id { get; set; }
        public T Value { get; set; }
    }

    public static class Evelations {
        public static DropShadowEffect Lv1 = new DropShadowEffect {
            Color = Colors.Black,
            Direction = 270,
            ShadowDepth = 1,
            BlurRadius = 8,
            Opacity = 0.32,
        };

        public static DropShadowEffect Lv3 = new DropShadowEffect {
            Color = Colors.Black,
            Direction = 270,
            ShadowDepth = 6,
            BlurRadius = 8,
            Opacity = 0.32,
        };
    }
    public static class LogicalTreeHelperExtensions {

        public static T FindAncestor<T>(DependencyObject dependencyObject) where T : class {
            var target = dependencyObject;

            do {
                target = LogicalTreeHelper.GetParent(target);
            }
            while (target != null && !(target is T));

            return target as T;
        }
    }

    public static class VisualTreeHelperExtensions {

        public static T FindAncestor<T>(DependencyObject dependencyObject)
            where T : class {
            DependencyObject target = dependencyObject;
            do {
                target = VisualTreeHelper.GetParent(target);
            }
            while (target != null && !(target is T));
            return target as T;
        }

        public static T FindParentByName<T>(DependencyObject child, string name) where T : FrameworkElement {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // If we reach the top of the visual tree, return null
            if (parentObject == null) return null;

            // Check if the parent object is of the specified type and matches the name
            if (parentObject is T parent && parent.Name == name) {
                return parent;
            }
            else {
                // Recursively call this function to check the next level up
                return FindParentByName<T>(parentObject, name);
            }
        }

        public static T FindParentOfType<T>(DependencyObject child) where T : DependencyObject {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // Return null if reached the end of the visual tree
            if (parentObject == null) return null;

            // Check if the parent is of the desired type
            if (parentObject is T parent) {
                return parent;
            }
            else {
                // Recursively call the function to move further up the visual tree
                return FindParentOfType<T>(parentObject);
            }
        }
    }
    public class MultiView : UserControl {
        private Grid _grid;
        private Border _scrim;

        private UiMode uiMode;

        public List<UiController> Controllers { get; set; } = new List<UiController>();

        public NodeShape[] SelectedNodes => Controllers.Select(c => c.SelectedNode).ToArray();

        public UiMode UiMode { get => uiMode; set => uiMode = value; }

        public MapEntryWidget Widget { get; init; }
        public SessionViewModel SessionViewModel => DataContext as SessionViewModel;

        public MultiView() : base() {
            // UserControl
            Background = new SolidColorBrush(Colors.Transparent);

            // Grid Container
            _grid = new Grid() {
                Name = "Multiview_Grid",
            };
            Content = _grid;

            Widget = new MapEntryWidget() {
                Visibility = Visibility.Collapsed
            };
            Panel.SetZIndex(Widget, 10);
            
            _scrim = new Border {
                CornerRadius = new CornerRadius(16),
                Background = new SolidColorBrush(Colors.Black),
                Opacity = 0.32,
                Visibility = Visibility.Collapsed
            };
            Panel.SetZIndex(_scrim, 5);

            _grid.Children.Add(Widget);
            _grid.Children.Add(_scrim);

            // Register messages from children
            WeakReferenceMessenger.Default.Register<UiSelectedNodeChangedMessage>(this, (r, m) => {
                var controller = m.Sender as UiController;
                var view = r as MultiView;
                var vm = DataContext as SessionViewModel;

                if (m.Node == null)
                    vm.ShowWidget = false;
                else {
                    vm.SelectedNodeStates = view.Controllers.Select(c => c.SelectedNode.State).ToArray();
                    vm.ShowWidget = true;
                }
            });

            WeakReferenceMessenger.Default.Register<UiControllerControlledMessage>(this, (r, m) => {
                (DataContext as SessionViewModel).Interpolate();
            });

            DataContextChanged += MultiView_DataContextChanged;
            //MouseLeftButtonDown += MultiView_MouseLeftButtonDown;
        }

        private void BindProperties() {
            Binding binding;
            binding = new Binding("MapEntryWidget") {
                Source = DataContext,
            };

            Widget.SetBinding(DataContextProperty, binding);
        }

        private void MultiView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var control = sender as MultiView;

            if (e.NewValue != null) {
                if (e.NewValue is SessionViewModel vm) {
                    vm.PropertyChanged += Vm_PropertyChanged;

                    control.Close();
                    control.Open();

                    // data binding
                    BindProperties();
                }
            }

            if (e.OldValue != null) {
                if (e.OldValue is SessionViewModel vm) {
                    vm.PropertyChanged -= Vm_PropertyChanged;
                }
            }
        }

        private void MultiView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var logger = LogManager.GetCurrentClassLogger();

            //logger.Debug("Click in here");
            var clickElement = e.OriginalSource as FrameworkElement;

            while(clickElement != null) {
                if (clickElement is MapEntryWidget) {
                    return;
                }

                clickElement = VisualTreeHelper.GetParent(clickElement) as FrameworkElement;
            }

            CloseWidget();

        }
        private void ShowWidget(bool v) {
            if (v) {
                Widget.Visibility = Visibility.Visible;
                _scrim.Visibility = Visibility.Visible;
            }
            else {
                Widget.Visibility = Visibility.Collapsed;
                _scrim.Visibility = Visibility.Collapsed;
            }
        }

        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var vm = (sender as SessionViewModel);

            if (e.PropertyName == nameof(SessionViewModel.Mode)) {
                uiMode = vm.Mode;

                foreach(var item in Controllers) {
                    item.UiMode = UiMode;
                }
            }
            else if (e.PropertyName == nameof(SessionViewModel.Uis)) {
                Close();
                Open();
            }
            else if (e.PropertyName == nameof(SessionViewModel.ShowWidget)) {
                if (vm.ShowWidget) {
                    OpenWidget();
                }
                else {
                    CloseWidget();
                }
            }
        }
        public void Close() {
            Controllers.Clear();
        }

        public void CloseWidget() {
            ShowWidget(false);
        }

        public void Layout() {
            // single
            Panel.SetZIndex(Controllers[0], 5);
            _grid.Children.Add(Controllers[0]);
        }

        public void Open() {
            foreach(var uiViewModel in (DataContext as SessionViewModel).Uis) {
                // 1. Open Ui
                var uiController = new UiController() {
                    //DataContext = uiViewModel,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(16),
                };

                Binding binding;

                binding = new Binding() {
                    Source = uiViewModel,
                };
                uiController.SetBinding(DataContextProperty, binding);

                // data binding

                // Register messages

                // TODO: potential memory leak because of unregistering
                Controllers.Add(uiController);
            }

            Layout();
        }
        public void OpenWidget() {
            if (UiMode == UiMode.Assign && SelectedNodes.All(v => v != null)) {
                ShowWidget(true);
            }
        }
        public async Task RequestMotorDialog() {
            if (UiMode == UiMode.Assign && SelectedNodes.All(v => v != null)) {
                // Send dialog message
                var result = await WeakReferenceMessenger.Default.Send(new DialogRequestMessage());

                if (result.Result == MessageBoxResult.OK) {
                    // Commit map entry
                    SessionViewModel?.SetValue(result.Value as double[]);
                }
            }
        }
    }
}