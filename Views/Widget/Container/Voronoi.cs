using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace taskmaker_wpf.Views.Widgets.Container {
    public struct VoronoiState : IWidgetState {
        public SKRect Bound { get; set; }
        public SKPoint[] Points { get; set; }
        public int Hash { get; set; }

        static public VoronoiState Default => new VoronoiState {
            Bound = new SKRect(0, 0, 100, 100),
            Points = new SKPoint[0],
            Hash = -1
        };
    }

    public class VoronoiWidget : RenderWidget<VoronoiState> {
        public VoronoiWidget(string name, VoronoiState initState) : base(name, initState) {
            ModelHash = initState.Hash;
        }

        public override bool Contains(SKPoint p) {
            using (var path = new SKPath()) {
                if (State.Points.Length == 4) {
                    path.MoveTo(State.Points[0]);
                    path.LineTo(State.Points[1]);
                    path.LineTo(State.Points[2]);
                    path.LineTo(State.Points[3]);
                    path.Close();
                }
                else if (State.Points.Length == 3) {
                    var radius = (State.Points[1] - State.Points[0]).Length;

                    path.MoveTo(State.Points[0]);
                    path.LineTo(State.Points[1]);
                    path.LineTo(State.Points[2]);
                    //path.AddArc(new SKRect(0,0, radius, radius), )
                    path.Close();
                }

                var result = path.Contains(p.X, p.Y);
                
                return result;
            }
        }

        public override void Build() {
            RenderObject = new VoronoiRenderObject(State);

            base.Build();
        }
    }

    public class VoronoiRenderObject : RenderObject<VoronoiState> {
        public VoronoiRenderObject(VoronoiState initState) : base(initState) {
        }

        protected override void OnRender(SKCanvas canvas, VoronoiState state) {
            var stroke = new SKPaint {
                IsAntialias = true,
                StrokeWidth = 2,
                IsStroke = true,
                Color = SKColors.Black,
            };
            var fill = new SKPaint {
                IsAntialias = true,
                Color = SKColors.YellowGreen
            };

            var path = new SKPath();

            if (state.Points.Length == 4) {
                path.MoveTo(state.Points[0]);
                path.LineTo(state.Points[1]);
                path.LineTo(state.Points[2]);
                path.LineTo(state.Points[3]);
                path.Close();
            }
            else if (state.Points.Length == 3) {
                var radius = (state.Points[1] - state.Points[0]).Length;

                path.MoveTo(state.Points[0]);
                path.LineTo(state.Points[1]);
                path.LineTo(state.Points[2]);
                //path.AddArc(new SKRect(0,0, radius, radius), )
                path.Close();
            }

            canvas.DrawPath(path, stroke);

            stroke.Dispose();
            fill.Dispose();
            path.Dispose();
        }
    }
}
