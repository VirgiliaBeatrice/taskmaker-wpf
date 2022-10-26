using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace taskmaker_wpf.Views {
    public interface ISelectableWidget {
        bool IsSelected { get; set; }
    }

    public class NodeWidget : SKFrameworkElement, ISelectableWidget {

        public double Radius {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(NodeWidget), new PropertyMetadata(10.0));


        public Point Location {
            get { return (Point)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Location.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register("Location", typeof(Point), typeof(NodeWidget), new PropertyMetadata(new Point(), OnLocationChanged));

        private static void OnLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            //Canvas.SetLeft(d as NodeWidget, ((Point)e.NewValue).X);
            //Canvas.SetTop(d as NodeWidget, ((Point)e.NewValue).Y);
        }

        public int Id {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Id.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(int), typeof(NodeWidget), new PropertyMetadata(-1));


        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(NodeWidget));

        public event RoutedEventHandler Click {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }



        public bool IsSet {
            get { return (bool)GetValue(IsSetProperty); }
            set { SetValue(IsSetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasBindingValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSetProperty =
            DependencyProperty.Register("IsSet", typeof(bool), typeof(NodeWidget), new PropertyMetadata(false));


        public bool IsSelected {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(NodeWidget), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

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
            (Parent as ComplexWidget)?.InvalidateSKContext();

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e) {
            (Parent as ComplexWidget)?.InvalidateSKContext();

            base.OnMouseLeave(e);
        }

        private void OnClick(EventPattern<MouseButtonEventArgs> e) {
            IsSelected = !IsSelected;

            if (IsSelected) {
                var parent = ((ComplexWidget)Parent);

                //parent.SetSelection(this);
                parent.Select(this);
            }

            //RaiseClickEvent();

            e.EventArgs.Handled = true;

            InvalidateVisual();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters) {
            Point pt = hitTestParameters.HitPoint;
            //var wPt = (Parent as ComplexWidget).ViewPort.ViewportToWorld(pt.ToSKPoint());
            var vLoc = (Parent as ComplexWidget).ViewPort.WorldToViewport(Location.ToSKPoint());


            if ((pt.ToSKPoint() - vLoc).LengthSquared <= Math.Pow(Radius, 2.0))
                return new PointHitTestResult(this, pt);
            else
                return null;
        }

        public override void Draw(SKCanvas canvas) {
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
                    stroke.Color = SKColors.Black.WithAlpha((256/2));
                if (IsSet)
                    fill.Color = SKColors.Orange;
                if (IsSelected)
                    fill.Color = SKColors.AliceBlue;

                canvas.DrawCircle(Location.ToSKPoint(), (float)Radius, fill);
                //canvas.DrawCircle(new SKPoint(0, 0), (float)Radius, fill);
                canvas.DrawCircle(Location.ToSKPoint(), (float)Radius, stroke);
                //canvas.DrawCircle(new SKPoint(0, 0), (float)Radius, stroke);
            }
        }
    }
}
