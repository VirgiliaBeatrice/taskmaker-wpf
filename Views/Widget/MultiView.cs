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
        public Point Location { get; set; }
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
        static public SolidColorBrush[] ColorPalette = new SolidColorBrush[] {
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

    public struct SimplexInfo {
        public Point[] Points { get; set; }
    }

    public class UiController : Canvas {
        public UiController() {
            var nodes = new NodeInfo[] {
                new NodeInfo { NodeId = 0, UiId = 0, Location = new Point(0, 0) },
                new NodeInfo { NodeId = 1, UiId = 0, Location = new Point(10, 40) },
                new NodeInfo { NodeId = 2, UiId = 0, Location = new Point(20, 80) },
                new NodeInfo { NodeId = 3, UiId = 0, Location = new Point(30, 100) },
            };

            foreach (var node in nodes) {
                var nodeWidget = new NodeWidget_v1 {
                    Node = node,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };

                SetLeft(nodeWidget, node.Location.X - 20 / 2);
                SetTop(nodeWidget, node.Location.Y - 20 / 2);

                Children.Add(nodeWidget);
            }


            var simplex = new SimplexWidget_v1();

            SetLeft(simplex, 30);
            SetTop(simplex, 30);

            Children.Add(new SimplexWidget_v1());
            Children.Add(simplex);

        }

        private Path MakeSimplex() {
            var simplices = new SimplexInfo[] {
                new SimplexInfo { Points = new Point[] {new Point(200, 20), new Point(100, 150), new Point(300, 150)}},
            };

            var pathGeo = new PathGeometry();
            var pathFig = new PathFigure {
                StartPoint = simplices[0].Points[0],
            };

            pathGeo.Figures.Add(pathFig);
            pathFig.Segments.Add(new LineSegment { Point = simplices[0].Points[1] });
            pathFig.Segments.Add(new LineSegment { Point = simplices[0].Points[2] });


            var path = new Path {
                Fill = new SolidColorBrush(Colors.LightBlue),
                Stroke = new SolidColorBrush(Colors.LightGreen),
                Stretch = Stretch.None,
                StrokeThickness = 1,
                Data = pathGeo
            };

            return path;
        }
    }

    public class VoronoiShape : ContentControl {
        public VoronoiShape() {
            var points = new Point[] {
                new Point(200, 20),
                new Point(100, 150),
                new Point(300, 150)
            };

            if (points.Length == 3) {
                var radius = (points[1] - points[0]).Length;
                var o = points[1];
                var p0 = points[0];
                var p1 = points[2];

                var p0o = (p0 - o);
                var p1o = (p1 - o);
                var dotProd = (p0o.X * p1o.X) + (p0o.Y * p1o.Y);
                var alpha = Math.Abs(Math.Acos(dotProd / (p0o.Length * p1o.Length)));

                var midLen = (float)Math.Tan(alpha / 2.0f) * Math.Abs(p0o.Length);

                var op0 = Point.Normalize(o - p0);
                var midP0 = SKMatrix.CreateRotation((float)(Math.PI * 90.0 / 180.0)).MapVector(op0);
                midP0.X *= midLen;
                midP0.Y *= midLen;

                var mid = p0 + midP0;

                _shape.MoveTo(o);
                _shape.LineTo(p0);
                _shape.ArcTo(mid, p1, radius);
                _shape.LineTo(p1);
                _shape.Close();
            }
        }
    }

    public class SimplexWidget_v1 : ContentControl {


        public IEnumerable<SimplexInfo> Simplices {
            get { return (IEnumerable<SimplexInfo>)GetValue(SimplicesProperty); }
            set { SetValue(SimplicesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Simplices.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SimplicesProperty =
            DependencyProperty.Register("Simplices", typeof(IEnumerable<SimplexInfo>), typeof(SimplexWidget_v1), new PropertyMetadata(new SimplexInfo[0]));

        public SimplexWidget_v1() {
            BorderBrush = Brushes.Black;
            BorderThickness = new Thickness(1);

            var simplices = new SimplexInfo[] {
                new SimplexInfo { Points = new Point[] {new Point(200, 20), new Point(100, 150), new Point(300, 150)}},
            };

            var pathGeo = new PathGeometry();
            var pathFig = new PathFigure {
                StartPoint = simplices[0].Points[0],
            };

            pathGeo.Figures.Add(pathFig);
            pathFig.Segments.Add(new LineSegment { Point = simplices[0].Points[1] });
            pathFig.Segments.Add(new LineSegment { Point = simplices[0].Points[2] });


            var path = new Path {
                Fill = new SolidColorBrush(Colors.LightBlue),
                Stroke = new SolidColorBrush(Colors.LightGreen),
                Stretch = Stretch.None,
                StrokeThickness = 1,
                Data = pathGeo
            };

            Content = path;
        }
    }

    public class NodeWidget_v1 : ContentControl {


        public NodeInfo Node {
            get { return (NodeInfo)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Node.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NodeProperty =
            DependencyProperty.Register("Node", typeof(NodeInfo), typeof(NodeWidget_v1), new PropertyMetadata(new NodeInfo(), OnPropertyChanged));



        public NodeWidget_v1() {
            Invalidate();
        }

        static public void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is NodeWidget_v1 widget) {
                widget.Invalidate();
            }
        }

        public void Invalidate() {
            var node = new NodeInfo {
                NodeId = 0,
                UiId = 0
            };

            var circle = new Ellipse {
                Width = 20,
                Height = 20,
                Stroke = Brushes.Black,
                Fill = NodeRelationViewer.ColorPalette[node.UiId],
                StrokeThickness = 2.0
            };

            ToolTip = $"Node[{node.NodeId}]-(x,y)";

            Content = circle;
        }
    }
}
