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
using System.Windows.Shapes;
using System.Windows.Media;
using Rectangle = System.Windows.Shapes.Rectangle;
using System.Diagnostics;

namespace taskmaker_wpf.Views.Widget {
    public class NodeRelation {
        public object Master { get; set; }
        public object[] Remainder { get; set; }
        public bool HasValue { get; set; } = false;
    }

    public struct NodeInfo {
        public int UiId { get; set; }
        public int NodeId { get; set; }
    }

    public class Helper {
        static public NodeInfo[][] a => GetCombinations(Test());

        static public NodeInfo[][] Test() {
            return new NodeInfo[][] {
                Enumerable.Range(0, 2).Select(e => new NodeInfo() {NodeId = e, UiId = 0}).ToArray(),
                Enumerable.Range(0, 3).Select(e => new NodeInfo() {NodeId = e, UiId = 1}).ToArray(),
                Enumerable.Range(0, 3).Select(e => new NodeInfo() {NodeId = e, UiId = 2}).ToArray(),
            };
        }

        static public NodeInfo[][] Calc(NodeInfo[] a, NodeInfo[] b) {
            var result = new List<NodeInfo[]>();

            for (int i = 0; i < a.Length; i++) {
                for (int j = 0; j < b.Length; j++) {
                    result.Add(new NodeInfo[] { a[i], b[j] });
                }
            }

            return result.ToArray();
        }

        static public NodeInfo[][] Calc(NodeInfo[] a, NodeInfo[][] b) {
            var result = new List<NodeInfo[]>();

            for (int i = 0; i < a.Length; i++) {
                for (int j = 0; j < b.Length; j++) {
                    result.Add(new NodeInfo[] { a[i] }.Concat(b[j]).ToArray());
                }
            }

            return result.ToArray();
        }

        static public NodeInfo[][] GetCombinations(IEnumerable<NodeInfo[]> nodeSets) {
            var a = nodeSets.Take(1).First();
            var b = nodeSets.Skip(1).ToArray();

            NodeInfo[][] result;

            if (b.Count() > 1) {
                result = Calc(a, GetCombinations(b));
            }
            else {
                result = Calc(a, b.First());
            }

            return result;
        }
    }


    public class NodeRelationViewer : ContentControl {

        public IEnumerable NodeRelations {
            get { return (IEnumerable)GetValue(NodeRelationsProperty); }
            set { SetValue(NodeRelationsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NodeRelationsProperty =
            DependencyProperty.Register("NodeRelations", typeof(IEnumerable), typeof(NodeRelationViewer), new FrameworkPropertyMetadata(new NodeRelation[0], OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            if (d is NodeRelationViewer viewer) {
                var grid = new Grid();
                //var items = viewer.NodeRelations.Cast<NodeRelation>();
                var items = new NodeRelation[] {
                    new NodeRelation { HasValue = false },
                    new NodeRelation { HasValue = true },
                    new NodeRelation { HasValue = true },
                };
                var color = Brushes.DarkRed;

                Func<Brush, double, Brush> withOpacity = (b, o) => {
                    var c = b.Clone();
                    c.Opacity = o;
                    return c;
                };

                for (int i = 0; i < items.Count(); i++) {
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                    var block = new Rectangle() {
                        Width = 10,
                        Height = 10,
                        //Fill = Brushes.Gray,
                        Margin = new Thickness(1),
                        Fill = items.ElementAt(i).HasValue ? Brushes.Red : withOpacity(Brushes.Red, 0.3),
                    };

                    block.ToolTip = "ControlUi[x]-Node[y]";
                    block.MouseEnter += (o, ev) => {
                        Console.WriteLine("hover");
                    };

                    Grid.SetColumn(block, i);

                    grid.Children.Add(block);
                }


                viewer.Content = grid;

            }
        }

        public NodeRelationViewer() {

        }
    }

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

            if (d is MultiView control) {
                control.Layout();
            }

            //if (DesignerProperties.GetIsInDesignMode(d) && d is MultiView control) {
            //    control.Layout();
            //}
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
            if (ItemsSource == null) return;

            var items = ItemsSource.Cast<object>().ToList();
            var grid = new Grid();

            if (items.Count == 1) {
                var widget = new ComplexWidget { };

                BindingOperations.SetBinding(
                    widget,
                    ComplexWidget.UiStateProperty,
                    new Binding() {
                        Source = items.First()
                    });

                grid.Children.Add(widget);
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

                    //var item = new Button() {
                    //    Content = $"TEST{q}{r}",
                    //    Margin = new Thickness(2),

                    //};
                    var widget = new ComplexWidget {
                        Margin = new Thickness(2),
                        Background = Brushes.LightGray,
                    };
                    var textblock = new TextBlock {
                        Text = "Title",
                        FontSize = 42,
                        Foreground = Brushes.DimGray,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,

                    };

                    widget.SetBinding(ComplexWidget.UiStateProperty,
                        new Binding() {
                            Source = items.First()
                        });

                    //BindingOperations.SetBinding(
                    //    widget,
                    //    ComplexWidget.UiStateProperty,
                    //    new Binding() {
                    //        Source = items.First()
                    //    });

                    Grid.SetColumn(widget, r);
                    Grid.SetRow(widget, q);

                    Grid.SetColumn(textblock, r);
                    Grid.SetRow(textblock, q);

                    grid.Children.Add(widget);
                    grid.Children.Add(textblock);
                }
            }

            //BindingOperations.SetBinding(
            //    grid,
            //    )
            ((ScrollViewer)Content).Content = grid;
        }

        public void NotifyCombination() {

        }
    }
}
