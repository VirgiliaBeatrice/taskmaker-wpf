using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Views.Widgets;
using SkiaSharp;

namespace taskmaker_wpf.Views.Debug {
    public class DataMonitorWidgetProps : IProps {
        public SKRect Bound { get; set; }

    }

    public class DataMonitorWidget : RenderWidget {
        public DataMonitorWidget(string name) { Name = name; }
    }

    public class DataMonitorRenderObject
        : RenderObject_Wpf<DataMonitorWidgetProps> {
        public DataMonitorRenderObject(DataMonitorWidgetProps props)
            : base(props) {
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
