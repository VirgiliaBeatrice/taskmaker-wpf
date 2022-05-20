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

    public class DebugInfoWidgetRenderObject : RenderObject<DebugInfoWidgetProps> {
        public DebugInfoWidgetRenderObject(DebugInfoWidgetProps props)
            : base(props) {
        }

        public override bool HitTest(SKPoint pt) {
            return false;
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

        public DebugInfoWidget(string name) : base(name) {
            //_TProps = Type.GetType(GetType().FullName + "Props");
            //_TRenderObj = Type.GetType(GetType().FullName + "RenderObject");
            //_TProps = typeof(DebugInfoWidgetProps);
            //_TRenderObj = typeof(DebugInfoRenderObject);
        }

        protected override IProps GetProps() {
            return new DebugInfoWidgetProps { Message = Message };
        }
    }
}
