using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace taskmaker_wpf.Views {
    public class CursorWidget : SKFrameworkElement {


        public Point Location {
            get { return (Point)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Location.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register("Location", typeof(Point), typeof(CursorWidget), new PropertyMetadata(new Point()));

        public override void Draw(SKCanvas canvas) {
            using (var axis = new SKPaint())
            using (var circle = new SKPaint()) {
                axis.IsAntialias = true;
                axis.StrokeWidth = 2;
                axis.Style = SKPaintStyle.Stroke;
                axis.Color = SKColors.Black;

                var effect = SKPathEffect.CreateDash(
                    new[] { 4.0f, 4.0f },
                    0);

                circle.IsAntialias = true;
                circle.StrokeWidth = 2;
                circle.Style = SKPaintStyle.Stroke;
                circle.Color = SKColors.White;


                canvas.DrawCircle(Location.ToSKPoint(), 10.0f, circle);

                circle.PathEffect = effect;
                circle.Color = SKColors.Red;
                canvas.DrawCircle(Location.ToSKPoint(), 10.0f, circle);

                var a = Location.ToSKPoint() - new SKPoint(15, 0);
                var b = Location.ToSKPoint() + new SKPoint(15, 0);
                var c = Location.ToSKPoint() - new SKPoint(0, 15);
                var d = Location.ToSKPoint() + new SKPoint(0, 15);

                canvas.DrawLine(a, b, axis);
                canvas.DrawLine(c, d, axis);

                effect.Dispose();
            }
        }
    }


    public class IndicatorWidget : SKFrameworkElement {
        public Point Location {
            get { return (Point)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Location.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register(
                "Location",
                typeof(Point),
                typeof(IndicatorWidget),
                new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender));



        public override void Draw(SKCanvas canvas) {
            if (Visibility == Visibility.Hidden) return;

            using (var fill = new SKPaint())
            using (var stroke = new SKPaint()) {
                stroke.IsAntialias = true;
                stroke.StrokeWidth = 2;
                stroke.IsStroke = true;
                stroke.Color = SKColors.Black;

                fill.IsAntialias = true;
                fill.Color = SKColors.Aquamarine;

                canvas.DrawCircle(Location.ToSKPoint(), 5, stroke);
                canvas.DrawCircle(Location.ToSKPoint(), 5, fill);
            }
                

            //var stroke = new SKPaint {
            //    IsAntialias = true,
            //    StrokeWidth = 2,
            //    IsStroke = true,
            //    Color = SKColors.Black,
            //};

            //var fill = new SKPaint {
            //    IsAntialias = true,
            //    Color = SKColors.Aquamarine
            //};

            //if (IsMouseOver)
            //    stroke.Color = SKColors.AliceBlue;
            //if (IsSelected)
            //    fill.Color = SKColors.AliceBlue;
        }

    }
}
