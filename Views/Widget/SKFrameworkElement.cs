using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SkiaSharp;

namespace taskmaker_wpf.Views {
    public class SKFrameworkElement : FrameworkElement {
        public SKFrameworkElement() : base() {
            SizeChanged += SKFrameWorkElement_SizeChanged;
        }

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            //(Parent as ComplexWidget).ViewPort.Register(Draw, Canvas.GetZIndex(this));
        }

        private void SKFrameWorkElement_SizeChanged(object sender, SizeChangedEventArgs e) {
            InvalidateVisual();
        }

        public virtual void Draw(SKCanvas canvas) { }
    }
}
