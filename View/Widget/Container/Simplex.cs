using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace taskmaker_wpf.View.Widgets.Container {

    public struct SimplexState : IWidgetState {
        public SKRect Bound { get; set; }
        public SKPoint[] Points { get; set; }
        public int Hash { get; set; }

        public static SimplexState Default => new SimplexState {
            Bound = new SKRect(0, 0, 100, 100),
            Points = new SKPoint[0],
            Hash = -1
        };
    }

    public class SimplexWidget : RenderWidget<SimplexState> {
        public SimplexWidget(string name, SimplexState initState) : base(name, initState) {
            ModelHash = initState.Hash;
        }

        public override bool Contains(SKPoint p) {
            using (var path = new SKPath()) {
                path.MoveTo(State.Points[0]);
                path.LineTo(State.Points[1]);
                path.LineTo(State.Points[2]);
                path.Close();

                var result = path.Contains(p.X, p.Y);

                return result;
            }
        }

        public override void Build() {
            RenderObject = new SimplexRenderObject(State);

            base.Build();
        }
    }

    public class SimplexRenderObject : RenderObject<SimplexState> {
        public SimplexRenderObject(SimplexState initState) : base(initState) {
        }

        protected override void OnRender(SKCanvas canvas, SimplexState state) {
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

            path.MoveTo(state.Points[0]);
            path.LineTo(state.Points[1]);
            path.LineTo(state.Points[2]);
            path.Close();

            canvas.DrawPath(path, stroke);

            stroke.Dispose();
            fill.Dispose();
            path.Dispose();
        }
    }
}
