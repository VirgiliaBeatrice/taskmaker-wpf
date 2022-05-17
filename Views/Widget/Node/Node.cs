using SkiaSharp;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace taskmaker_wpf.Views.Widgets {
    public class NodeWidgetProps : IProps {
        public SKRect Bound { get; set; } = new SKRect { Right = 10, Top = 10 };
        public SKPoint Location { get; set; }
        public float Radius { get; set; }
        public bool IsSelected { get; set; }
    }

    public class NodeWidgetRenderObject : RenderObject_Wpf<NodeWidgetProps> {
        public NodeWidgetRenderObject(NodeWidgetProps props) : base(props) { }

        protected override void OnRender(SKCanvas canvas) {
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

            if (_props.IsSelected)
                fill.Color = SKColors.AliceBlue;

            canvas.DrawCircle(_props.Location, _props.Radius, stroke);
            canvas.DrawCircle(_props.Location, _props.Radius, fill);

            stroke.Dispose();
            fill.Dispose();
        }
    }

    public class NodeWidget_v1 : RenderWidget {


        public SKPoint Location {
            get { return (SKPoint)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Location.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register(
                "Location",
                typeof(SKPoint),
                typeof(NodeWidget_v1),
                new PropertyMetadata(SKPoint.Empty, OnPropertyChanged));

        public NodeWidget_v1(string name) : base(name) { }

        protected override IProps GetProps() {
            return new NodeWidgetProps {
                Location = Location,
                Radius = 5,
                IsSelected = false,
            };
        }

        //public async override void RenderAsync() {
        //    var props = GetProps();

        //    await Task.Run(() => {
        //        // Renderer worker
        //        var obj = new NodeWidgetRenderObject(props);

        //        // Invoke to UI thread
        //        Dispatcher.BeginInvoke((Action)(() => {
        //            RenderObject = obj;
        //        }));
        //    });
        //}

        //private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        //    (d as NodeWidget_v1).RenderAsync();
        //}
    }
}
