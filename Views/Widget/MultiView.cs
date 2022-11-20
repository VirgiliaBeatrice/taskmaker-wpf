using SharpVectors.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Linq;
using System.Linq.Expressions;
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
using taskmaker_wpf.Model.SimplicialMapping;
using taskmaker_wpf.ViewModels;
using taskmaker_wpf.Views.Widgets;
using static Unity.Storage.RegistrationSet;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace taskmaker_wpf.Views.Widget {
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
            if (nodeSets.Count() == 1)
                return nodeSets.ToArray();
            else {
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

    public class NodeRelationViewer : UserControl {
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
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MultiView), new FrameworkPropertyMetadata(new IInputPort[0], OnPropertyChanged));


        public int MaxColumnCount {
            get { return (int)GetValue(MaxColumnCountProperty); }
            set { SetValue(MaxColumnCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxRowCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxColumnCountProperty =
            DependencyProperty.Register("MaxColumnCount", typeof(int), typeof(MultiView), new FrameworkPropertyMetadata(2));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            if (d is MultiView view) {
                var newUis = (args.NewValue as IInputPort[]).Select(e => e.Name);
                var oldUis = (args.OldValue as IInputPort[]).Select(e => e.Name);
                

                if (Enumerable.SequenceEqual(oldUis, newUis)) {
                    for (int i = 0; i < view.Controllers.Count; i++) {
                        view.Controllers[i].UiState = (ControlUiState)(args.NewValue as IInputPort[])[i];
                    }
                }
                else {
                    view.Layout();

                    view.InvalidateViewer();
                }



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
                var box = new Viewbox {
                    Stretch = Stretch.None,
                };

                var ui = new UiController {
                    VerticalAlignment= VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(2),
                    Command = vm.UiCommand,
                };

                //box.Child = ui;
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
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
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

        internal static readonly DependencyPropertyKey SelectedNodePropertyKey = DependencyProperty.RegisterReadOnly("SelectedNode", typeof(NodeShape), typeof(UiController), new FrameworkPropertyMetadata(null, OnSelectedNodePropertyChanged));

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

            ui.InvalidateTransform();
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
            var nodeInfos = UiState.Nodes.Select(e => new NodeInfo() { Location = e.Value, NodeId = e.Id, UiId = UiState.Id }).ToArray();

            // Clear nodes
            var nodes = _canvas.Children.OfType<NodeShape>().ToArray();

            foreach (var shape in nodes) {
                _canvas.Children.Remove(shape);
            }


            foreach (var item in nodeInfos) {
                var nodeShape = new NodeShape() {
                    //Node = item,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
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

                //Canvas.SetLeft(nodeShape, item.Location.X - 20 / 2);
                //Canvas.SetTop(nodeShape, item.Location.Y - 20 / 2);
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
            var simplexStates = UiState.Regions.OfType<SimplexState>();
            var voronoiStates = UiState.Regions.OfType<VoronoiState>();

            var simplices = _canvas.Children.OfType<SimplexShape>().ToArray();
            foreach (var shape in simplices) {
                _canvas.Children.Remove(shape);
            }

            foreach (var item in simplexStates) {
                var shape = new SimplexShape(UiState.Id) {
                    Points = item.Points,
                };

                Canvas.SetZIndex(shape, 5);

                _canvas.Children.Add(shape);
            }

            var voronois = _canvas.Children.OfType<VoronoiShape>().ToArray();
            foreach (var shape in voronois) {
                _canvas.Children.Remove(shape);
            }

            foreach (var item in voronoiStates) {
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


            var container = new Grid {
                Background = Brushes.Azure,
                ClipToBounds = true,
                //Width = 400,
                //Height = 400,
            };
            _canvas = new Canvas() {
                Background = Brushes.DarkGray,
                SnapsToDevicePixels = true,
                UseLayoutRounding = true,
            };

            var axisX = new Arrow {
                Name = "AxisX",
                Start = new Point(0, 0),
                End = new Point(100, 0),
                Stroke = Brushes.Red
            };

            var axisY = new Arrow {
                Name = "AxisY",
                Start = new Point(0, 0),
                End = new Point(0, 100),
                Stroke = Brushes.Green
            };

            //var origin = new Ellipse {
            //    Fill = new SolidColorBrush(Colors.Black),
            //    Width = 2,
            //    Height = 2,
            //};

            //Canvas.SetTop(origin, -2 / 2);
            //Canvas.SetLeft(origin, -2 / 2);

            _canvas.Children.Add(axisX);
            _canvas.Children.Add(axisY);
            container.Children.Add(_canvas);

            container.SetBinding(WidthProperty, new Binding() {
                Path = new PropertyPath("ActualWidth"),
                RelativeSource = new RelativeSource() {
                    AncestorType = typeof(Grid),
                    Mode = RelativeSourceMode.FindAncestor
                },

            });

            container.SetBinding(HeightProperty, new Binding() {
                Path = new PropertyPath("ActualHeight"),
                RelativeSource = new RelativeSource() {
                    AncestorType = typeof(Grid),
                    Mode = RelativeSourceMode.FindAncestor
                },

            });

            Content = container;

            container.SizeChanged += (s, e) => {

                var cCenter = new Point(e.NewSize.Width / 2.0, e.NewSize.Height / 2.0);

                Normalized.SetIdentity();
                Normalized.Scale(1, -1);

                Normalized.Translate(cCenter.X, cCenter.Y);
                //var pCenter = new Point(e.PreviousSize.Width / 2.0, e.PreviousSize.Height / 2.0);

                //var diff = cCenter - pCenter;

                //Offset = Matrix.Identity;
                //Offset.Translate(diff.X, diff.Y);

                InvalidateTransform();
            };

            // Pan
            MouseDown += (s, e) => {

                //Keyboard.Focus(this);
                

                if (mode == UiMode.Add) {
                    var point = e.GetPosition(_canvas);
                    var invMat = Transform;

                    invMat.Invert();
                    mousePosition = invMat.Transform(point);
                }
                else if (mode == UiMode.Pan) {
                    isDragging = true;
                    start = e.GetPosition(_canvas);
                    startMat = Translate;
                }
            };

            MouseMove += (s, e) => {
                if (mode == UiMode.Pan) {
                    if (isDragging) {
                        var curr = e.GetPosition(_canvas);
                        var vector = (curr - start);

                        var tMat = Matrix.Parse(startMat.ToString());
                        tMat.Translate(vector.X, vector.Y);

                        Translate = tMat;

                        InvalidateTransform();
                    }
                }
            };

            MouseUp += (s, e) => {
                if (mode == UiMode.Add) {
                    OnMouseClicked(mousePosition);
                }
                else if (mode == UiMode.Pan) {
                    var curr = e.GetPosition(_canvas);
                    var vector = (curr - start);

                    var tMat = Matrix.Parse(startMat.ToString());
                    tMat.Translate(vector.X, vector.Y);

                    Translate = tMat;

                    InvalidateTransform();
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

                Scale.ScaleAt(scale, scale, pivot.X, pivot.Y);

                InvalidateTransform();
            };

            PreviewKeyDown += (s, e) => {
                if (e.Key == Key.R) {
                    Translate = Matrix.Identity;
                    Scale = Matrix.Identity;

                    InvalidateTransform();
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

        private Matrix Normalized = Matrix.Identity;

        private Matrix Transform = Matrix.Identity;
        private Matrix Offset = Matrix.Identity;
        private Matrix Translate = Matrix.Identity;
        private Matrix Scale = Matrix.Identity;

        private UiMode mode = UiMode.Default;
        private Point mousePosition;

        private bool isDragging;
        private Point start;
        private Matrix startMat;

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

            Transform.SetIdentity();
            Transform.Append(Normalized);
            Transform.Append(Translate);
            Transform.Append(Scale);

            //Transform.
            //Transform = Offset * Translate * Scale;

            var x = _canvas.Children.OfType<Arrow>().Where(e => e.Name == "AxisX").First();
            var y = _canvas.Children.OfType<Arrow>().Where(e => e.Name == "AxisY").First();

            x.Transform = Transform;
            y.Transform = Transform;

            // Invalidate all nodes transformation
            foreach (var node in _canvas.Children.OfType<NodeShape>()) {
                var point = node.Node.Location;
                var tPoint = Transform.Transform(point);

                Canvas.SetLeft(node, tPoint.X - 20 / 2);
                Canvas.SetTop(node, tPoint.Y - 20 / 2);
            }

            foreach(var simplex in _canvas.Children.OfType<SimplexShape>()) {
                simplex.Transform = Transform;

                simplex.Invalidate();
            }

            foreach(var voronoi in _canvas.Children.OfType<VoronoiShape>()) {
                voronoi.Transform = Transform;

                voronoi.Invalidate();
            }
        }
    }

    public class VoronoiShape : UserControl {
        public int UiId { get; set; }
        public Matrix Transform { get; set; } = Matrix.Identity;

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
            var points = Points.Select(e => Transform.Transform(e)).ToArray();

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

                pathFig.Segments.Add(new LineSegment { Point = p1 });
                pathFig.Segments.Add(new ArcSegment { Point = p0, Size = new Size(radius, radius), SweepDirection = SweepDirection.Counterclockwise });
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

        public Matrix Transform { get; set; } = Matrix.Identity;

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
            var points = Points.Select(e => Transform.Transform(e)).ToArray();

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

    public class Arrow : Shape {



        public Matrix Transform {
            get { return (Matrix)GetValue(TransformProperty); }
            set { SetValue(TransformProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Transform.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransformProperty =
            DependencyProperty.Register("Transform", typeof(Matrix), typeof(Arrow), new PropertyMetadata(Matrix.Identity, OnPropertyChanged));



        public Point Start {
            get { return (Point)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Start.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start", typeof(Point), typeof(Arrow), new PropertyMetadata(new Point(), (PropertyChangedCallback)OnPropertyChanged));

        static public void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            (d as Arrow).InvalidateVisual();
        }


        public Point End {
            get { return (Point)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }

        // Using a DependencyProperty as the backing store for End.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End", typeof(Point), typeof(Arrow), new PropertyMetadata(new Point(), OnPropertyChanged));


        public Arrow() {
            Stroke = Brushes.Green;
            //SnapsToDevicePixels = false;
            //UseLayoutRounding = false;
        }

        protected override Geometry DefiningGeometry => Generate();

        private Geometry Generate() {
            var start = Transform.Transform(Start);
            var end = Transform.Transform(End);

            var arrowLine = new PathFigure();

            if (start == end)
                return new LineGeometry();

            arrowLine.StartPoint = start;

            arrowLine.Segments.Add(new LineSegment(end, true));

            var vector = (start - end);

            vector.Normalize();

            var rotate = Matrix.Identity;

            rotate.Rotate(15);

            var e0 = rotate.Transform(vector * 10.0) + end;

            rotate.SetIdentity();
            rotate.Rotate(-15);

            var e1 = rotate.Transform(vector * 10.0) + end;

            var arrowTip = new PathFigure();

            arrowTip.StartPoint = end;

            arrowTip.Segments.Add(new LineSegment(e0, true));
            arrowTip.Segments.Add(new LineSegment(e1, true));
            arrowTip.Segments.Add(new LineSegment(end, true));

            var geometry = new PathGeometry();

            geometry.Figures.Add(arrowLine);
            geometry.Figures.Add(arrowTip);

            return geometry;
        }
    }
}
