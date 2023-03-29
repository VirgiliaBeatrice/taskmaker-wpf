using SharpVectors.Converters;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.XPath;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Model.SimplicialMapping;
using taskmaker_wpf.ViewModels;
using taskmaker_wpf.Views.Widgets;
using static Unity.Storage.RegistrationSet;
using Rectangle = System.Windows.Shapes.Rectangle;
using Point = System.Windows.Point;
using NLog;
using static System.Windows.Forms.LinkLabel;
using System.IO;
using Path = System.Windows.Shapes.Path;

namespace taskmaker_wpf.Views.Widget {
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

    public class NodeRelation {
        public bool HasValue { get; set; }
        public readonly NodeInfo[] Nodes;

        public NodeRelation(NodeInfo[] nodes) {
            Nodes = nodes;
        }

        public NodeInfo this[int index] {
            get => Nodes[index];
        }

        public override string ToString() {
            return $"[{string.Join(",", Nodes.Select(e => $"{e.UiId}({e.NodeId})"))}]";
        }
    }


    public struct NodeInfo {
        public int UiId { get; set; }
        public int NodeId { get; set; }
        public Point Location { get; set; }
    }

    public static class Helper {
        static public (double, double) Intersect(Point[] lineA, Point[] lineB) {
            var a = lineA[0];
            var b = lineA[1];
            var c = lineB[0];
            var d = lineB[1];

            var dir0 = b - a;
            var dir1 = d - c;

            if (dir0.Cross(dir1) == 0)
                return (double.NaN, double.NaN);

            // p = a + dir0 * t1 = c + dir1 * t2
            // A * T = B
            //var A = new Matrix() {

            //}(2, 2, new float[] { dir0.X, dir1.X, dir0.Y, dir1.Y });
            //var B = (c - a).ToVector();
            //var T = A.Solve(B);

            //return (T[0], T[1]);
            return default;
        }

        static public double Cross(this Vector a, Vector b) {
            return a.X * b.Y - a.Y * b.X;
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

            widget.IsSelected = false;
        }

        public override void SetOverlay() {
            var grid = Parent.Overlay as Grid;
            var widget = Parent as RelationWidget;
            var overlay = grid.Children.OfType<Rectangle>().First();
            var icon = grid.Children.OfType<SvgIcon>().First();

            // Set color
            overlay.Fill = Brushes.White;
            // Set opacity
            overlay.Opacity = 0.0;


            //if (widget.)
            //icon.Visibility = Visibility.Hidden;
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

            //icon.Visibility = Visibility.Hidden;
        }
    }

    public class SelectedState : BaseState {
        public SelectedState(StatefulWidget parent) : base(parent) {
        }

        public override void SetContainer() {
        }

        public override void SetFlag() {
            var widget = Parent as RelationWidget;

            widget.IsSelected = true;
        }

        public override void SetOverlay() {
            var grid = Parent.Overlay as Grid;
            var overlay = grid.Children.OfType<Rectangle>().First();
            var icon = grid.Children.OfType<SvgIcon>().First();

            // Set color
            overlay.Fill = Brushes.Red;
            // Set opacity
            overlay.Opacity = 0.12;

            //icon.Visibility = Visibility.Visible;
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
            
            //icon.Visibility = Visibility.Hidden;

        }
    }

    public class ActivedState : BaseState {
        public ActivedState(StatefulWidget parent) : base(parent) {
        }

        public override void SetContainer() {
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
        }
    }


    public class RelationWidget : StatefulWidget {
        public bool IsSelected { get; set; } = false;
        //public bool HasValue { get; set; } = false;
        public NodeRelation Relation { get; set; }

        public RelationWidget(NodeRelation relation) {
            Relation = relation;

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
                Visibility = Relation.HasValue ? Visibility.Visible : Visibility.Hidden,
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

            //MouseLeftButtonDown += (s, e) => {
            //    var widget = s as RelationWidget;

            //    CaptureMouse();
            //    if (widget.State == UiElementState.Hover)
            //        GoToState(UiElementState.Pressed);
            //    else if (widget.State == UiElementState.Selected) {
            //        GoToState(UiElementState.Activated);
            //    }
            //};

            //MouseLeftButtonUp += (s, e) => {
            //    var widget = s as RelationWidget;

            //    if (widget.State == UiElementState.Pressed) {
            //        GoToState(UiElementState.Selected);

            //        var parent = LogicalTreeHelperExtensions.FindAncestor<NodeRelationViewer>(this);

            //        parent.Select(Relation.Nodes);
            //        //if (!IsSelected)
            //        //    GoToState(UiElementState.Default);
            //    }
            //    else if (widget.State == UiElementState.Activated) {
            //        var parent = LogicalTreeHelperExtensions.FindAncestor<MultiView>(this);

            //        parent.Bind();

            //        GoToState(UiElementState.Default);
            //    }

            //    ReleaseMouseCapture();
            //};

            PreviewKeyDown += (s, e) => {

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
                    _state = new ActivedState(this);
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
            DependencyProperty.Register("Combinations", typeof(IEnumerable<NodeRelation>), typeof(NodeRelationViewer), new PropertyMetadata(new NodeRelation[0], OnPropertyChanged));


        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            if (d is NodeRelationViewer viewer) {
                //viewer.Layout();
                var relations = args.NewValue as NodeRelation[];

                if (relations.Length == 1) {
                    viewer.IsWaitToBind = true;
                    //viewer.View.SetSelectedNodeIndices(relations[0].Nodes.Select(e => e.NodeId).ToArray());
                }else {
                    viewer.IsWaitToBind = false;
                }

                //if ((bool)args.NewValue == true)
                //    viewer.Visibility= Visibility.Visible;
                //else
                //    viewer.Visibility= Visibility.Collapsed;
            }
        }

        public bool IsWaitToBind { get; set; } = false;

        public MultiView View { get; set; }
        public List<RelationWidget> Relations { get; set; } = new List<RelationWidget>();

        public NodeRelationViewer(MultiView parent) {
            View = parent;

            Margin = new Thickness(0, 10, 0, 0);
            Background = new SolidColorBrush(Colors.White);

            InvalidateState();
        }

        public void InvalidateState() {
            Layout();

            if (IsWaitToBind)
                Relations[0].GoToState(UiElementState.Activated);
        }

        public void Layout() {
            Relations.Clear();

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
                    Stroke = new SolidColorBrush(Colors.Black),
                    Fill = new SolidColorBrush(fill),
                    Margin = new Thickness(2),
                    ToolTip = $"{a.ElementAt(i).UiId}({a.ElementAt(i).NodeId})"
                };

                panel0.Children.Add(circle);
            }

