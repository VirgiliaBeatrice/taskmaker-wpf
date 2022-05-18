using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace taskmaker_wpf.Views.Widgets.Container {
    public class SimplexWidgetProps : IProps {
        public SKRect Bound { get; set; } = new SKRect() {
            Right = 100,
            Bottom = 100
        };
        public IEnumerable Points { get; set; } 
    }

    public class SimplexWidgetRenderObject : RenderObject<SimplexWidgetProps> {
        public SimplexWidgetRenderObject(SimplexWidgetProps props) : base(props) {
        }

        protected override void OnRender(SKCanvas canvas) {
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
            var points = _props.Points.Cast<SKPoint>().ToArray();

            path.MoveTo(points[0]);
            path.LineTo(points[1]);
            path.LineTo(points[2]);
            path.Close();

            canvas.DrawPath(path, stroke);

            stroke.Dispose();
            fill.Dispose();
            path.Dispose();
        }
    }

    //public class SimplexWidget : RenderWidget<SimplexState> {
    //    public SimplexWidget(string name, SimplexState initState) : base(name, initState) {
    //        ModelHash = initState.Hash;
    //    }

    //    public override bool Contains(SKPoint p) {
    //        using (var path = new SKPath()) {
    //            path.MoveTo(State.Points[0]);
    //            path.LineTo(State.Points[1]);
    //            path.LineTo(State.Points[2]);
    //            path.Close();

    //            var result = path.Contains(p.X, p.Y);

    //            return result;
    //        }
    //    }

    //    public override void Build() {
    //        RenderObject = new SimplexRenderObject(State);

    //        base.Build();
    //    }
    //}
}
