using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace taskmaker_wpf.Views {
    public class NodeWidget : SKFrameworkElement {



        public Point Location {
            get { return (Point)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Location.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register("Location", typeof(Point), typeof(NodeWidget), new PropertyMetadata(new Point()));




        public Guid Id {
            get { return (Guid)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Id.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(Guid), typeof(NodeWidget), new PropertyMetadata(Guid.Empty));


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
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(NodeWidget), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

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

            if ((pt - Location).LengthSquared <= 25.0f)
                return new PointHitTestResult(this, pt);
            else
                return null;
        }

        protected override void Draw(SKCanvas canvas) {
            canvas.Save();

            var t = (Parent as ComplexWidget).ViewPort.GetTranslate();

            canvas.SetMatrix(t);

            using (var fill = new SKPaint())
            using (var stroke = new SKPaint()) {
                stroke.IsAntialias = true;
                stroke.StrokeWidth = 2;
                stroke.IsStroke = true;
                stroke.Color = SKColors.Black;

                fill.IsAntialias = true;
                fill.Color = SKColors.YellowGreen;

                if (IsMouseOver)
                    stroke.Color = SKColors.AliceBlue;
                if (IsSelected)
                    fill.Color = SKColors.AliceBlue;

                canvas.DrawCircle(Location.ToSKPoint(), 10, fill);
                canvas.DrawCircle(Location.ToSKPoint(), 10, stroke);
            }
        }
    }
}
