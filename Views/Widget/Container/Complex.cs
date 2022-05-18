using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace taskmaker_wpf.Views.Widgets.Container {
    public class ComplexWidgetProps : IProps {
        public SKRect Bound { get; set; } = new SKRect { Right = 0, Bottom = 0 };
        public IEnumerable Nodes { get; set; }
        public IEnumerable Simplices { get; set; }
        public IEnumerable Voronois { get; set; }
    }

    public class ComplexWidgetRenderObject : RenderObject<ComplexWidgetProps> {
        public ComplexWidgetRenderObject(ComplexWidgetProps props) : base(props) {
        }

        protected override void OnRender(SKCanvas canvas) { }
    }

    public class ComplexWidget : RenderWidget {
        public IEnumerable NodeSource {
            get { return (IEnumerable)GetValue(NodeSourceProperty); }
            set { SetValue(NodeSourceProperty, value); }
        }

        public IEnumerable SimplexSource {
            get { return (IEnumerable)GetValue(SimplexSourceProperty); }
            set { SetValue(SimplexSourceProperty, value); }
        }

        public IEnumerable VoronoiSource {
            get { return (IEnumerable)GetValue(VoronoiSourceProperty); }
            set { SetValue(VoronoiSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VoronoiSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VoronoiSourceProperty =
            DependencyProperty.Register("VoronoiSource", typeof(IEnumerable), typeof(ComplexWidget), new PropertyMetadata(null, OnPropertyChanged));



        // Using a DependencyProperty as the backing store for SimplexSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SimplexSourceProperty =
            DependencyProperty.Register(
                "SimplexSource",
                typeof(IEnumerable),
                typeof(ComplexWidget),
                new PropertyMetadata(null, OnPropertyChanged));



        // Using a DependencyProperty as the backing store for NodeSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NodeSourceProperty =
            DependencyProperty.Register(
                "NodeSource",
                typeof(IEnumerable),
                typeof(ComplexWidget),
                new PropertyMetadata(
                    null, OnPropertyChanged_ComplexWidget));

        private static void OnPropertyChanged_ComplexWidget(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is ComplexWidget cw) {
                if (e.OldValue is INotifyCollectionChanged oldCollection) {
                    oldCollection.CollectionChanged -= cw.CollectionChanged;
                }

                if (e.NewValue is INotifyCollectionChanged newCollection) {
                    newCollection.CollectionChanged += cw.CollectionChanged;
                }
            }
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add) {
                foreach(var item in e.NewItems) {
                    var idx = e.NewStartingIndex;
                    var newNode = new NodeWidget("Node" + idx) {
                        DataContext = DataContext
                    };

                    AddChild(newNode);

                    var bind = new Binding {
                        Source = DataContext,
                        Path = new PropertyPath($"Nodes_v1[{idx}].Location")
                    };

                    BindingOperations.SetBinding(
                        newNode,
                        NodeWidget.LocationProperty,
                        bind);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove) {
                foreach(var item in e.OldItems) {
                    var idx = e.OldStartingIndex;

                    Remove("Node" + idx);
                }
            }
        }

        ~ComplexWidget() {
            if (NodeSource is INotifyCollectionChanged collection) {
                collection.CollectionChanged -= CollectionChanged;
            }
        }

        public ComplexWidget(string name) : base(name) {
        }

        protected override IProps GetProps() {
            return new ComplexWidgetProps {
                Nodes = NodeSource,
                Simplices = SimplexSource,
                Voronois = VoronoiSource,
            };
        }

        public override bool HitTest(SKPoint point) {
            return true;
        }
    }


    //public class ComplexWidget : 
    //    StatefulWidget<ComplexWidgetState>,
    //    IContainerWidget {
    //    //public ComplexWidgetState State { get; set; }
    //    private SKRect _clipBound;
    //    private int _layer;

    //    public ComplexWidget(string name, ComplexWidgetState initState) : base(name) {
    //        State = initState;
    //        _clipBound = new SKRect(0, 0, State.width, State.height);

    //        ModelHash = initState.Hash;
    //    }

    //    //static Stopwatch w = Stopwatch.StartNew();
    //    public override void Build() {
    //        base.Build();

    //        RemoveAll();

    //        for(var idx = 0; idx < State.nodes.Length; idx++) {
    //            var state = NodeState.Default;
    //            state.location = State.nodes[idx].location;
    //            var node = new NodeWidget(
    //                $"Node_{idx}",
    //                state);

    //            AddChild(node);
    //            node.Build();
    //        }

    //        if (State.simplices != null) {
    //            for(var idx = 0; idx < State.simplices.Length; idx++) {
    //                var state = SimplexState.Default;
    //                state.Hash = State.simplices[idx].Item1;
    //                state.Points = State.simplices[idx].Item2;
    //                var simplex = new SimplexWidget(
    //                    $"Simplex_{idx}", state);

    //                AddChild(simplex);
    //                simplex.Build();
    //            }
    //        }

    //        if (State.voronois != null) {
    //            for (var idx = 0; idx < State.voronois.Length; idx++) {
    //                var state = VoronoiState.Default;
    //                state.Hash = State.voronois[idx].Item1;
    //                state.Points = State.voronois[idx].Item2;
    //                var voronoi = new VoronoiWidget(
    //                    $"VoronoiRegion_{idx}", state);

    //                AddChild(voronoi);
    //                voronoi.Build();
    //            }
    //        }
    //    }

    //    public void OnPainting(SKCanvas canvas) {
    //        _layer = canvas.Save();

    //        var transform = SKMatrix.CreateTranslation(State.location.X, State.location.Y);

    //        canvas.SetMatrix(transform);
    //        canvas.ClipRect(_clipBound);

    //        if (RenderObject != null)
    //            canvas.DrawPicture(RenderObject.Picture);
    //    }

    //    public void OnPainted(SKCanvas canvas) {
    //        canvas.RestoreToCount(_layer);
    //    }
    //}
}
