using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace taskmaker_wpf.Views.Widgets.Container {
    public class VoronoiWidget_wpf : FrameworkElement {
        private SKPath _region;
        private Point[] Points = new Point[] { };

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters) {
            var pt = hitTestParameters.HitPoint;
            var skPt = pt.ToSKPoint();

            if (_region?.Contains(skPt.X, skPt.Y) == true)
                return new PointHitTestResult(this, pt);
            else
                return null;
        }

        protected WriteableBitmap OnSKRender(SKImageInfo info) {
            var bitmap = new SKBitmap(info);
            var canvas = new SKCanvas(bitmap);

            var points = Points.Cast<SKPoint>().ToArray();
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

            if (points.Length == 4) {
                path.MoveTo(points[0]);
                path.LineTo(points[1]);
                path.LineTo(points[2]);
                path.LineTo(points[3]);
                path.Close();
            }
            else if (points.Length == 3) {
                var radius = (points[1] - points[0]).Length;

                path.MoveTo(points[0]);
                path.LineTo(points[1]);
                path.LineTo(points[2]);
                //path.AddArc(new SKRect(0,0, radius, radius), )
                path.Close();
            }

            canvas.DrawPath(path, stroke);

            _region?.Dispose();
            _region = path;

            var ret = bitmap.ToWriteableBitmap();

            stroke.Dispose();
            fill.Dispose();
            path.Dispose();

            return ret;
        }
    }

    public class VoronoiWidgetProps : IProps {
        public SKRect Bound { get; set; } = new SKRect {
            Right = 100,
            Bottom = 100
        };
        public IEnumerable Points { get; set; }
    }

    public class VoronoiWidgetRenderObject : RenderObject<VoronoiWidgetProps> {
        public VoronoiWidgetRenderObject(VoronoiWidgetProps props) : base(props) {
        }


    }

    public class VoronoiWidget : RenderWidget {
        public VoronoiWidget(string name) : base(name) {
        }

        public SKPoint[] Points {
            get { return (SKPoint[])GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(SKPoint[]), typeof(VoronoiWidget), new PropertyMetadata(null, OnPropertyChanged));


    }

    //public class VoronoiWidget : RenderWidget<VoronoiState> {
    //    public VoronoiWidget(string name, VoronoiState initState) : base(name, initState) {
    //        ModelHash = initState.Hash;
    //    }

    //    public override bool Contains(SKPoint p) {
    //        using (var path = new SKPath()) {
    //            if (State.Points.Length == 4) {
    //                path.MoveTo(State.Points[0]);
    //                path.LineTo(State.Points[1]);
    //                path.LineTo(State.Points[2]);
    //                path.LineTo(State.Points[3]);
    //                path.Close();
    //            }
    //            else if (State.Points.Length == 3) {
    //                var radius = (State.Points[1] - State.Points[0]).Length;

    //                path.MoveTo(State.Points[0]);
    //                path.LineTo(State.Points[1]);
    //                path.LineTo(State.Points[2]);
    //                //path.AddArc(new SKRect(0,0, radius, radius), )
    //                path.Close();
    //            }

    //            var result = path.Contains(p.X, p.Y);

    //            return result;
    //        }
    //    }
    //}
}
