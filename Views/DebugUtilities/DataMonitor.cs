using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Views.Widgets;
using SkiaSharp;
using System.Windows;

namespace taskmaker_wpf.Views.Debug {
    public class DataMonitorWidgetProps : IProps {
        public SKRect Bound { get; set; }
        public float[] Seqs { get; set; }
    }

    public class DataMonitorWidget : RenderWidget {
        public float[] Sequence {
            get { return (float[])GetValue(SequenceProperty); }
            set { SetValue(SequenceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Sequence.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SequenceProperty =
            DependencyProperty.Register(
                "Sequence",
                typeof(float[]),
                typeof(DataMonitorWidget),
                new PropertyMetadata(new float[] {}, OnPropertyChanged));

        public DataMonitorWidget(string name): base(name) { }

        protected override IProps GetProps() {
            return new DataMonitorWidgetProps {
                Seqs = Sequence
            };
        }
    }

    public class DataMonitorRenderObject
        : RenderObject<DataMonitorWidgetProps> {
        public DataMonitorRenderObject(DataMonitorWidgetProps props)
            : base(props) {
        }

        protected override void OnRender(SKCanvas canvas) {
            var stroke = new SKPaint {
                IsAntialias = true,
                StrokeWidth = 2,
                IsStroke = true,
                Color = SKColors.Black,
            };
            
            var path = new SKPath();

            var seq = _props.Seqs;
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
