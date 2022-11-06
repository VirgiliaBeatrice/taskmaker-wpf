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
using System.Drawing.Imaging;

namespace taskmaker_wpf.Views.Widget {
    //public class NodeRelation {
    //    public object Master { get; set; }
    //    public object[] Remainder { get; set; }
    //    public bool HasValue { get; set; } = false;
    //}

    public class NodeRelation {
        public bool HasValue { get; set; }
        private readonly NodeInfo[] _nodes;

        public NodeRelation(NodeInfo[] nodes) {
            _nodes = nodes;
        }

        public NodeInfo this[int index] {
            get => _nodes[index];
        }

        public override string ToString() {
            return $"[{string.Join(",", _nodes.Select(e => $"{e.UiId}({e.NodeId})"))}]";
        }
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
        static SolidColorBrush[] ColorPalette = new SolidColorBrush[] {
            (SolidColorBrush)new BrushConverter().ConvertFrom("#A7226E"),
            (SolidColorBrush)new BrushConverter().ConvertFrom("#EC2049"),
            (SolidColorBrush)new BrushConverter().ConvertFrom("#F26B38"),
            (SolidColorBrush)new BrushConverter().ConvertFrom("#F7DB4F"),
            (SolidColorBrush)new BrushConverter().ConvertFrom("#2F9599"),
        };


        public IEnumerable<NodeInfo> SelectedNodes {
            get { return (IEnumerable<NodeInfo>)GetValue(SelectedNodesProperty); }
            set { SetValue(SelectedNodesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedNodes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedNodesProperty =
            DependencyProperty.Register("SelectedNodes", typeof(IEnumerable<NodeInfo>), typeof(NodeRelationViewer), new PropertyMetadata(new NodeInfo[0]));



        public IEnumerable<NodeRelation> PossiblePairs {
            get { return (IEnumerable<NodeRelation>)GetValue(PossiblePairsProperty); }
            set { SetValue(PossiblePairsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PossiblePairs.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PossiblePairsProperty =
            DependencyProperty.Register("PossiblePairs", typeof(IEnumerable<NodeRelation>), typeof(NodeRelationViewer), new PropertyMetadata(new NodeRelation[0]));


        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            return;
            if (d is NodeRelationViewer viewer) {
                var grid = new Grid();
                //var items = viewer.NodeRelations.Cast<NodeRelation>();
                var items = new NodeRelation[] {
                    //new NodeRelation { HasValue = false },
                    //new NodeRelation { HasValue = true },
                    //new NodeRelation { HasValue = true },
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
            Layout();
        }

        private (IEnumerable<NodeInfo>, IEnumerable<NodeRelation>) PrepareSampleData() {
            var a = Enumerable.Range(0, 2).Select(e => new NodeInfo { NodeId = 0 , UiId = e });
            var b = Enumerable.Range(0, 3).Select(e => new NodeInfo { NodeId = 0, UiId = e });
            var c = Enumerable.Range(0, 10).Select(e => new NodeRelation(b.ToArray()) { HasValue = e % 2 == 1 });

            return (a, c);
        }

        public void Layout() {
            IEnumerable<NodeInfo> a;
            IEnumerable<NodeRelation> b;

            //if (DesignerProperties.GetIsInDesignMode(this)) {
            //    (a, b) = PrepareSampleData();
            //}
            //else {
            //    a = SelectedNodes;
            //    b = PossiblePairs;
            //}

            (a, b) = PrepareSampleData();

            //var a = Enumerable.Range(0, 2).Select(e => new NodeInfo { NodeId = e });
            //var b = Enumerable.Range(0, 10).Select(e => new NodeRelation(new NodeInfo[3]) { HasValue = e % 2 == 1 });

            var grid = new Grid() { };

            grid.ColumnDefinitions.Add(new ColumnDefinition() {
                Width = GridLength.Auto,
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition() {
                Width = GridLength.Auto,
            }); 
            grid.ColumnDefinitions.Add(new ColumnDefinition() {
                Width = GridLength.Auto,
            });

            var panel0 = new StackPanel { 
                Orientation = Orientation.Horizontal,
            };
            var panel1 = new StackPanel {
                Orientation = Orientation.Horizontal,
            };
            var split = new Line { X1 = 0, X2 = 0, Y1 = 0, Y2 = 20,
                Stroke = new SolidColorBrush(Colors.Black),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(2),
            };

            for (int i = 0; i < a.Count(); i++) {
                var circle = new Ellipse() {
                    Width = 20,
                    Height = 20,
                    Fill = ColorPalette[i],
                    Margin = new Thickness(2),
                    ToolTip = $"{a.ElementAt(i).UiId}({a.ElementAt(i).NodeId})"
                };

                panel0.Children.Add(circle);
            }

            foreach (var item in b) {
                var rect = new Rectangle() {
                    Width = 20,
                    Height = 20,
                    Fill = ColorPalette[4],
                    Margin = new Thickness(2),
                    Opacity = item.HasValue ? 1.0 : 0.6,
                    ToolTip = item.ToString()
                };

                panel1.Children.Add(rect);
            }

            Grid.SetColumn(panel0, 0);
            Grid.SetColumn(split, 1);
            Grid.SetColumn(panel1, 2);

            grid.Children.Add(panel0);
            grid.Children.Add(split);
            grid.Children.Add(panel1);

            var scroll = new ScrollViewer {
                Width = Width,
                Content = grid,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            Content = scroll;
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
                    new Binding {
                        Source = items.First()
                    });

                //BindingOperations.SetBinding(
                //    widget,
                //    ComplexWidget.UiStateProperty,
                //    new Binding() {
                //        Source = items.First()
                //    });

                grid.Children.Add(widget);
                grid.Children.Add(textblock);
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
