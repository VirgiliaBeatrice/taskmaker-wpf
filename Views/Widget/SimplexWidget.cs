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

namespace taskmaker_wpf.Views.Widgets {
    public class SimplexWidget : FrameworkElement {
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
            DependencyProperty.Register("Points", typeof(Point[]), typeof(SimplexWidget), new PropertyMetadata(null));


        protected override void OnRender(DrawingContext drawingContext) {
            var path = new SKPath();
            var points = Points.Select(e => e.ToSKPoint()).ToArray();

            path.MoveTo(points[0]);
            path.LineTo(points[1]);
            path.LineTo(points[2]);
            path.Close();

            _region?.Dispose();
            _region = path;

            var bitmap = OnSKRender(new SKImageInfo((int)(Parent as Canvas).ActualWidth, (int)(Parent as Canvas).ActualHeight));

            drawingContext.PushTransform(new TranslateTransform());
            drawingContext.DrawImage(bitmap, new Rect(0, 0, (Parent as Canvas).ActualWidth, (int)(Parent as Canvas).ActualHeight));
            drawingContext.Pop();

            //base.OnRender(drawingContext);
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters) {
            var pt = hitTestParameters.HitPoint;
            var skPt = pt.ToSKPoint();

            if (_region?.Contains(skPt.X, skPt.Y) == true)
                return new PointHitTestResult(this, pt);
            else
                return null;
        }

        private SKPath _region;

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
                Color = SKColors.Bisque
            };

            //var path = new SKPath();
            //var points = Points.Select(e => e.ToSKPoint()).ToArray();

            //path.MoveTo(points[0]);
            //path.LineTo(points[1]);
            //path.LineTo(points[2]);
            //path.Close();

            canvas.DrawPath(_region, stroke);
            canvas.DrawPath(_region, fill);

            //_region?.Dispose();
            //_region = path;

            var ret = bitmap.ToWriteableBitmap();

            stroke.Dispose();
            fill.Dispose();
            canvas.Dispose();
            bitmap.Dispose();

            return ret;
        }
    }
}
