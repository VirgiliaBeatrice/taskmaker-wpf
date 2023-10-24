using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace taskmaker_wpf.Views.Widget {
    public class Arrow : Shape {

        // Using a DependencyProperty as the backing store for End.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End", typeof(Point), typeof(Arrow), new PropertyMetadata(new Point(), OnPropertyChanged));

        // Using a DependencyProperty as the backing store for Start.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start", typeof(Point), typeof(Arrow), new PropertyMetadata(new Point(), (PropertyChangedCallback)OnPropertyChanged));

        // Using a DependencyProperty as the backing store for Transform.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransformProperty =
            DependencyProperty.Register("Transform", typeof(Matrix), typeof(Arrow), new PropertyMetadata(Matrix.Identity, OnPropertyChanged));

        public Arrow() {
            Stroke = Brushes.Green;
            //SnapsToDevicePixels = false;
            //UseLayoutRounding = false;
        }

        public Point End {
            get { return (Point)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }

        public Point Start {
            get { return (Point)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }

        public Matrix Transform {
            get { return (Matrix)GetValue(TransformProperty); }
            set { SetValue(TransformProperty, value); }
        }
        protected override Geometry DefiningGeometry => Generate();

        public static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            (d as Arrow).InvalidateVisual();
        }
        private Geometry Generate() {
            var start = Transform.Transform(Start);
            var end = Transform.Transform(End);

            var arrowLine = new PathFigure();

            if (start == end)
                return new LineGeometry();

            arrowLine.StartPoint = start;

            arrowLine.Segments.Add(new LineSegment(end, true));

            var vector = (start - end);

            vector.Normalize();

            var rotate = Matrix.Identity;

            rotate.Rotate(15);

            var e0 = rotate.Transform(vector * 10.0) + end;

            rotate.SetIdentity();
            rotate.Rotate(-15);

            var e1 = rotate.Transform(vector * 10.0) + end;

            var arrowTip = new PathFigure();

            arrowTip.StartPoint = end;

            arrowTip.Segments.Add(new LineSegment(e0, true));
            arrowTip.Segments.Add(new LineSegment(e1, true));
            arrowTip.Segments.Add(new LineSegment(end, true));

            var geometry = new PathGeometry();

            geometry.Figures.Add(arrowLine);
            geometry.Figures.Add(arrowTip);

            return geometry;
        }
    }
}