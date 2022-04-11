using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace taskmaker_wpf.Views.Widgets.Container {
    public struct ComplexWidgetState :
        IWidgetState, ICloneable {
        public float width;
        public float height;
        public SKPoint location;
        public ViewModels.Node[] nodes;
        public (int, SKPoint[])[] simplices;
        public (int, SKPoint[])[] voronois;

        public SKRect Bound { get; set; }
        public int Hash { get; set; }

        public object Clone() {
            return new ComplexWidgetState {
                width = width,
                height = height,
                location = location,
                nodes = (ViewModels.Node[])nodes.Clone(),
                simplices = simplices,
                voronois = voronois,
                Hash = Hash,
            };
        }
    }

    public class ComplexWidget : 
        StatefulWidget<ComplexWidgetState>,
        IContainerWidget {
        //public ComplexWidgetState State { get; set; }
        private SKRect _clipBound;
        private int _layer;

        public ComplexWidget(string name, ComplexWidgetState initState) : base(name) {
            State = initState;
            _clipBound = new SKRect(0, 0, State.width, State.height);

            ModelHash = initState.Hash;
        }

        //static Stopwatch w = Stopwatch.StartNew();
        public override void Build() {
            base.Build();

            RemoveAll();

            for(var idx = 0; idx < State.nodes.Length; idx++) {
                var state = NodeState.Default;
                state.location = State.nodes[idx].location;
                var node = new NodeWidget(
                    $"Node_{idx}",
                    state);

                AddChild(node);
                node.Build();
            }

            if (State.simplices != null) {
                for(var idx = 0; idx < State.simplices.Length; idx++) {
                    var state = SimplexState.Default;
                    state.Hash = State.simplices[idx].Item1;
                    state.Points = State.simplices[idx].Item2;
                    var simplex = new SimplexWidget(
                        $"Simplex_{idx}", state);

                    AddChild(simplex);
                    simplex.Build();
                }
            }

            if (State.voronois != null) {
                for (var idx = 0; idx < State.voronois.Length; idx++) {
                    var state = VoronoiState.Default;
                    state.Hash = State.voronois[idx].Item1;
                    state.Points = State.voronois[idx].Item2;
                    var voronoi = new VoronoiWidget(
                        $"VoronoiRegion_{idx}", state);

                    AddChild(voronoi);
                    voronoi.Build();
                }
            }
        }

        public void OnPainting(SKCanvas canvas) {
            _layer = canvas.Save();

            var transform = SKMatrix.CreateTranslation(State.location.X, State.location.Y);

            canvas.SetMatrix(transform);
            canvas.ClipRect(_clipBound);

            if (RenderObject != null)
                canvas.DrawPicture(RenderObject.Picture);
        }

        public void OnPainted(SKCanvas canvas) {
            canvas.RestoreToCount(_layer);
        }

        public void Layout() {
            throw new NotImplementedException();
        }

        public override void Paint(SKCanvas canvas) { }

        public override void SetState(IWidgetState state) {
            State = (ComplexWidgetState)state;
        }
    }
}
