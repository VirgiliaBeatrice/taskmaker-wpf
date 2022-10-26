using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace taskmaker_wpf.Views {
    public class SimplexWidget : SKFrameworkElement {
        public Guid Id {
            get { return (Guid)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Id.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(Guid), typeof(SimplexWidget), new PropertyMetadata(Guid.Empty));



        public Point[] Points {
            get { return (Point[])GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(Point[]), typeof(SimplexWidget), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        private SKPath _shape;

        internal void CreateShape() {
            _shape?.Dispose();

            _shape = new SKPath();

            if (Points == null) return;

            var points = Points.Select(e => e.ToSKPoint()).ToArray();

            _shape.MoveTo(points[0]);
            _shape.LineTo(points[1]);
            _shape.LineTo(points[2]);
            _shape.Close();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters) {
            var pt = hitTestParameters.HitPoint;
            var skPt = pt.ToSKPoint();

            var wPt = (Parent as ComplexWidget).ViewPort.ViewportToWorld(skPt);

            if (_shape?.Contains(wPt.X, wPt.Y) == true)
                return new PointHitTestResult(this, pt);
            else
                return null;
        }

        protected override void OnMouseEnter(MouseEventArgs e) {
            (Parent as ComplexWidget)?.InvalidateSKContext();

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e) {
            (Parent as ComplexWidget)?.InvalidateSKContext();

            base.OnMouseLeave(e);
        }


        public override void Draw(SKCanvas canvas) {
            canvas.Save();

            var t = (Parent as ComplexWidget).ViewPort.GetTranslate();

            canvas.SetMatrix(t);

            var stroke = new SKPaint {
                IsAntialias = true,
                StrokeWidth = 2,
                IsStroke = true,
                Color = SKColors.Black,
            };
            var fill = new SKPaint {
                IsAntialias = true,
                Color = SKColors.Bisque
            };

            CreateShape();

            if (IsMouseOver)
                fill.Color = SKColors.BlanchedAlmond;

            canvas.DrawPath(_shape, stroke);
            canvas.DrawPath(_shape, fill);

            canvas.Restore();

            stroke.Dispose();
            fill.Dispose();
        }
    }
}
