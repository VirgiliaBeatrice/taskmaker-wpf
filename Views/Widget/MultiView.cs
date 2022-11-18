using SharpVectors.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.XPath;
using taskmaker_wpf.ViewModels;
using taskmaker_wpf.Views.Widgets;
using static Unity.Storage.RegistrationSet;
using Rectangle = System.Windows.Shapes.Rectangle;

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

    public interface IState {
        void SetOverlay();
        void SetContainer();
        void SetFlag();
    }

    public abstract class StatefulWidget : ContentControl {
        public UiElementState State { get; set; }
        protected IState _state;

        public UIElement Container { get; set; }
        public UIElement Overlay { get; set; }

        public virtual void GoToState(UiElementState state) {
            State = state;

            InvalidateCustomVisual();
        }

        protected void InvalidateCustomVisual() {
            _state.SetOverlay();
            _state.SetContainer();
            _state.SetFlag();
        }
    }

    public abstract class BaseState : IState {
        public StatefulWidget Parent { get; set; }

        public BaseState(StatefulWidget parent) {
            Parent = parent;
        }

        public abstract void SetContainer();
        public abstract void SetOverlay();
        public abstract void SetFlag();
    }

    public class DefaultState : BaseState {
        public DefaultState(StatefulWidget parent) : base(parent) { }

        public override void SetContainer() {
            var grid = Parent.Container as Grid;

            // Set background color
            //grid.Background
        }

        public override void SetFlag() {
            var widget = Parent as RelationWidget;

            widget.IsSelected = !widget.IsSelected;
        }

        public override void SetOverlay() {
            var grid = Parent.Overlay as Grid;
            var overlay = grid.Children.OfType<Rectangle>().First();
            var icon = grid.Children.OfType<SvgIcon>().First();

            // Set color
            overlay.Fill = Brushes.White;
            // Set opacity
            overlay.Opacity = 0.0;

            icon.Visibility = Visibility.Hidden;
        }
    }

    public class HoverState : BaseState {
        public HoverState(StatefulWidget parent) : base(parent) { }

        public override void SetContainer() {
            var grid = Parent.Container as Grid;

            // Set color
            //grid.Background

            // Set opacity
        }

        public override void SetFlag() {

        }

        public override void SetOverlay() {
            var grid = Parent.Overlay as Grid;
            var overlay = grid.Children.OfType<Rectangle>().First();
            var icon = grid.Children.OfType<SvgIcon>().First();

            // Set color
            overlay.Fill = Brushes.White;
            // Set opacity
            overlay.Opacity = 0.08;

            icon.Visibility = Visibility.Hidden;
        }
    }

    public class SelectedState : BaseState {
        public SelectedState(StatefulWidget parent) : base(parent) {
        }

        public override void SetContainer() {
        }

        public override void SetFlag() {
            var widget = Parent as RelationWidget;

            widget.IsSelected = !widget.IsSelected;
        }

        public override void SetOverlay() {
            var grid = Parent.Overlay as Grid;
            var overlay = grid.Children.OfType<Rectangle>().First();
            var icon = grid.Children.OfType<SvgIcon>().First();

            // Set color
            overlay.Fill = Brushes.Red;
            // Set opacity
            overlay.Opacity = 0.12;

            icon.Visibility = Visibility.Visible;
        }
    }

    public class PressedState : BaseState {
        public PressedState(StatefulWidget parent) : base(parent) { }

        public override void SetContainer() {
            var grid = Parent.Container as Grid;

            // Set color
            //grid.Background

            // Set opacity
        }

        public override void SetFlag() {
        }

        public override void SetOverlay() {
            var grid = Parent.Overlay as Grid;
            var overlay = grid.Children.OfType<Rectangle>().First();
            var icon = grid.Children.OfType<SvgIcon>().First();

            // Set color
            overlay.Fill = Brushes.DarkRed;
            // Set opacity
            overlay.Opacity = 0.12;
            
            icon.Visibility = Visibility.Hidden;

        }
    }


    public class RelationWidget : StatefulWidget {
        public bool IsSelected { get; set; } = false; 

        public RelationWidget() {
            var content = new Rectangle {
                Width = 20,
                Height = 20,
                Fill = new SolidColorBrush(ColorManager.Palette.Last()),
            };

            Container = new Grid { };
            //Overlay = new Border { };

            var overlayContainer = new Grid();

            var overlay = new Rectangle {
                Width = 20,
                Height = 20,
                Fill = new SolidColorBrush(Colors.White),
                Opacity = 0.0
            };
            var icon = new SvgIcon {
                Width = 12,
                Height = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                UriSource = new Uri(@"icons/done.svg", UriKind.Relative),
                Fill = new SolidColorBrush(Colors.White),
                Visibility = Visibility.Hidden,
            };

            overlayContainer.Children.Add(overlay);
            overlayContainer.Children.Add(icon);

            Overlay = overlayContainer;

            (Container as Grid).Children.Add(content);
            (Container as Grid).Children.Add(Overlay);

            Content = Container;

            MouseEnter += (s, e) => {
                var widget = s as RelationWidget;

                if (widget.State == UiElementState.Default) {
                    GoToState(UiElementState.Hover);
                }
            };

            MouseLeave += (s, e) => {
                var widget = s as RelationWidget;

                if (widget.State == UiElementState.Hover) {
                    GoToState(UiElementState.Default);
                }
            };

            MouseLeftButtonDown += (s, e) => {
                var widget = s as RelationWidget;

                GoToState(UiElementState.Pressed);
            };

            MouseLeftButtonUp += (s, e) => {
                var widget = s as RelationWidget;

                if (widget.State == UiElementState.Pressed) {
                    if (widget.IsSelected) {
                        GoToState(UiElementState.Default);
                    }
                    else {
                        GoToState(UiElementState.Selected);
                    }
                }
                else if (widget.State == UiElementState.Selected) {
                    GoToState(UiElementState.Default);
                }
            };
        }

        public override void GoToState(UiElementState state) {
            switch (state) {
                case UiElementState.Default:
                    _state = new DefaultState(this);
                    break;
                case UiElementState.Hover:
                    _state = new HoverState(this);
                    break;
                case UiElementState.Focus:
                    break;
                case UiElementState.Selected:
                    _state = new SelectedState(this);
                    break;
                case UiElementState.Activated:
                    break;
                case UiElementState.Pressed:
                    _state = new PressedState(this);
                    break;
                case UiElementState.Dragged:
                    break;
                default:
                    break;
            }

            base.GoToState(state);
        }


    }

    public class ColorManager {
        // https://abierre.com/article/5c18da1189599b92b890ecc1
        static public double TintFactor = 0.25;

        static public Color[] Palette = {
            Colors.Red,
            Colors.Pink,
            Colors.Purple,
            Colors.Indigo,
            Colors.Blue,
            Colors.LightBlue,
            Colors.Cyan,
            Colors.Teal,
            Colors.Green,
            Colors.LightGreen,
            Colors.Lime,
            Colors.Yellow,
            Colors.Orange,
            Colors.Brown,
            Colors.Gray,
        };

        static public Color GetTintedColor(Color color, int level) {
            return new Color {
                R = (byte)(color.R + (255 - color.R) * (TintFactor * level)),
                G = (byte)(color.G + (255 - color.G) * (TintFactor * level)),
                B = (byte)(color.B + (255 - color.B) * (TintFactor * level)),
                A = 255
            };
        }

        public static double GetRelativeLuminance(Color color) {
            var r = (double)color.R / 255.0;
            var g = (double)color.G / 255.0;
            var b = (double)color.B / 255.0;

            double R, G, B;

            if (r <= 0.03928)
                R = r / 12.92;
            else
                R = Math.Pow(((r + 0.055) / 1.055), 2.4);

            if (g <= 0.03928)
                G = g / 12.92;
            else
                G = Math.Pow(((g + 0.055) / 1.055), 2.4);

            if (b <= 0.03928)
                B = b / 12.92;
            else
                B = Math.Pow(((b + 0.055) / 1.055), 2.4);

            return 0.2126 * R + 0.7152 * G + 0.0722 * B;
        }

        public static double GetConstrastRatio(Color color0, Color color1) {
            var l0 = GetRelativeLuminance(color0);
            var l1 = GetRelativeLuminance(color1);

            var luminH = l0 > l1 ? l0 : l1;
            var luminL = l0 > l1 ? l1 : l0;

            return (luminH + 0.05) / (luminL + 0.05);
        }

        public static Color GetComplemetaryColor(Color color) {
            return new Color() {
                R = (byte)(255 - color.R),
                G = (byte)(255 - color.G),
                B = (byte)(255 - color.B),
                A = 255
            };
        }
    }


    public class NodeRelationViewer : UserControl {
        //static public SolidColorBrush[] ColorPalette = new SolidColorBrush[] {
        //    (SolidColorBrush)new BrushConverter().ConvertFrom("#A7226E"),
        //    (SolidColorBrush)new BrushConverter().ConvertFrom("#EC2049"),
        //    (SolidColorBrush)new BrushConverter().ConvertFrom("#F26B38"),
        //    (SolidColorBrush)new BrushConverter().ConvertFrom("#F7DB4F"),
        //    (SolidColorBrush)new BrushConverter().ConvertFrom("#2F9599"),
        //};

        public IEnumerable<NodeInfo> Locked {
            get { return (IEnumerable<NodeInfo>)GetValue(LockedProperty); }
            set { SetValue(LockedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Locked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LockedProperty =
            DependencyProperty.Register("Locked", typeof(IEnumerable<NodeInfo>), typeof(NodeRelationViewer), new PropertyMetadata(new NodeInfo[0]));


        public IEnumerable<NodeRelation> Combinations {
            get { return (IEnumerable<NodeRelation>)GetValue(CombinationsProperty); }
            set { SetValue(CombinationsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Combinations.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CombinationsProperty =
            DependencyProperty.Register("Combinations", typeof(IEnumerable<NodeRelation>), typeof(NodeRelationViewer), new PropertyMetadata(new NodeRelation[0]));


        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            if (d is NodeRelationViewer viewer) {
                //viewer.Layout();

                if ((bool)args.NewValue == true)
                    viewer.Visibility= Visibility.Visible;
                else
                    viewer.Visibility= Visibility.Collapsed;
            }
        }

        public NodeRelationViewer(UIElement target) {
            Layout();
        }

        public void InvalidateState() {
            Layout();
        }

        public void Layout() {
            var a = Locked;
            var b = Combinations;

            var grid = new Grid() { };

            grid.ColumnDefinitions.Add(new ColumnDefinition() {
                Width = GridLength.Auto,
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition() {
                Width = GridLength.Auto,
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition() {
                Width = GridLength.Auto,
                MinWidth = 20
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
                var fill = ColorManager.GetTintedColor(ColorManager.Palette[a.ElementAt(i).UiId], 2);

                var circle = new Ellipse() {
                    Width = 20,
                    Height = 20,
                    Fill = new SolidColorBrush(fill),
                    Margin = new Thickness(2),
                    ToolTip = $"{a.ElementAt(i).UiId}({a.ElementAt(i).NodeId})"
                };

                panel0.Children.Add(circle);
            }

            foreach (var item in b) {
                var rect = new RelationWidget {
                    Margin = new Thickness(2),
                    ToolTip = item.ToString(),
                };
                //var rect = new Rectangle() {
                //    Width = 20,
                //    Height = 20,
                //    Fill = ColorPalette[4],
                //    Margin = new Thickness(2),
                //    //Opacity = item.HasValue ? 1.0 : 0.6,
                //    ToolTip = item.ToString()
                //};

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
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Focusable = false,
                HorizontalAlignment= HorizontalAlignment.Stretch,
            };

            Content = scroll;
        }

    }

    public class MultiView : UserControl {
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
            if (d is MultiView view) {

                view.Layout();

                view.InvalidateViewer();


                //if (DesignerProperties.GetIsInDesignMode(d) && d is MultiView control) {
                //    control.Layout();
                //}
            }
        }

        public void InvalidateViewer() {
            var locked = new List<NodeInfo>();
            var unlocked = new List<NodeInfo[]>();

            foreach (var c in Controllers) {
                if (c.SelectedNode == null)
                    unlocked.Add(c.NodeInfos);
                //view.Candidates.Add(c.UiState.Id, c.UiState.Nodes.Select(e => new NodeInfo { Location = e.Value, NodeId = e.Id, UiId = c.UiState.Id }).ToArray());
                else
                    locked.Add(c.SelectedNode.Node);
                //view.Candidates.Add(c.UiState.Id, new NodeInfo[] { c.SelectedNode.Node });
            }

            if (locked.Count > 0) {
                var temp = locked.Select(e => new NodeInfo[] { e }).Concat(unlocked).ToArray();
                var combinations = Helper.GetCombinations(temp);

                Viewer.Locked = locked;
                Viewer.Combinations = combinations.Select(e => new NodeRelation(e));

                Viewer.Visibility = Visibility.Visible;
                Viewer.InvalidateState();
            }
            else {
                Viewer.Visibility = Visibility.Collapsed;
                Viewer.InvalidateState();
            }
        }

        //public Dictionary<int, NodeInfo[]> Candidates { get; set; } = new Dictionary<int, NodeInfo[]>();

        public List<UiController> Controllers = new List<UiController>();

        public NodeRelationViewer Viewer { get; set; }

        public MultiView() : base() {
            var grid = new Grid();
            //var icon = new SvgIcon() {
            //    Width = 200,
            //    Height = 200,
            //    UriSource = new Uri(@"icons/done.svg", UriKind.Relative),
            //};

            var scroll = new ScrollViewer() {
                Focusable= false,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            grid.Children.Add(scroll);
            //grid.Children.Add(icon);

            Content = grid;
            //AddLogicalChild(scroll);
        }

        public void Layout() {
            Viewer = new NodeRelationViewer(this) {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            Panel.SetZIndex(Viewer, 10);

            if (ItemsSource == null) return;

            var items = ItemsSource.Cast<object>().ToList();
            var grid = new Grid() {
                Visibility = Visibility.Visible
            };

            var vm = DataContext as RegionControlUIViewModel;

            ((Grid)Content).Children.Clear();
            Controllers.Clear();

            if (items.Count == 1) {
                var ui = new UiController {
                    Margin = new Thickness(2),
                    Command = vm.UiCommand,
                };

                //var widget = new ComplexWidget {
                //    Margin = new Thickness(2),
                //    Background = Brushes.LightGray,
                //};

                var textblock = new TextBlock {
                    Text = "Title",
                    FontSize = 42,
                    Foreground = Brushes.DimGray,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,

                };

                ui.SetBinding(
                    UiController.UiStateProperty,
                    new Binding {
                        Source = items[0]
                    });

                grid.Children.Add(ui);
                grid.Children.Add(textblock);

                Controllers.Add(ui);
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

                    var ui = new UiController {
                        Margin = new Thickness(2),
                        Command = vm.UiCommand,
                    };
                    var textblock = new TextBlock {
                        Text = "Title",
                        FontSize = 42,
                        Foreground = Brushes.DimGray,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,

                    };

                    ui.SetBinding(
                        UiController.UiStateProperty,
                        new Binding() {
                            Source = items[i]
                        });

                    Grid.SetColumn(ui, r);
                    Grid.SetRow(ui, q);

                    Grid.SetColumn(textblock, r);
                    Grid.SetRow(textblock, q);

                    grid.Children.Add(ui);
                    grid.Children.Add(textblock);
                    
                    Controllers.Add(ui);
                }
            }

            //BindingOperations.SetBinding(
            //    grid,
            //    )



            ((Grid)Content).Children.Add(grid);
            ((Grid)Content).Children.Add(Viewer);
        }
    }

    public struct SimplexInfo {
        public Point[] Points { get; set; }
    }

    public enum UiMode {
        Default = 0,
        Add,
        Delete,
        Build,
        Trace,
        Pan,
        Zoom
    }

    public class CommandParameter {
        public string Type { get; set; }
        public object[] Payload { get; set; }
    }

    public class UiController : UserControl {

        internal static readonly DependencyPropertyKey SelectedNodePropertyKey = DependencyProperty.RegisterReadOnly("SelectedNode", typeof(NodeShape), typeof(UiController), new FrameworkPropertyMetadata(null, (PropertyChangedCallback)OnSelectedNodePropertyChanged));

        private static void OnSelectedNodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var parent = VisualTreeHelper.GetParent(d as Visual);

            while (parent != null && !typeof(MultiView).IsInstanceOfType(parent)) {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is MultiView v) {
                v.InvalidateViewer();
                Console.WriteLine("Some property changed.");
            }
        }

        public NodeShape SelectedNode => (NodeShape)GetValue(SelectedNodePropertyKey.DependencyProperty);

        public NodeInfo[] NodeInfos => UiState.Nodes.Select(e => new NodeInfo {
            Location = e.Value,
            NodeId = e.Id,
            UiId = UiState.Id
        }).ToArray();

        public ControlUiState UiState {
            get { return (ControlUiState)GetValue(UiStateProperty); }
            set { SetValue(UiStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UiStateProperty =
            DependencyProperty.Register("UiState", typeof(ControlUiState), typeof(UiController), new FrameworkPropertyMetadata(default(ControlUiState), OnUiStatePropertyChanged));

        private static void OnUiStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            var ui = (UiController)d;
            var state = (ControlUiState)args.NewValue;

            ui.InvalidateNode();

            if (state.Regions != null)
                ui.InvalidateRegion();
        }

        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(UiController), new PropertyMetadata((ICommand)null));

        private Canvas _canvas;

        public void InvalidateNode() {
            var nodes = UiState.Nodes.Select(e => new NodeInfo() { Location = e.Value, NodeId = e.Id, UiId = UiState.Id }).ToArray();

            // Clear nodes
            foreach (var shape in _canvas.Children.OfType<NodeShape>()) {
                _canvas.Children.Remove(shape);
            }


            foreach (var item in nodes) {
                var nodeShape = new NodeShape() {
                    //Node = item,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };

                nodeShape.Clicked += (s, ev) => {
                    if (s is NodeShape n) {
                        if (n.State == UiElementState.Selected)
                            SetValue(SelectedNodePropertyKey, n);
                        else
                            SetValue(SelectedNodePropertyKey, null);

                        foreach(var node in _canvas.Children.OfType<NodeShape>().Where(e => e != s)) {
                            node.Reset();
                        }

                        //Command.Execute(new CommandParameter {
                        //    Type = "Select",
                        //    Payload = new object[] { SelectedNode.Node },
                        //});

                    }
                };

                Canvas.SetLeft(nodeShape, item.Location.X - 20 / 2);
                Canvas.SetTop(nodeShape, item.Location.Y - 20 / 2);
                Canvas.SetZIndex(nodeShape, 10);

                _canvas.Children.Add(nodeShape);

                nodeShape.SetBinding(
                    NodeShape.NodeProperty,
                    new Binding() {
                        Source = item
                    });
            }
        }

        public void InvalidateRegion() {
            var simplices = UiState.Regions.OfType<SimplexState>();
            var voronois = UiState.Regions.OfType<VoronoiState>();

            foreach (var shape in _canvas.Children.OfType<SimplexShape>()) {
                _canvas.Children.Remove(shape);
            }

            foreach (var item in simplices) {
                var shape = new SimplexShape(UiState.Id) {
                    Points = item.Points,
                };

                Canvas.SetZIndex(shape, 5);

                _canvas.Children.Add(shape);
            }

            foreach (var shape in _canvas.Children.OfType<VoronoiShape>()) {
                _canvas.Children.Remove(shape);
            }

            foreach (var item in voronois) {
                var shape = new VoronoiShape(UiState.Id) {
                    Points = item.Points,
                };

                Canvas.SetZIndex(shape, 5);

                _canvas.Children.Add(shape);
            }
        }



        public UiController() {
            Focusable = true;
            Visibility = Visibility.Visible;

            Loaded += (se, ev) => {

                //Keyboard.AddPreviewGotKeyboardFocusHandler(
                //    this,
                //    (s, e) => {
                //        var ui = s as UiController;

                //        Console.WriteLine("{0}, {1}, {2}", Keyboard.FocusedElement, ui.IsFocused, ui.IsVisible);

                //        ui.BorderThickness = new Thickness(10);

                //    });

                //Keyboard.AddLostKeyboardFocusHandler(
                //    this,
                //    (s, e) => {
                //        var ui = s as UiController;

                //        ui.BorderThickness = new Thickness(0);

                //        ;
                //    });
            };

            MouseEnter += (s, e) => {
                var el = Keyboard.Focus(s as IInputElement);

                //Console.WriteLine(el);
            };


            var viewbox = new Viewbox {
                
            };
            var container = new Grid {
                Background = Brushes.Azure,
                ClipToBounds = true
            };
            _canvas = new Canvas() {
                Background = Brushes.DarkGray,
                SnapsToDevicePixels = true,
                UseLayoutRounding = true,
            };
            container.Children.Add(_canvas);
            viewbox.Child = container;

            container.SetBinding(WidthProperty, new Binding() {
                Path = new PropertyPath("ActualWidth"),
                RelativeSource = new RelativeSource() {
                    AncestorType = typeof(UiController),
                    Mode = RelativeSourceMode.FindAncestor
                },

            });

            container.SetBinding(HeightProperty, new Binding() {
                Path = new PropertyPath("ActualHeight"),
                RelativeSource = new RelativeSource() {
                    AncestorType = typeof(UiController),
                    Mode = RelativeSourceMode.FindAncestor
                },

            });

            Content = viewbox;

            // Pan
            MouseDown += (s, e) => {

                //Keyboard.Focus(this);
                

                if (mode == UiMode.Add) {
                    mousedownLocation = e.GetPosition(_canvas);
                }
                else if (mode == UiMode.Pan) {
                    isDragging = true;
                    start = e.GetPosition(this);
                    startMat = _canvas.RenderTransform.Value;
                }
            };

            MouseMove += (s, e) => {
                if (mode == UiMode.Pan) {
                    if (isDragging) {
                        var curr = e.GetPosition(this);
                        var vector = (curr - start);

                        var tMat = Matrix.Parse(startMat.ToString());
                        tMat.Translate(vector.X, vector.Y);

                        offset = (Point)vector;

                        translate = tMat;

                        tMat.Prepend(scale);

                        _canvas.RenderTransform = new MatrixTransform() {
                            Matrix = tMat,
                        };

                    }
                }
            };

            MouseUp += (s, e) => {
                if (mode == UiMode.Add) {
                    OnMouseClicked(mousedownLocation);
                }
                else if (mode == UiMode.Pan) {
                    var curr = e.GetPosition(this);
                    var vector = (curr - start);

                    var tMat = Matrix.Parse(startMat.ToString());
                    tMat.Translate(vector.X, vector.Y);

                    translate = tMat;

                    tMat.Append(scale);

                    _canvas.RenderTransform = new MatrixTransform() {
                        Matrix = tMat,
                    };

                    isDragging = false;
                }

                //var result = Keyboard.Focus(s as IInputElement);
                //var el = Keyboard.FocusedElement;
                //Console.WriteLine("{0}, {1}", result, el);
            };

            PreviewMouseWheel += (s, e) => {
                if (!(Keyboard.Modifiers == ModifierKeys.Control)) {
                    return;
                }

                var pivot = e.GetPosition(_canvas);
                var scale = e.Delta > 0 ? 1.25 : 1 / 1.25;

                Transform.ScaleAt(scale, scale, pivot.X, pivot.Y);

                InvalidateTransform();
            };

            PreviewKeyDown += (s, e) => {
                if (e.Key == Key.R) {
                    _canvas.RenderTransform = new MatrixTransform {
                        Matrix = Matrix.Identity
                    };
                }
                else if (e.Key == Key.B) {
                    OnBuild();
                }
                else if (e.Key == Key.D1) {
                    mode = UiMode.Add;
                }
                else if (e.Key == Key.D2) {
                    mode = UiMode.Pan;
                }
                else if (e.Key == Key.Escape) {
                    mode = UiMode.Default;
                }
            };

            Keyboard.AddPreviewKeyDownHandler(this, (s, e) => {
                //Console.WriteLine($"Keydown, {e.Key}");
            });
        }

        private Matrix Transform = Matrix.Identity;

        private UiMode mode = UiMode.Default;
        private Point mousedownLocation;

        private Point offset = new Point(0, 0);
        private Matrix translate = Matrix.Identity;
        private Matrix scale = Matrix.Identity;
        private Point prevAnchor = new Point(0, 0);

        private bool isDragging;
        private Point start;
        private Matrix startMat;
        //private Matrix translate = Matrix.Identity;

        private void OnMouseClicked(Point point) {
            var param = new CommandParameter();

            param.Payload = new object[] {
                point,
                UiState.Id
            };
            param.Type = "AddNode";

            Command.Execute(param);
        }

        private void OnBuild() {
            var param = new CommandParameter() {
                Payload = new object[] {
                    UiState.Id
                },
                Type = "Build",
            };
            
            Command.Execute(param);
        }

        private void InvalidateTransform() {
            foreach(var node in _canvas.Children.OfType<NodeShape>()) {
                var point = node.Node.Location;
                var tPoint = Transform.Transform(point);

                Canvas.SetLeft(node, tPoint.X);
                Canvas.SetTop(node, tPoint.Y);
            }
        }
    }

    public class VoronoiShape : UserControl {
        public int UiId { get; set; }

        public IEnumerable<Point> Points {
            get { return (IEnumerable<Point>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(IEnumerable<Point>), typeof(VoronoiShape), new FrameworkPropertyMetadata(null, OnPointsPropertyChanged));

        private static void OnPointsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var shape = (VoronoiShape)d;

            shape.Invalidate();
        }

        public VoronoiShape(int uiId) {
            UiId = uiId;

            MouseEnter += (s, e) => {
                var v = s as VoronoiShape;
                var path = v.Content as Path;

                path.Opacity = 0.1;
            };

            MouseLeave += (s, e) => {
                var v = s as VoronoiShape;
                var path = v.Content as Path;

                path.Opacity = 1;
            };
        }

        public void Invalidate() {
            var points = Points.ToArray();

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

                var op0 = o - p0;

                op0.Normalize();
                //var op0 = Point.Normalize(o - p0);
                var transform = Matrix.Identity;
                transform.Rotate(Math.PI * 90.0 / 180.0);
                var midP0 = transform.Transform(op0);
                //var midP0 = SKMatrix.CreateRotation((float)(Math.PI * 90.0 / 180.0)).MapVector(op0);
                midP0 *= midLen;

                var mid = p0 + midP0;

                var pathGeo = new PathGeometry();
                var pathFig = new PathFigure {
                    StartPoint = o,
                };

                pathGeo.Figures.Add(pathFig);

                pathFig.Segments.Add(new LineSegment { Point = p0 });
                pathFig.Segments.Add(new ArcSegment { Point = p1, Size = new Size(radius, radius), SweepDirection = SweepDirection.Counterclockwise });
                pathFig.Segments.Add(new LineSegment { Point = o });

                var fill = ColorManager.GetTintedColor(ColorManager.Palette[UiId], 2);
                var radial = new RadialGradientBrush();
                var radialRadius = (p0 - o).Length;

                radial.MappingMode = BrushMappingMode.Absolute;
                radial.GradientOrigin = o;
                radial.Center = o;
                radial.RadiusX = radialRadius;
                radial.RadiusY = radialRadius;
                radial.GradientStops.Add(new GradientStop(fill, 0.0));
                radial.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
                radial.Freeze();

                var path = new Path {
                    Fill = radial,
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    Stretch = Stretch.None,
                    StrokeThickness = 1.0,
                    Data = pathGeo
                };

                Content = path;

            }
            else {
                var pathGeo = new PathGeometry();
                var pathFig = new PathFigure {
                    StartPoint = points[0],
                };

                pathGeo.Figures.Add(pathFig);

                pathFig.Segments.Add(new LineSegment { Point = points[1] });
                pathFig.Segments.Add(new LineSegment { Point = points[2] });
                pathFig.Segments.Add(new LineSegment { Point = points[3] });
                pathFig.Segments.Add(new LineSegment { Point = points[0] });

                var fill = ColorManager.GetTintedColor(ColorManager.Palette[UiId], 2);
                var linear = new LinearGradientBrush();

                linear.MappingMode = BrushMappingMode.Absolute;
                linear.StartPoint = points[1];
                linear.EndPoint = points[2];
                linear.GradientStops.Add(new GradientStop(fill, 0.0));
                linear.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
                linear.Freeze();

                var path = new Path {
                    Fill = linear,
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    Stretch = Stretch.None,
                    StrokeThickness = 1.0,
                    Data = pathGeo
                };

                Content = path;
            }
        }
    }

    public class SimplexShape : UserControl {
        public int UiId { get; set; }


        public IEnumerable<Point> Points {
            get { return (IEnumerable<Point>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(IEnumerable<Point>), typeof(SimplexShape), new FrameworkPropertyMetadata(null, OnPointsPropertyChanged));

        private static void OnPointsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var shape = (SimplexShape)d;

            shape.Invalidate();

        }

        public SimplexShape(int uiId) {
            UiId = uiId;

            MouseEnter += (s, e) => {
                var v = s as SimplexShape;
                var path = v.Content as Path;

                path.Opacity = 0.1;
            };

            MouseLeave += (s, e) => {
                var v = s as SimplexShape;
                var path = v.Content as Path;

                path.Opacity = 1;
            };
        }

        public void Invalidate() {
            var points = Points.ToArray();

            var pathGeo = new PathGeometry();
            var pathFig = new PathFigure {
                StartPoint = points[0],
            };

            pathGeo.Figures.Add(pathFig);
            pathFig.Segments.Add(new LineSegment { Point = points[1] });
            pathFig.Segments.Add(new LineSegment { Point = points[2] });
            pathFig.Segments.Add(new LineSegment { Point = points[0] });

            var fill = ColorManager.GetTintedColor(ColorManager.Palette[UiId], 2);

            var path = new Path {
                Fill = new SolidColorBrush(fill),
                Stroke = new SolidColorBrush(Colors.DarkGray),
                Stretch = Stretch.None,
                StrokeThickness = 1.0,
                Data = pathGeo
            };

            Content = path;
        }
    }

    public class NodeShape : StatefulWidget {
        public UIElement overlay;
        public UiElementState state = UiElementState.Default;
        public Color PrimaryColor;

        public Image checkIcon;
        public Matrix Transform { get; set; } = Matrix.Identity;


        public static readonly RoutedEvent ClickedEvent = EventManager.RegisterRoutedEvent("Clicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NodeShape));

        public event RoutedEventHandler Clicked {
            add { AddHandler(ClickedEvent, value); }
            remove { RemoveHandler(ClickedEvent, value);}
        }

        public bool IsSelected { get; set; } = false;

        public NodeInfo Node {
            get { return (NodeInfo)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Node.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NodeProperty =
            DependencyProperty.Register("Node", typeof(NodeInfo), typeof(NodeShape), new PropertyMetadata(new NodeInfo(), OnPropertyChanged));

        public void InvalidateOverlay() {
            var overlay = new Grid();

            var shape = new Ellipse {
                Width = 20,
                Height = 20,
                Fill = new SolidColorBrush(Colors.Black),
                Opacity = 0.0,
            };

            var icon = new SvgIcon {
                Width = 12,
                Height = 12,
                Fill = new SolidColorBrush(Colors.Black),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                UriSource = new Uri(@"/icons/done.svg", UriKind.Relative),
                Visibility = Visibility.Hidden
            };

            overlay.Children.Add(shape);
            overlay.Children.Add(icon);

            Overlay = overlay;
        }

        public void InvalidateContent() {
            PrimaryColor = ColorManager.GetTintedColor(ColorManager.Palette[Node.UiId], 2);

            var container = new Grid();
            var content = new Ellipse {
                Width = 20,
                Height = 20,
                Stroke = Brushes.Black,
                Fill = new SolidColorBrush(PrimaryColor),
                StrokeThickness = 1.0,
            };

            container.Children.Add(content);
            container.Children.Add(Overlay);

            ToolTip = $"Node[{Node.NodeId}]-({Node.Location.X}, {Node.Location.Y})";

            RenderOptions.SetEdgeMode(content, EdgeMode.Unspecified);
            RenderOptions.SetBitmapScalingMode(content, BitmapScalingMode.HighQuality);

            Container = container;
        }


        public NodeShape() {
            _state = new DefaultNodeState(this);
        }

        protected override void OnMouseEnter(MouseEventArgs e) {
            base.OnMouseEnter(e);

            if (_state is DefaultNodeState) {
                GoToState(UiElementState.Hover);
            }
        }
        protected override void OnMouseLeave(MouseEventArgs e) {
            base.OnMouseLeave(e);

            if (_state is HoverNodeState) {
                GoToState(UiElementState.Default);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);

            if (_state is HoverNodeState && !IsSelected) {
                GoToState(UiElementState.Selected);
            }
            else if (_state is SelectedNodeState && IsSelected) {
                GoToState(UiElementState.Default);
            }

            RaiseEvent(new RoutedEventArgs(ClickedEvent));
        }

        static public void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is NodeShape widget) {
                widget.Invalidate();
            }
        }

        public void Reset() {
            GoToState(UiElementState.Default);
        }

        public override void GoToState(UiElementState state) {
            switch (state) {
                case UiElementState.Default:
                    _state = new DefaultNodeState(this);
                    break;
                case UiElementState.Hover:
                    _state = new HoverNodeState(this);
                    break;
                case UiElementState.Focus:
                    break;
                case UiElementState.Selected:
                    _state = new SelectedNodeState(this);
                    break;
                case UiElementState.Activated:
                    break;
                case UiElementState.Pressed:
                    break;
                case UiElementState.Dragged:
                    break;
            }

            base.GoToState(state);
        }

        public void Invalidate() {
            InvalidateOverlay();
            InvalidateContent();

            Content = Container;
        }
    }


    public class DefaultNodeState : BaseState {
        public DefaultNodeState(StatefulWidget widget) : base(widget) { }

        public override void SetOverlay() {
            var node = Parent as NodeShape;
            var overlay = node.Overlay as Grid;
            var shape = overlay.Children.OfType<Ellipse>().First();
            var icon = overlay.Children.OfType<SvgIcon>().First();

            icon.Visibility = Visibility.Hidden;
            shape.Opacity = 0;
        }

        public override void SetFlag() {
            var node = Parent as NodeShape;

            node.IsSelected = false;
        }

        public override void SetContainer() { }
    }

    public class SelectedNodeState : BaseState {
        public SelectedNodeState(NodeShape node) : base(node) {
        }

        public override void SetOverlay() {
            var node = Parent as NodeShape;
            var overlay = node.Overlay as Grid;
            var shape = overlay.Children.OfType<Ellipse>().First();
            var icon = overlay.Children.OfType<SvgIcon>().First();

            icon.Visibility = Visibility.Visible;
            shape.Opacity = 0.12;
        }

        public override void SetContainer() { }

        public override void SetFlag() {
            var node = Parent as NodeShape;

            node.IsSelected = true;
        }
    }

    public class HoverNodeState : BaseState {
        public HoverNodeState(NodeShape node) : base(node) {
        }

        public override void SetContainer() { }

        public override void SetFlag() { }

        public override void SetOverlay() {
            var node = Parent as NodeShape;
            var overlay = node.Overlay as Grid;
            var shape = overlay.Children.OfType<Ellipse>().First();
            var icon = overlay.Children.OfType<SvgIcon>().First();

            icon.Visibility = Visibility.Hidden;
            shape.Opacity = 0.08;
        }
    }

    public enum UiElementState {
        Default = 0,
        Hover,
        Focus,
        Selected,
        Activated,
        Pressed,
        Dragged
    }
}
