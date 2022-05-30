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
    public class SKFrameworkElement : FrameworkElement {
        //private WriteableBitmap _bitmap;

        public SKFrameworkElement() {
            SizeChanged += SKFrameWorkElement_SizeChanged;
        }

        private void SKFrameWorkElement_SizeChanged(object sender, SizeChangedEventArgs e) {
            InvalidateVisual();
        }

        protected virtual void Draw(SKCanvas canvas) { }

        protected override void OnRender(DrawingContext drawingContext) {
            (Parent as ComplexWidget).ViewPort.Render(Draw);
        }
    }

        public class VoronoiWidget : SKFrameworkElement {
        private SKPath _shape;

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
            DependencyProperty.Register("Points", typeof(Point[]), typeof(VoronoiWidget), new FrameworkPropertyMetadata(new Point[] { }, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnPropertyChanged_Points)));

        private static void OnPropertyChanged_Points(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            (d as VoronoiWidget).CreateShape();
        }

        public VoronoiWidget() : base() { }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters) {
            var pt = hitTestParameters.HitPoint;
            var skPt = pt.ToSKPoint();

            if (_shape?.Contains(skPt.X, skPt.Y) == true)
                return new PointHitTestResult(this, pt);
            else
                return null;
        }

        internal void CreateShape() {
            _shape?.Dispose();

            _shape = new SKPath();

            if (Points == null) return;

            var points = Points.Select(e => e.ToSKPoint()).ToArray();

            if (points.Length == 4) {
                _shape.MoveTo(points[0]);
                _shape.LineTo(points[1]);
                _shape.LineTo(points[2]);
                _shape.LineTo(points[3]);
                _shape.Close();
            }
            else if (points.Length == 3) {
                var radius = (points[1] - points[0]).Length;
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
                midP0.X *= midLen;
                midP0.Y *= midLen;

                var mid = p0 + midP0;

                _shape.MoveTo(o);
                _shape.LineTo(p0);
                _shape.ArcTo(mid, p1, radius);
                _shape.LineTo(p1);
                _shape.Close();
            }
        }

        protected override void Draw(SKCanvas canvas) {
            canvas.Save();

            var t = (Parent as ComplexWidget).ViewPort.GetTranslate();

            canvas.SetMatrix(t);

            var stroke = new SKPaint {
                IsAntialias = true,
                StrokeWidth = 1,
                IsStroke = true,
                Color = SKColors.Black,
            };
            var fill = new SKPaint {
                IsAntialias = true,
                Color = SKColors.Bisque
            };

            CreateShape();

            if (_shape.PointCount == 6) {
                var transparent = SKColors.Bisque.WithAlpha(0);
                fill.Shader = SKShader.CreateRadialGradient(
                    _shape.Points[0],
                    (_shape.Points[1] - _shape.Points[0]).Length,
                    new SKColor[] { SKColors.Bisque, transparent },
                    SKShaderTileMode.Clamp);
            }
            else {
                var transparent = SKColors.Bisque.WithAlpha(0);

                fill.Shader = SKShader.CreateLinearGradient(
                    _shape.Points[1],
                    _shape.Points[2],
                    new SKColor[] { SKColors.Bisque, transparent },
                    SKShaderTileMode.Clamp);
            }


            canvas.DrawPath(_shape, fill);
            canvas.DrawPath(_shape, stroke);

            canvas.Restore();

            stroke.Dispose();
            fill.Dispose();
        }
    }
}
