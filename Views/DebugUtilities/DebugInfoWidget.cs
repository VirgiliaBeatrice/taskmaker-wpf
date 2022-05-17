using SkiaSharp;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


namespace taskmaker_wpf.Views.Widgets {
    public class DebugInfoWidgetProps : IProps {
        public SKRect Bound { get; set; } = new SKRect { Right = 100, Bottom = 40 };
        public string Message { get; set; }
    }

    public class DebugInfoRenderObject : RenderObject_Wpf<DebugInfoWidgetProps> {
        public DebugInfoRenderObject(DebugInfoWidgetProps props)
            : base(props) {
        }

        protected override void OnRender(SKCanvas canvas) {
            var paint = new SKPaint {
                IsAntialias = true,
                StrokeWidth = 2,
                Color = SKColors.Black,
                TextSize = 14,
            };

            canvas.DrawText(
                _props.Message,
                new SKPoint { X = 0, Y = 20 }, 
                paint
            );

            paint.Dispose();
        }
    }

    public class DebugInfoWidget : RenderWidget {
        public string Message {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                "Message",
                typeof(string),
                typeof(DebugInfoWidget),
                new PropertyMetadata("Debug", OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            (d as DebugInfoWidget).RenderAsync();
        }

        private DebugInfoWidgetProps GetProps() {
            return new DebugInfoWidgetProps { Message = Message };
        }

        public async override void RenderAsync() {
            var props = GetProps();

            await Task.Run(() => {
                // Renderer worker
                var obj = new DebugInfoRenderObject(props);

                // Invoke to UI thread
                Dispatcher.BeginInvoke((Action)(() => {
                    RenderObject = obj;
                }));
            });
        }

        public DebugInfoWidget(string name) { }

    }
}