            foreach (var item in b) {
                var rect = new RelationWidget(item) {
                    Margin = new Thickness(2),
                    ToolTip = item.ToString(),
                };

                rect.MouseDoubleClick += Rect_MouseDoubleClick;
                rect.MouseLeftButtonDown += Rect_MouseLeftButtonDown;
                rect.MouseLeftButtonUp += Rect_MouseLeftButtonUp;
                //var rect = new Rectangle() {
                //    Width = 20,
                //    Height = 20,
                //    Fill = ColorPalette[4],
                //    Margin = new Thickness(2),
                //    //Opacity = item.HasValue ? 1.0 : 0.6,
                //    ToolTip = item.ToString()
                //};

                panel1.Children.Add(rect);
                Relations.Add(rect);
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

        private void Rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs args) {
            var r = sender as RelationWidget;

            r.ReleaseMouseCapture();

            if (!IsWaitToBind) {
                if (r.State == UiElementState.Pressed) {
                    var parent = this;

                    parent.Select(r.Relation.Nodes);

                    r.GoToState(UiElementState.Default);
                }
            }
            else {
                if (r.State == UiElementState.Pressed) {
                    var parent = LogicalTreeHelperExtensions.FindAncestor<MultiView>(this);
                    var index = Combinations.First().Nodes.Select(e => e.NodeId).ToArray();

                    parent.Bind(index);

                    r.GoToState(UiElementState.Default);
                }
            }
        }

        private void Rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var r = sender as RelationWidget;

            r.CaptureMouse();

            if (!IsWaitToBind) {
                if (r.State == UiElementState.Hover)
                    r.GoToState(UiElementState.Pressed);
            }
            else {
                if (r.State == UiElementState.Activated)
                    r.GoToState(UiElementState.Pressed);
            }
        }

        private void Rect_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var relationWidget = sender as RelationWidget;

