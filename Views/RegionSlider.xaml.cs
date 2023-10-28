using GongSolutions.Wpf.DragDrop;
using Prism.Regions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Windows.Media;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Views {
    /// <summary>
    /// Interaction logic for RegionSlider.xaml
    /// </summary>
    public partial class RegionSlider : UserControl, INavigationAware {
        public RegionSlider() {
            Focusable = true;
            InitializeComponent();
            Background = new SolidColorBrush(Colors.Transparent);

            var dragHandler = new MotorControllersDragHandler();
            GongSolutions.Wpf.DragDrop.DragDrop.SetDragHandler(DragableControl, dragHandler);


            // TODO: bug
            //WeakReferenceMessenger.Default.Register<MotorValueUpdatedMessage>(this, (r, m) => {

            //});
            KeyDown += Region_KeyDown;
            KeyUp += Region_KeyUp;
            MouseDoubleClick += (_, _) => {
                Focus();
            };
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext) {
        }

        private void Region_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                // Update border thickness for all items
                foreach (var item in DragableControl.Items) {
                    var container = DragableControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                    var border = FindVisualChild<Border>(container);

                    if (border != null) {
                        border.BorderThickness = new Thickness(1);
                        border.BorderBrush = new SolidColorBrush(Colors.DimGray);
                    }
                }
            }
            else if (e.Key == Key.F5) {
                var vm = DataContext as RegionSliderViewModel;

                vm.FetchThis();
            }
        }

        private void Region_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                // Reset border thickness for all items
                foreach (var item in DragableControl.Items) {
                    var container = DragableControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                    var border = FindVisualChild<Border>(container);
                    if (border != null) {
                        border.BorderThickness = new Thickness(0);
                    }
                }
            }
        }

        private void ItemsControl_PreviewDragOver(object sender, DragEventArgs e) {
            // Get the target item
            var targetItem = (e.OriginalSource as TextBlock)?.DataContext;
            if (targetItem != null) {
                var container = DragableControl.ItemContainerGenerator.ContainerFromItem(targetItem) as FrameworkElement;
                var border = FindVisualChild<Border>(container);
                if (border != null) {
                    // Apply DropShadowEffect to the target item's border
                    border.Effect = new DropShadowEffect {
                        Color = Colors.Black,
                        Direction = 320,
                        ShadowDepth = 5,
                        BlurRadius = 8
                    };
                }
            }
        }

        // Helper method to find a child control by name
        public static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject {
            if (depObj != null) {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T) {
                        return (T)child;
                    }

                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null) return childItem;
                }
            }
            return null;
        }


    }

    public class MotorControllersDragHandler : DefaultDragHandler {
        public override bool CanStartDrag(IDragInfo dragInfo) {
            return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        }
    }
}
