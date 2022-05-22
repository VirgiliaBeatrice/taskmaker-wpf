using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace taskmaker_wpf.Views {
    public class VoronoiWidget : FrameworkElement {
        private SKPath _region;

        public Guid Id {
            get { return (Guid)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Id.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(Guid), typeof(VoronoiWidget), new PropertyMetadata(Guid.Empty));

        public Point[] Points {
            get { return (Point[])GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(Point[]), typeof(VoronoiWidget), new PropertyMetadata(null));

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters) {
            var pt = hitTestParameters.HitPoint;
            var skPt = pt.ToSKPoint();

            if (_region?.Contains(skPt.X, skPt.Y) == true)
                return new PointHitTestResult(this, pt);
            else
                return null;
        }

        protected void OnCreateRegion() {
            _region?.Dispose();

            _region = new SKPath();

            var points = Points.Select(e => e.ToSKPoint()).ToArray();

            if (points.Length == 4) {
                _region.MoveTo(points[0]);
                _region.LineTo(points[1]);
                _region.LineTo(points[2]);
                _region.LineTo(points[3]);
                _region.Close();
            }
            else if (points.Length == 3) {
                var radius = (points[1] - points[0]).Length;

                //_region.MoveTo(points[0]);
                //_region.LineTo(points[1]);
                //_region.LineTo(points[2]);
                ////_region.AddArc(new SKRect(0,0, radius, radius), )
                //_region.Close();

                var o = points[1];
                var p0 = points[0];
                var p1 = points[2];

                var p0o = (p0 - o);
                var p1o = (p1 - o);
                var dotProd = (p0o.X * p1o.X) + (p0o.Y * p1o.Y);
                var alpha = Math.Abs(Math.Acos(dotProd / (p0o.Length * p1o.Length)));

                var midLen = (float)Math.Tan(alpha / 2.0f) * Math.Abs(p0o.Length);

                var op0 = SKPoint.Normalize(o - p0);
                var midP0 = SKMatrix.CreateRotation((float)(Math.PI * 90.0 / 180.0)).MapVector(op0);
                midP0.X = midP0.X * midLen;
                midP0.Y = midP0.Y * midLen;

                var mid = p0 + midP0;

                _region.MoveTo(o);
                _region.LineTo(p0);
                _region.ArcTo(mid, p1, radius);
                _region.Close();
            }
        }

        protected WriteableBitmap OnSKRender(SKImageInfo info) {
            var bitmap = new SKBitmap(info);
            var canvas = new SKCanvas(bitmap);

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

            canvas.DrawPath(_region, stroke);

            var ret = bitmap.ToWriteableBitmap();

            stroke.Dispose();
            fill.Dispose();

            return ret;
        }

        protected override void OnRender(DrawingContext drawingContext) {
            OnCreateRegion();

            var bitmap = OnSKRender(new SKImageInfo((int)(Parent as Canvas).ActualWidth, (int)(Parent as Canvas).ActualHeight));

            drawingContext.PushTransform(new TranslateTransform());
            drawingContext.DrawImage(bitmap, new Rect(0, 0, (Parent as Canvas).ActualWidth, (int)(Parent as Canvas).ActualHeight));
            drawingContext.Pop();

            //base.OnRender(drawingContext);
        }
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
