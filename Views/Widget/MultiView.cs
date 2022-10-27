using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace taskmaker_wpf.Views.Widget {
    public class MultiView : ContentControl {



        public IEnumerable ItemsSource {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MultiView), new FrameworkPropertyMetadata(new object[0], OnPropertyChanged));



        public int MaxColumnCount {
            get { return (int)GetValue(MaxColumnCountProperty); }
            set { SetValue(MaxColumnCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxRowCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxColumnCountProperty =
            DependencyProperty.Register("MaxColumnCount", typeof(int), typeof(MultiView), new FrameworkPropertyMetadata(2));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {

            if (DesignerProperties.GetIsInDesignMode(d) && d is MultiView control) {
                control.Layout();
            }
        }

        public MultiView() : base() {
            var scroll = new ScrollViewer() {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            Content = scroll;
            //AddLogicalChild(scroll);
        }

        public void Layout() {
            var items = ItemsSource.Cast<object>().ToList();
            

            var grid = new Grid();

            if (items.Count == 1) {
                var item = new Button() {
                    Content = $"TEST{0}"
                };

                grid.Children.Add(item);
            }
            else if (items.Count == 0) {
                var textblock = new TextBlock {
                    Text = "NULL",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(2),
                    
                };

                grid.Children.Add(textblock);
            }
            else {
                int remainder, quotient = Math.DivRem(items.Count, MaxColumnCount, out remainder);

                for (int i = 0; i < (remainder == 0 ? quotient : quotient + 1); i++) {
                    grid.RowDefinitions.Add(new RowDefinition() { MinHeight = 400 });
                }

                for (int i = 0; i < MaxColumnCount; i++) {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                for (int i = 0; i < items.Count; i++) {
                    int r, q = Math.DivRem(i, MaxColumnCount, out r);

                    var item = new Button() {
                        Content = $"TEST{q}{r}",
                        Margin = new Thickness(2),

                    };

                    Grid.SetColumn(item, r);
                    Grid.SetRow(item, q);

                    grid.Children.Add(item);
                }
            }

            //BindingOperations.SetBinding(
            //    grid,
            //    )
            ((ScrollViewer)Content).Content = grid;
        }
    }
}
