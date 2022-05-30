using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace taskmaker_wpf.Views {
    public class IndicatorWidget : SKFrameworkElement {
        protected override void Draw(SKCanvas canvas) {

            using (var fill = new SKPaint())
            using (var stroke = new SKPaint()) {
                stroke.IsAntialias = true;
                stroke.StrokeWidth = 2;
                stroke.IsStroke = true;
                stroke.Color = SKColors.Black;

                fill.IsAntialias = true;
                fill.Color = SKColors.Aquamarine;

                canvas.DrawCircle(new SKPoint(10, 10), 5, stroke);
                canvas.DrawCircle(new SKPoint(10, 10), 5, fill);
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
