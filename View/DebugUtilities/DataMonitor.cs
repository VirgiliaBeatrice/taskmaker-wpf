using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.View.Widgets;
using SkiaSharp;

namespace taskmaker_wpf.View.Debug {
    public struct DataMonitorState : IWidgetState {
        public SKRect Bound { get; set; }
        public float[] Seqs { get; set; } 
    }

    public class DataMonitorWidget : RenderWidget<DataMonitorState> {
        public DataMonitorWidget(string name, DataMonitorState initState) : base(name, initState) {
        }

        public override void SetState(IWidgetState state) {
            State = (DataMonitorState)state;
        }

        public override void Build() {
            RenderObject = new DataMonitorRenderObject(State);

            base.Build();
        }
    }

    public class DataMonitorRenderObject : RenderObject<DataMonitorState> {
        public DataMonitorRenderObject(DataMonitorState initState) : base(initState) {
        }

        protected override void OnRender(SKCanvas canvas, DataMonitorState state) {
            var stroke = new SKPaint {
                IsAntialias = true,
                StrokeWidth = 2,
                IsStroke = true,
                Color = SKColors.Black,
            };
            
            var path = new SKPath();

            var seq = state.Seqs;
            var step = 2;

            if (seq.Length > 1) {
                var max = seq.Max();
                var min = seq.Min();
                var factor = (max - min) / 100f;

                path.MoveTo(0, (seq[0] - min) /factor);
                seq.Skip(1)
                    .ToList()
                    .ForEach(e => {
                        path.LineTo(step, (e - min) / factor);
                        step += 2;
                    });

                canvas.DrawPath(path, stroke);
            }

            stroke.Dispose();
            path.Dispose();
        }
    }
}
