using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace taskmaker_wpf.Views {
    public class NodeWidget : FrameworkElement {


        public Guid Id {
            get { return (Guid)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Id.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(Guid), typeof(NodeWidget), new PropertyMetadata(Guid.Empty));


        public bool IsDirty {
            get { return (bool)GetValue(IsDirtyProperty); }
            set { SetValue(IsDirtyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDirty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDirtyProperty =
            DependencyProperty.Register("IsDirty", typeof(bool), typeof(NodeWidget), new PropertyMetadata(false, OnPropertyChanged_IsDirty));

        private static void OnPropertyChanged_IsDirty(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            (d as NodeWidget).InvalidateVisual();
        }

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(NodeWidget));

        public event RoutedEventHandler Click {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public bool IsSelected {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(NodeWidget), new PropertyMetadata(false, OnPropertyChanged));


        protected static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            (d as NodeWidget).InvalidateVisual();
        }

        static NodeWidget() {
        }

        public NodeWidget() {
            var mouseLeftDownAsObs = Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(
                (e) => { MouseLeftButtonDown += e; },
                (e) => { MouseLeftButtonDown -= e; });
            var mouseLeftUpAsObs =
                Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(
                    (e) => MouseLeftButtonUp += e,
                    (e) => MouseLeftButtonDown -= e);

            //var mouseMoveAsObs =
            //    Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(this, nameof(MouseMove));

            var mouseLeftClick = mouseLeftDownAsObs
                .TakeUntil(mouseLeftUpAsObs)
                .Repeat()
                .Subscribe(OnClick);

            //var mouseDrag = mouseLeftDownAsObs
            //    .Take(1)
            //    .Timeout()
            //    .TakeUntil()
        }

        protected void RaiseClickEvent() {
            var args = new RoutedEventArgs() { RoutedEvent = ClickEvent };

            RaiseEvent(args);
        }

        protected override void OnMouseEnter(MouseEventArgs e) {
            InvalidateVisual();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e) {
            InvalidateVisual();
            base.OnMouseLeave(e);
        }

        private void OnClick(EventPattern<MouseButtonEventArgs> e) {
            IsSelected = !IsSelected;

            RaiseClickEvent();

            e.EventArgs.Handled = true;

            InvalidateVisual();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters) {
            Point pt = hitTestParameters.HitPoint;

            if ((pt - new Point()).LengthSquared <= 25.0f)
                return new PointHitTestResult(this, pt);
            else
                return null;
        }

        protected override void OnRender(DrawingContext drawingContext) {
            var info = new SKBitmap(20, 20);
            var bound = new SKRect(0, 0, info.Width, info.Height);
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

            if (IsMouseOver)
                stroke.Color = SKColors.AliceBlue;
            if (IsSelected)
                fill.Color = SKColors.AliceBlue;

            canvas.DrawRect(bound, fill);
            canvas.DrawCircle(new SKPoint(10, 10), 5, stroke);
            canvas.DrawCircle(new SKPoint(10, 10), 5, fill);


            stroke.Dispose();
            fill.Dispose();
            canvas.Dispose();

            var bitmap = info.ToWriteableBitmap();
            info.Dispose();
            //bitmap.Unlock();
            drawingContext.PushTransform(new TranslateTransform(-10, -10));
            drawingContext.DrawImage(bitmap, new Rect(0, 0, 20, 20));
            drawingContext.Pop();

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