            if (Combinations.Count() == 1) {
                Console.WriteLine("Bind!");
            }
        }

        public void Select(NodeInfo[] nodes) {
            var parent = LogicalTreeHelperExtensions.FindAncestor<MultiView>(this);

            parent.Select(nodes);
        }
    }

    public class MultiView : UserControl {
        public IEnumerable ItemsSource {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MultiView), new FrameworkPropertyMetadata(new ControlUiState[0], OnPropertyChanged));



        public ICommand BindCommand {
            get { return (ICommand)GetValue(BindCommandProperty); }
            set { SetValue(BindCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BindCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindCommandProperty =
            DependencyProperty.Register("BindCommand", typeof(ICommand), typeof(MultiView), new PropertyMetadata(default));


        public int MaxColumnCount {
            get { return (int)GetValue(MaxColumnCountProperty); }
            set { SetValue(MaxColumnCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxRowCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxColumnCountProperty =
            DependencyProperty.Register("MaxColumnCount", typeof(int), typeof(MultiView), new FrameworkPropertyMetadata(2));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            if (d is MultiView view) {
                var uis = args.NewValue as ControlUiState[];

                //// clear view.Controllers
                //view.Controllers.Clear();

                //// for all view.Controllers
                //foreach (var ui in uis) {
                //    var controller = new UiController() {
                //        UiState = ui,
                //    };

                //    view.Controllers.Add(controller);
                    
                //    controller.SetBinding(UiController.UiStateProperty, new Binding() { Source = ui });
                //}

                view.Layout();
                view.InvalidateViewer();
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
                var temp = locked.Select(e => new NodeInfo[] { e }).Concat(unlocked).OrderBy(e => e.First().UiId).ToArray();
                var combinations = Helper.GetCombinations(temp);
                var vm = (RegionControlUIViewModel)DataContext;

                var combinationStatus = vm.GetTensorStatus(
                    combinations.Select(e => e.Select(e1 => e1.NodeId).ToArray()).ToArray());

                Viewer.Locked = locked;
                Viewer.Combinations = combinations.Zip(combinationStatus, (combination, status) => new NodeRelation(combination) { HasValue = status }).ToArray();


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
        public ScrollViewer Scroll { get; set; }

        public MultiView() : base() {
            var grid = new Grid() {
                Name = "Multiview_Grid",
            };
            //var icon = new SvgIcon() {
            //    Width = 200,
            //    Height = 200,
            //    UriSource = new Uri(@"icons/done.svg", UriKind.Relative),
            //};

            Scroll = new ScrollViewer() {
                Focusable= false,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            grid.Children.Add(Scroll);

            Content = grid;
        }

        public void Bind(int[] index) {
            BindCommand.Execute(index);

            InvalidateViewer();
            //Console.WriteLine("A binding data has prepared.");
        }

        public void UnSelect() {
            foreach(var c in Controllers) {
                c.UnSelect();
            }
        }

        public void Select(NodeInfo[] nodes) {
            UnSelect();

            foreach (var node in nodes) {
                Select(node.UiId, node.NodeId);
            }
        }

        public void Select(int uiId, int nodeId) {
            var controller = Controllers.Find(e => e.UiState.Id == uiId);

            controller.Select(nodeId);
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
                Name = "Multiview_SubGrid",
                Visibility = Visibility.Visible
            };

            var vm = DataContext as RegionControlUIViewModel;

            Scroll.Content = grid;
            Controllers.Clear();

            if (items.Count == 1) {
                var ui = new UiController {
                    VerticalAlignment= VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(2),
                    Command = vm.UpdateUiCommand,
                };

                ui.NotifyStatus += Ui_NotifyStatus;

                var textblock = new TextBlock {
                    Text = items[0].ToString(),
                    FontSize = 42,
                    Foreground = Brushes.DimGray,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Right,

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
                        Command = vm.UpdateUiCommand,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                    };
                    var textblock = new TextBlock {
                        Text = items[i].ToString(),
                        FontSize = 42,
                        Foreground = Brushes.DimGray,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = HorizontalAlignment.Right,

                    };

                    ui.NotifyStatus += Ui_NotifyStatus;

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

            //Scroll.Content = grid;
            //((Grid)Content).Children.Add(Scroll);
            //((Grid)Content).Children.Add(Viewer);
        }

        private void Ui_NotifyStatus(object sender, NotifyStatusEventArgs e) {
        }
    }

    public enum UiMode {
        Default = 0,
        Add,
        Delete,
        Build,
        Trace,
        Drag,
        Pan,
        Zoom
    }

    public abstract class BaseUiState {
        public UiController Parent { get; set; }

        public BaseUiState(UiController parent) {
            Parent = parent;
        }

        public virtual void SetFlags() { }
    }

    public class DefaultUiState : BaseUiState {
        public DefaultUiState(UiController parent) : base(parent) {
        }

        public override void SetFlags() {
            Parent.Pointer.Visibility = Visibility.Collapsed;
        }
    }

    public class TraceUiState : BaseUiState {
        public TraceUiState(UiController parent) : base(parent) {
        }

        public override void SetFlags() {
            Parent.Pointer.Visibility = Visibility.Visible;
        }
    }

    public class CommandParameter {
        public string Type { get; set; }
        public object[] Payload { get; set; }
    }

    public class NotifyStatusEventArgs : EventArgs {
        public string Status { get; set; }

        
        public NotifyStatusEventArgs(string status) {
            Status = status;
        }

        public NotifyStatusEventArgs() {
        }
    }

    public class UiController : UserControl {
        public PointerWidget Pointer { get; set; } = new PointerWidget();

        public BaseUiState State { get; set; }

        public event EventHandler<NotifyStatusEventArgs> NotifyStatus;

        internal static readonly DependencyPropertyKey SelectedNodePropertyKey = DependencyProperty.RegisterReadOnly("SelectedNode", typeof(NodeShape), typeof(UiController), new FrameworkPropertyMetadata(null, OnSelectedNodePropertyChanged));

        private static void OnSelectedNodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var parent = VisualTreeHelper.GetParent(d as Visual);

            while (parent != null && !typeof(MultiView).IsInstanceOfType(parent)) {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is MultiView v) {
                v.InvalidateViewer();
                //Console.WriteLine("Some property changed.");
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



        public object VMTemp {
            get { return (object)GetValue(VMTempProperty); }
            set { SetValue(VMTempProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VMTemp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMTempProperty =
            DependencyProperty.Register("VMTemp", typeof(object), typeof(UiController), new PropertyMetadata(null));



        private static void OnUiStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            var ui = (UiController)d;
            var state = (ControlUiState)args.NewValue;

            state.PropertyChanged += (s, e) => {
                if (e.PropertyName == "Nodes") {
                    ui.InvalidateNode();
                }
                else if (e.PropertyName == "Regions") {
                    ui.InvalidateRegion();
                }

                ui.InvalidateTransform();
            };

            ui.InvalidateNode();
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
        private Label _status;
        public NodeShape[] NodeVisuals { get; set; } = new NodeShape[0];

        public void InvalidateNode() {
            var nodeInfos = UiState.Nodes.Select(e => new NodeInfo() { Location = e.Value, NodeId = e.Id, UiId = UiState.Id }).ToArray();

            // Clear nodes
            var nodes = _canvas.Children.OfType<NodeShape>().ToArray();

            foreach (var shape in nodes) {
                _canvas.Children.Remove(shape);
            }


            foreach (var item in nodeInfos) {
                var nodeShape = new NodeShape(item.NodeId) {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };

                nodeShape.MouseLeftButtonDown += NodeShape_MouseLeftButtonDown;
                nodeShape.MouseLeftButtonUp += NodeShape_MouseLeftButtonUp;

                //nodeShape.Clicked += (s, ev) => {
                //    if (s is NodeShape n) {
                //        if (n.State == UiElementState.Selected)
                //            SetValue(SelectedNodePropertyKey, n);
                //        else
                //            SetValue(SelectedNodePropertyKey, null);

                //        foreach(var node in _canvas.Children.OfType<NodeShape>().Where(e => e != s)) {
                //            node.Reset();
                //        }
                //    }
                //};

                //Canvas.SetLeft(nodeShape, item.Location.X - 20 / 2);
                //Canvas.SetTop(nodeShape, item.Location.Y - 20 / 2);
                nodeShape.Transform = Transform;
                Panel.SetZIndex(nodeShape, 10);

                _canvas.Children.Add(nodeShape);

                nodeShape.SetBinding(
                    NodeShape.NodeProperty,
                    new Binding() {
                        Source = item
                    });
            }

            NodeVisuals = _canvas.Children.OfType<NodeShape>().ToArray();
        }

        private void NodeShape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var nodeShape = sender as NodeShape;

            nodeShape.ReleaseMouseCapture();

            if (nodeShape.State == UiElementState.Hover) {
                UnSelect();
                Select(nodeShape.NodeId);
            }
            else if (nodeShape.State == UiElementState.Selected) {
                UnSelect();
            }
        }

        private void NodeShape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var nodeShape = sender as NodeShape;

            nodeShape.CaptureMouse();
        }

        public void InvalidateRegion() {
            var simplexStates = UiState.Regions.OfType<SimplexState>();
            var voronoiStates = UiState.Regions.OfType<VoronoiState>();

            var simplices = _canvas.Children.OfType<SimplexShape>().ToArray();
            foreach (var shape in simplices) {
                _canvas.Children.Remove(shape);
            }

            foreach (var item in simplexStates) {
                var shape = new SimplexShape(UiState.Id, item) {
                    Points = item.Points,
                };

                Panel.SetZIndex(shape, 5);

                _canvas.Children.Add(shape);
            }

            var voronois = _canvas.Children.OfType<VoronoiShape>().ToArray();
            foreach (var shape in voronois) {
                _canvas.Children.Remove(shape);
            }

            foreach (var item in voronoiStates) {
                var shape = new VoronoiShape(UiState.Id, item) {
                    Points = item.Points,
                };

                Panel.SetZIndex(shape, 5);

                _canvas.Children.Add(shape);
            }

        }

        private void CreateIKTemplate() {
            var template = new IKTemplate() {
                Name = "IK",
                Width = 400,
                Height = 400,
                Visibility = Visibility.Visible,
            };

            //var o = new Point(0, 0);

            //var radius = Math.Sin(30.0 * Math.PI / 180.0) * 100.0;

            //var p1 = new Point(0, 100);
            //var p2 = new Point {
            //    X = -Math.Cos(60.0 * Math.PI / 180.0),
            //    Y = -radius
            //};
            //var p3 = new Point {
            //    X = Math.Cos(60.0 * Math.PI / 180.0),
            //    Y = radius
            //};

            //var p1Shape = DrawPoint(p1);
            //var p2Shape = DrawPoint(p2);
            //var p3Shape = DrawPoint(p3);


            //var circle = new Ellipse() {
            //    Width = radius * 2,
            //    Height = radius * 2,
            //    StrokeThickness = 2,
            //    Stroke = Brushes.Black
            //};

            //var centroid = new Point(o.X - radius, o.Y - radius);

            //_canvas.Children.Add(circle);
            //_canvas.Children.Add(p1Shape);
            //_canvas.Children.Add(p2Shape);
            //_canvas.Children.Add(p3Shape);
            _canvas.Children.Add(template);
        }

        public void Save(string filename) {
            var rootVisual = Content as Visual;
            var rtb = new RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Pbgra32);

            rtb.Render(rootVisual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var stream = File.Create(filename)) {
                encoder.Save(stream);
            }
        }

        private void DrawLineWithArrowCap(Point startPoint, Point endPoint, Canvas canvas) {
            // Create a new line object
            Line line = new Line();

            // Set the start and end points of the line
            line.X1 = startPoint.X;
            line.Y1 = startPoint.Y;
            line.X2 = endPoint.X;
            line.Y2 = endPoint.Y;

            // Set the line thickness and color
            line.StrokeThickness = 2;
            line.Stroke = Brushes.Black;

            // Create an arrow cap at the end of the line
            double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X) * 180 / Math.PI;
            Polyline arrowCap = new Polyline();
            arrowCap.Points.Add(new Point(-10, -5));
            arrowCap.Points.Add(new Point(0, 0));
            arrowCap.Points.Add(new Point(-10, 5));
            arrowCap.RenderTransform = new RotateTransform(angle, endPoint.X, endPoint.Y);
            arrowCap.Fill = Brushes.Black;

            // Add the line and arrow cap to the canvas
            canvas.Children.Add(line);
            canvas.Children.Add(arrowCap);
        }

        private Shape DrawPoint(Point point) {
            // Create a new Ellipse object
            Ellipse ellipse = new Ellipse();

            // Set the center point and radius of the ellipse
            Canvas.SetLeft(ellipse, point.X - 2);
            Canvas.SetTop(ellipse, point.Y - 2);
            ellipse.Width = 4;
            ellipse.Height = 4;

            // Set the fill color of the ellipse
            ellipse.Fill = Brushes.Black;

            // Add the ellipse to the canvas
            return ellipse;
        }



        public UiController() {
            Pointer.Visibility = Visibility.Collapsed;
            Panel.SetZIndex(Pointer, 25);

            State = new DefaultUiState(this);
            Focusable = true;
            Visibility = Visibility.Visible;


            MouseEnter += (s, e) => {
                var el = Keyboard.Focus(s as IInputElement);

                //Console.WriteLine(el);
            };

            var border = new Border {
                BorderBrush = new SolidColorBrush(Colors.DarkGray),
                BorderThickness = new Thickness(1),
            };

            var container = new Grid {
                //Background = Brushes.Azure,
                ClipToBounds = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                //Width = 400,
                //Height = 400,
            };

            _status = new Label() {
                Content = mode.ToString()
            };
            _canvas = new Canvas() {
                Background = Brushes.White,
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

            _canvas.Children.Add(axisX);
            _canvas.Children.Add(axisY);
            _canvas.Children.Add(_status);
            _canvas.Children.Add(Pointer);

            CreateIKTemplate();

            container.Children.Add(_canvas);

            border.Child = container;
            Content = border;

            container.SizeChanged += (s, e) => {

                var cCenter = new Point(e.NewSize.Width / 2.0, e.NewSize.Height / 2.0);

                Normalized.SetIdentity();
                Normalized.Scale(1, -1);

                Normalized.Translate(cCenter.X, cCenter.Y);
                //var pCenter = new Point(e.PreviousSize.Width / 2.0, e.PreviousSize.Height / 2.0);

                //var diff = cCenter - pCenter;

                //Offset = Matrix.Identity;
                //Offset.Translate(diff.X, diff.Y);

                var ik = _canvas.Children.OfType<IKTemplate>().First();

                ik.Origin = cCenter;

                InvalidateTransform();
                //Save("test1.png");

            };

            // Pan
            MouseDown += (s, e) => {

                if (mode == UiMode.Add) {
                    CaptureMouse();

                    var point = e.GetPosition(_canvas);
                    var invMat = Transform;

                    invMat.Invert();
                    mousePosition = invMat.Transform(point);
                }
                else if (mode == UiMode.Pan) {
                    CaptureMouse();

                    isDragging = true;
                    start = e.GetPosition(_canvas);
                    startMat = Translate;
                }
                else if (mode == UiMode.Trace || mode == UiMode.Drag) {
                    CaptureMouse();

                    start = e.GetPosition(_canvas);

                    Pointer.Location = start;
                    isDragging = true;
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
                else if (mode == UiMode.Trace || mode == UiMode.Drag) {
                    if (isDragging) {
                        var curr = e.GetPosition(_canvas);
                        var mat = Transform;
                        var invMat = Transform;
                        invMat.Invert();

                        // pointer in local
                        var tCurr = invMat.Transform(curr);


                        if (UiState.Nodes.Length == 2) {
                            // P1 = P0 + k * V

                            double _clamp(double value, double min, double max) {
                                if (value > max)
                                    return max;
                                else if (value < min) 
                                    return min;
                                else return value;
                            }

                            var nodes = UiState.Nodes.OrderBy(e0 => e0.Value.X).Select(e0 => e0.Value).ToArray();
                            var v = nodes[1] - nodes[0];
                            var range = nodes.Select(e0 => e0.X).ToArray();
                            var x = _clamp(tCurr.X, range[0], range[1]);

                            var k = (x - nodes[0].X) / v.Length;

                            var p = nodes[0] + v * k;

                            // Pointer in screen
                            Pointer.Location = mat.Transform(p);
                            //var f = diff.Y > 0 ? 0.1 ;
                            //var line = UiState.Nodes[0].Value - UiState.Nodes[1].Value;
                            //var p = UiState.Nodes[0].Value + line * f;
                            //Pointer.Location = curr;
                        }
                        else {
                            Pointer.Location = curr;
                            if (VMTemp == null) {
                                var vm = DataContext as RegionControlUIViewModel;
                                var result = VisualTreeHelper.HitTest(_canvas, curr);

                                if (result != null) {
                                    //var state = LogicalTreeHelperExtensions.FindAncestor<IRegionShape>(result.VisualHit)?.State;

                                    if (mode == UiMode.Trace)
                                        vm.Interpolate(UiState, invMat.Transform(curr));
                                    else if (mode == UiMode.Drag)
                                        vm.UpdateControlUiValue(UiState, invMat.Transform(curr));

                                }
                            }
                            else if (VMTemp != null) {
                                var vm = VMTemp as RegionControlUIViewModel;
                                var result = VisualTreeHelper.HitTest(_canvas, curr);
                                var state = (LogicalTreeHelperExtensions.FindAncestor<IRegionShape>(result.VisualHit))?.State;

                                if (mode == UiMode.Trace)
                                    vm.Interpolate(UiState, invMat.Transform(curr));
                                else if (mode == UiMode.Drag)
                                    vm.UpdateControlUiValue(UiState, invMat.Transform(curr));
                            }
                        }
                    }
                }
            };

            MouseUp += (s, e) => {
                if (mode == UiMode.Add) {
                    OnMouseClicked(mousePosition);
                    GoToState(UiMode.Default);

                    ReleaseMouseCapture();
                }
                else if (mode == UiMode.Pan) {
                    var curr = e.GetPosition(_canvas);
                    var vector = (curr - start);

                    var tMat = Matrix.Parse(startMat.ToString());
                    tMat.Translate(vector.X, vector.Y);

                    Translate = tMat;

                    isDragging = false;
                    InvalidateTransform();

                    ReleaseMouseCapture();
                    GoToState(UiMode.Default);
                }
                else if (mode == UiMode.Trace || mode == UiMode.Drag) {
                    var last = e.GetPosition(_canvas);

                    isDragging = false;
                    Pointer.Location = last;

                    ReleaseMouseCapture();
                }

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
                    GoToState(UiMode.Add);
                }
                else if (e.Key == Key.D2) {
                    GoToState(UiMode.Pan);
                }
                else if (e.Key == Key.D3) {
                    GoToState(UiMode.Trace);
                }
                else if (e.Key == Key.D4) {
                    GoToState(UiMode.Drag);
                }
                else if (e.Key == Key.D5) {
                    Save("screenshot.png");
                }
                else if (e.Key == Key.Escape) {
                    GoToState(UiMode.Default);
                    isDragging = false;
                }
            };

            Keyboard.AddPreviewKeyDownHandler(this, (s, e) => {
                //Console.WriteLine($"Keydown, {e.Key}");
            });
        }

        public void GoToState(UiMode mode) {
            this.mode = mode;

            switch (mode) {
                case UiMode.Default:
                    State = new DefaultUiState(this);
                    break;
                case UiMode.Add:
                    break;
                case UiMode.Delete:
                    break;
                case UiMode.Build:
                    break;
                case UiMode.Trace:
                    State = new TraceUiState(this);
                    break;
                case UiMode.Drag:
                    State = new TraceUiState(this);
                    break;
                case UiMode.Pan:
                    break;
                case UiMode.Zoom:
                    break;
                default:
                    break;
            }

            // State operation
            State.SetFlags();

            NotifyStatus?.Invoke(this, new NotifyStatusEventArgs(mode.ToString()));
            _status.Content = mode.ToString();
        }

        public void Select(int nodeId) {
            var target = NodeVisuals.Where(e => e.NodeId == nodeId).First();

            target.GoToState(UiElementState.Selected);
            SetValue(SelectedNodePropertyKey, target);
        }

        public void UnSelect() {
            NodeVisuals.ToList().ForEach(e => e.GoToState(UiElementState.Default));
            SetValue(SelectedNodePropertyKey, null);
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

            //var ik = _canvas.Children.OfType<IKTemplate>().First();

            ////ik.Transform = Transform;
            //ik.Origin = new Point(ActualWidth / 2, ActualHeight / 2);

            // Invalidate widgets
            //Pointer.Transform = Transform;

            // Invalidate all nodes transformation
            foreach (var node in _canvas.Children.OfType<NodeShape>()) {
                node.Transform = Transform;
                //var point = node.Node.Location;
                //var tPoint = Transform.Transform(point);

                //Canvas.SetLeft(node, tPoint.X - 20 / 2);
                //Canvas.SetTop(node, tPoint.Y - 20 / 2);
            }

            foreach(var simplex in _canvas.Children.OfType<SimplexShape>()) {
                simplex.Transform = Transform;

                //simplex.Invalidate();
            }

            foreach(var voronoi in _canvas.Children.OfType<VoronoiShape>()) {
                voronoi.Transform = Transform;

                //voronoi.Invalidate();
            }
        }
    }

    public class PointerWidget : UserControl {
        private Matrix _transform = Matrix.Identity;
        private Point location = new Point();

        public Matrix Transform {
            get => _transform;
            set {
                var prevT = _transform;
                _transform = value;

                if (prevT != _transform) {
                    Invalidate();
                }
            }
        }

        public Point Location {
            get => location;
            set {
                location = value;

                Invalidate();
            }
        }
        public PointerWidget() { }

        public void Invalidate() {
            InvalidateContent();
            InvalidateTransform();
        }

        public void InvalidateContent() {
            var circle = new Ellipse {
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.Blue),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 1,
            };

            Content = circle;
        }

        public void InvalidateTransform() {
            var p = Location;
            var tP = Transform.Transform(p);

            Canvas.SetLeft(this, tP.X - 10 / 2);
            Canvas.SetTop(this, tP.Y - 10 / 2);
        }
    }

    public class VoronoiShape : UserControl, IRegionShape {
        public int UiId { get; set; }
        public BaseRegionState State { get; set; }

        private Matrix _transform = Matrix.Identity;
        public Matrix Transform {
            get => _transform;
            set {
                var prevT = _transform;
                _transform = value;

                if (prevT != _transform) {
                    Invalidate();
                }
            }
        }
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

        public VoronoiShape(int uiId, BaseRegionState state) {
            UiId = uiId;
            State = state;

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

    public interface IRegionShape {
        BaseRegionState State { get; set; }
    }

    public class SimplexShape : UserControl, IRegionShape {
        public int UiId { get; set; }
        public BaseRegionState State { get; set; }


        private Matrix _transform = Matrix.Identity;
        public Matrix Transform { 
            get => _transform;
            set {
                var prevT = _transform;
                _transform = value;

                if (prevT != _transform) {
                    Invalidate();
                }
            }
        }
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

        public SimplexShape(int uiId, BaseRegionState state) {
            UiId = uiId;
            State = state;

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

            // 3-simplex
            if (points.Length == 3) {
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
            // 2-simplex
            else if (points.Length == 2){
                var pathGeo = new PathGeometry();
                var pathFig = new PathFigure {
                    StartPoint = points[0],
                };

                pathGeo.Figures.Add(pathFig);
                pathFig.Segments.Add(new LineSegment { Point = points[1] });
                pathFig.Segments.Add(new LineSegment { Point = points[0] });


                var path = new Path {
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    Stretch = Stretch.None,
                    StrokeThickness = 2.0,
                    Data = pathGeo
                };

                Content = path;
            }
        }
    }

    public class NodeShape : StatefulWidget {
        public UIElement overlay;
        public UiElementState state = UiElementState.Default;
        public Color PrimaryColor;

        public Image checkIcon;
        
        private Matrix _transform = Matrix.Identity;

        public Matrix Transform { 
            get => _transform; 
            set {
                var prevT = _transform;
                _transform = value;

                if (prevT != _transform) {
                    Invalidate();
                }

            }
        }

        public static readonly RoutedEvent ClickedEvent = EventManager.RegisterRoutedEvent("Clicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NodeShape));

        public event RoutedEventHandler Clicked {
            add { AddHandler(ClickedEvent, value); }
            remove { RemoveHandler(ClickedEvent, value); }
        }

        public bool IsSelected { get; set; } = false;

        public NodeInfo Node {
            get { return (NodeInfo)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Node.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NodeProperty =
            DependencyProperty.Register("Node", typeof(NodeInfo), typeof(NodeShape), new PropertyMetadata(new NodeInfo(), OnPropertyChanged));


        public int NodeId { get; set; }

        public NodeShape(int nodeId) {
            NodeId = nodeId;

            InitializeComponents();
            Invalidate();
        }

        private void InitializeComponents() {
            _state = new DefaultNodeState(this);

            InitializeOverlay();
            InitializeContent();

            Content = Container;
        }

        public void Invalidate() {
            InvalidateContent();
            InvalidateTransform();
        }

        private void InvalidateContent() {
            PrimaryColor = ColorManager.GetTintedColor(ColorManager.Palette[Node.UiId], 2);
            ToolTip = $"Node[{Node.NodeId}]-({Node.Location.X}, {Node.Location.Y})";
            
            var shape = (Container as Grid).Children.OfType<Ellipse>().First();

            shape.Fill = new SolidColorBrush(PrimaryColor);
        }

        private void InvalidateTransform() {
            var p = Node.Location;
            var tP = Transform.Transform(p);

            Canvas.SetLeft(this, tP.X - 20 / 2);
            Canvas.SetTop(this, tP.Y - 20 / 2);
        }

        private void InitializeOverlay() {
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

        private void InitializeContent() {
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

        //protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
        //    base.OnMouseLeftButtonDown(e);
        //}

        //protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
        //    base.OnMouseLeftButtonUp(e);

        //    if (_state is HoverNodeState && !IsSelected) {
        //        GoToState(UiElementState.Selected);
        //    }
        //    else if (_state is SelectedNodeState && IsSelected) {
        //        GoToState(UiElementState.Default);
        //    }

        //    var parent = LogicalTreeHelperExtensions.FindAncestor<UiController>(this);

        //    parent.Select(NodeId);
        //    //RaiseEvent(new RoutedEventArgs(ClickedEvent));
        //}

        static public void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is NodeShape widget) {
                widget.Invalidate();
            }
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

    public class IKTemplate : UserControl {
        private ILogger logger => LogManager.GetCurrentClassLogger();


        public Matrix Transform {
            get { return (Matrix)GetValue(TransformProperty); }
            set { SetValue(TransformProperty, value); }
        }



        public Point Origin {
            get { return (Point)GetValue(OriginProperty); }
            set { SetValue(OriginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Origin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OriginProperty =
            DependencyProperty.Register("Origin", typeof(Point), typeof(IKTemplate), new PropertyMetadata(new Point(), OnPropertyChanged));


        // Using a DependencyProperty as the backing store for Transform.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransformProperty =
            DependencyProperty.Register("Transform", typeof(Matrix), typeof(IKTemplate), new PropertyMetadata(Matrix.Identity, OnPropertyChanged));

        static public void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            //(d as IKTemplate).InvalidateVisual();
            (d as IKTemplate).Invalidate();
        }

        private Canvas _canvas;

        public IKTemplate() {
            _canvas = new Canvas() {
                Width = 400,
                Height = 400,
                Background = Brushes.Tomato,
                Visibility = Visibility.Visible,
            };


            Canvas.SetLeft(this, 0);
            Canvas.SetTop(this, 0);

            Content = _canvas;
        }

        // save png
        public void Save(string filename) {
            var rootVisual = Content as Visual;
            var rtb = new RenderTargetBitmap((int)800, (int)800, 96, 96, PixelFormats.Pbgra32);

            rtb.Render(_canvas);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var stream = File.Create(filename)) {
                encoder.Save(stream);
            }
        }



        private Shape DrawPoint(Point point, Brush color, double radius) {
            // Create a new Ellipse object
            Ellipse ellipse = new Ellipse();

            // Set the center point and radius of the ellipse
            // the radius is 4
            ellipse.Width = radius * 2;
            ellipse.Height = radius * 2;
            //ellipse.Margin = new Thickness(point.X - 4, point.Y - 4, 0, 0);
            
            // Set the fill color of the ellipse
            ellipse.Fill = color;
            ellipse.StrokeThickness = 2;

            Canvas.SetLeft(ellipse, point.X - radius);
            Canvas.SetTop(ellipse, point.Y - radius);

            // Add the ellipse to the canvas
            return ellipse;
        }

        private Color GetColorWithGradient(Color startColor, Color endColor, int step, int numSteps) {
            double redIncrement = (endColor.R - startColor.R) / (double)(numSteps);
            double greenIncrement = (endColor.G - startColor.G) / (double)(numSteps);
            double blueIncrement = (endColor.B - startColor.B) / (double)(numSteps);

            //Color[] gradientColors = new Color[numSteps];
            //for (int i = 0; i < numSteps; i++) {
            //    byte red = (byte)(startColor.R + i * redIncrement);
            //    byte green = (byte)(startColor.G + i * greenIncrement);
            //    byte blue = (byte)(startColor.B + i * blueIncrement);
            //    gradientColors[i] = Color.FromRgb(red, green, blue);
            //}

            byte red = (byte)(startColor.R + step * redIncrement);
            byte green = (byte)(startColor.G + step * greenIncrement);
            byte blue = (byte)(startColor.B + step * blueIncrement);

            return Color.FromRgb(red, green, blue);
        }

        private void DrawTrajectory() {
            //var line = new ArrowLine(new Point(0,0), new Point(300, 0));

            //_canvas.Children.Add(line);

            var start = new Point();

            ArrowLine line;
            var numberOfPoints = 12;
            // generate 36 points
            // all points are on a circle with radius 100
            for (int i = 0; i <= numberOfPoints; i++) {
                var angle = i * (360 / numberOfPoints);
                var point = new Point(Math.Cos(angle * Math.PI / 180) * 100, Math.Sin(angle * Math.PI / 180) * 100);

                line = new ArrowLine(start, point) {
                    Stroke = new SolidColorBrush(GetColorWithGradient(Colors.Red, Colors.Yellow, i, numberOfPoints + 1))
                };
                _canvas.Children.Add(line);
                
                start = point;
            }

            line = new ArrowLine(start, new Point()) {
                Stroke = new SolidColorBrush(GetColorWithGradient(Colors.Red, Colors.Yellow, 37, numberOfPoints + 1))
            };
            _canvas.Children.Add(line);

        }

        // Method to make a tangent circle with three points
        // the center of circle is the origin
        private void MakeTangentCircle(double radius) {
            var circle = new Ellipse {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = Brushes.Linen,
                StrokeThickness = 2,
            };

            Canvas.SetLeft(circle, -radius);
            Canvas.SetTop(circle, -radius);
            
            _canvas.Children.Add(circle);

        }

        private Point p1;
        private Point p2;
        private Point p3;

        private void MakePoints() {
            // variable o is the origin
            var o = new Point(0, 0);

            // three point variables are p1, p2, p3
            // p1 is (0, 100)
            // the radius of the circle is 100
            // the center of the circle is (0, 0)

            p1 = new Point(0, 250);

            p2 = RotatePoint(p1, 120);
            p3 = RotatePoint(p1, -120);

            // draw points on _canvas
            _canvas.Children.Add(DrawPoint(o, Brushes.Red, 12));
            _canvas.Children.Add(DrawPoint(p1, Brushes.Black, 8));
            _canvas.Children.Add(DrawPoint(p2, Brushes.Green, 8));
            _canvas.Children.Add(DrawPoint(p3, Brushes.Blue, 8));

        }

        private Point RotatePoint(Point p, double degree) {
            var radian = degree * Math.PI / 180.0;

            double rotatedX = Math.Cos(radian) * p.X - Math.Sin(radian) * p.Y;
            double rotatedY = Math.Sin(radian) * p.X + Math.Cos(radian) * p.Y;


            return new Point(rotatedX, rotatedY);
        }

        private void Invalidate() {
            _canvas.Children.Clear();

            //var offset = Transform.Transform(new Point(0, 0));

            _canvas.RenderTransform = new MatrixTransform(new Matrix(1, 0, 0, -1, Origin.X, Origin.Y));


            MakePoints();
            DrawTriangle(p1, p2, p3);


            // calculate d that is the distance from origin o to the line p1 and p2
            var d = PerpendicularLength(new Point(), p1, p2);

            MakeTangentCircle(d);

            DrawTrajectory();

            //InvalidateVisual();
        }


        public static double PerpendicularLength(Point a, Point c, Point d) {
            double dx = d.X - c.X;
            double dy = d.Y - c.Y;
            double numerator = Math.Abs(dy * a.X - dx * a.Y + d.X * c.Y - d.Y * c.X);
            double denominator = Math.Sqrt(dx * dx + dy * dy);
            double length = numerator / denominator;
            return length;
        }

        // Method to draw a triangle from three points
        private void DrawTriangle(Point p1, Point p2, Point p3) {
            // Create a new Path object
            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
            // Create a StreamGeometry to use to specify my Path
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open()) {
                ctx.BeginFigure(p1, true /* is filled */, true /* is closed */);
                ctx.LineTo(p2, true /* is stroked */, false /* is smooth join */);
                ctx.LineTo(p3, true /* is stroked */, false /* is smooth join */);
            }
            // Freeze the geometry (make it unmodifiable)
            // for additional performance benefits
            geometry.Freeze();
            // specify the shape (triangle) of the path using the StreamGeometry
            path.Data = geometry;


            // set the fill color of the triangle
            // the fill color have 60% opacity
            var fillColor = Color.FromArgb(128, Brushes.Linen.Color.R, Brushes.Linen.Color.G, Brushes.Linen.Color.B);

            path.Fill = new SolidColorBrush(fillColor);
            

            // set the border of the triangle
            path.Stroke = Brushes.Black;
            path.StrokeThickness = 2;
            // Add the path to the canvas
            _canvas.Children.Add(path);
        }

    }

    public class ArrowLine : Shape {
        public ArrowLine(Point start, Point end) {
            Start = start;
            End = end;
            //Stroke = Brushes.Black;
            StrokeThickness = 2;
        }
        public Point Start {
            get { return (Point)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Start.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start", typeof(Point), typeof(ArrowLine), new PropertyMetadata(new Point(0, 0), OnPropertyChanged));
        public Point End {
            get { return (Point)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }
        // Using a DependencyProperty as the backing store for End.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End", typeof(Point), typeof(ArrowLine), new PropertyMetadata(new Point(0, 0), OnPropertyChanged));
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var arrow = (ArrowLine)d;
            arrow.Invalidate();
        }

        private void Invalidate() {
            var geometry = GetGeometry();
            Geometry = geometry;

            InvalidateVisual();
        }

        private Geometry GetGeometry() {
            var start = Start;
            var end = End;

            // Calculate the line segment vector
            Vector lineSegment = end - start;

            // Normalize the line segment vector
            Vector lineDirection = lineSegment;
            lineDirection.Normalize();

            // Calculate the perpendicular vector to the line segment vector
            Vector perpendicularVector = new Vector(-lineSegment.Y, lineSegment.X);
            perpendicularVector.Normalize();

            // Define the length and width of the arrowhead
            double arrowheadLength = 20;
            double arrowheadWidth = 8;

            // Calculate the endpoints of the arrowhead
            Point arrowheadEndpoint1 = end - (lineDirection * arrowheadLength) + (perpendicularVector * arrowheadWidth);
            Point arrowheadEndpoint2 = end - (lineDirection * arrowheadLength) - (perpendicularVector * arrowheadWidth);

            // Define the points of the arrow line
            PointCollection points = new PointCollection {
                start,
                end,
                arrowheadEndpoint1,
                arrowheadEndpoint2,
                end,
            };

            // Create PathFigure with points
            PathFigure figure = new PathFigure {
                StartPoint = start,
                Segments = new PathSegmentCollection {
                    new PolyLineSegment(points, true)
                },

            };

            // Create PathGeometry with PathFigure
            PathGeometry geometry = new PathGeometry {
                Figures = new PathFigureCollection {
                    figure
                }
            };

            return geometry;

        }

        protected override Geometry DefiningGeometry {
            get {
                return GetGeometry();
            }
        }

        protected Geometry Geometry { get; set; }
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
