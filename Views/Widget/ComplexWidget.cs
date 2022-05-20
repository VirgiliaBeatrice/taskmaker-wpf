using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace taskmaker_wpf.Views {
    public class ComplexWidget_Wpf : UserControl {
        public ComplexWidget_Wpf() { }
    }

    public class NodeWidget_Wpf : UserControl {




        //public Point Location {
        //    get { return (Point)GetValue(LocationProperty); }
        //    set { SetValue(LocationProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Location.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty LocationProperty =
        //    DependencyProperty.Register("Location", typeof(Point), typeof(NodeWidget_Wpf), new PropertyMetadata(new Point(250, 250)));



        public bool IsSelected {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(NodeWidget_Wpf), new PropertyMetadata(false, OnPropertyChanged));


        protected static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            (d as UserControl).InvalidateVisual();
        }


        public NodeWidget_Wpf() { }

        protected override void OnRender(DrawingContext drawingContext) {
            var info = new SKBitmap(20, 20);
            var canvas = new SKCanvas(info);
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

            if (ismoseover)
                fill.Color = SKColors.AliceBlue;

            canvas.DrawCircle(new SKPoint(10, 10), 5, stroke);
            canvas.DrawCircle(new SKPoint(10, 10), 5, fill);


            stroke.Dispose();
            fill.Dispose();
            canvas.Dispose();

            var bitmap = info.ToWriteableBitmap();
            info.Dispose();
            //bitmap.Unlock();

            drawingContext.DrawImage(bitmap, new Rect(0, 0, 20, 20));

            //if (Width > 0 && Height > 0) {
            //    if (_bitmap == null || _bitmap.Width != _width || _bitmap.Height != _height) {
            //        _bitmap = new WriteableBitmap(_width, _height, 96, 96, PixelFormats.Pbgra32, null);
            //    }

            //    _bitmap.Lock();
            //    using (var surface = SKSurface.Create(_width, _height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, _bitmap.BackBuffer, _bitmap.BackBufferStride)) {
            //        var canvas = surface.Canvas;
            //        canvas.Scale((float)_dpiX, (float)_dpiY);
            //        canvas.Clear();
            //        using (new SKAutoCanvasRestore(canvas, true)) {
            //            Presenter.Render(canvas, Renderer, Container, _offsetX, _offsetY);
            //        }
            //    }
            //    _bitmap.AddDirtyRect(new Int32Rect(0, 0, _width, _height));
            //    _bitmap.Unlock();

            //    drawingContext.DrawImage(_bitmap, new Rect(0, 0, _actualWidth, _actualHeight));
            //}

            //base.OnRender(drawingContext);
        }
    }
}
