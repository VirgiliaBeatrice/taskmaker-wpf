using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace taskmaker_wpf.Views.Widget {
    public class ArrowLine : Shape {

        // Using a DependencyProperty as the backing store for End.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End", typeof(Point), typeof(ArrowLine), new PropertyMetadata(new Point(0, 0), OnPropertyChanged));

        // Using a DependencyProperty as the backing store for Start.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start", typeof(Point), typeof(ArrowLine), new PropertyMetadata(new Point(0, 0), OnPropertyChanged));

        public ArrowLine(Point start, Point end) {
            Start = start;
            End = end;
            //Stroke = Brushes.Black;
            StrokeThickness = 2;
            StrokeLineJoin = PenLineJoin.Bevel;
        }

        public Point End {
            get { return (Point)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }

        public Point Start {
            get { return (Point)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }
        protected override Geometry DefiningGeometry {
            get {
                return GetGeometry();
            }
        }

        protected Geometry Geometry { get; set; }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var arrow = (ArrowLine)d;
            arrow.Invalidate();
        }

        private Geometry GetGeometry() {
            var start = Start;
            var end = End;

            // Calculate the line segment vector
            Vector lineSegment = end - start;

            // Normalize the line segment vector
            Vector lineDirection = lineSegment;
            lineDirection.Normalize();

            // Calculate the perpendicular vector to the line segment vector
            Vector perpendicularVector = new Vector(-lineSegment.Y, lineSegment.X);
            perpendicularVector.Normalize();

            // Define the length and width of the arrowhead
            double arrowheadLength = 20;
            double arrowheadWidth = 8;

            // Calculate the endpoints of the arrowhead
            Point arrowheadEndpoint1 = end - (lineDirection * arrowheadLength) + (perpendicularVector * arrowheadWidth);
            Point arrowheadEndpoint2 = end - (lineDirection * arrowheadLength) - (perpendicularVector * arrowheadWidth);

            // Define the points of the arrow line
            PointCollection points = new PointCollection {
                start,
                end,
                arrowheadEndpoint1,
                arrowheadEndpoint2,
                end,
            };

            // Create PathFigure with points
            PathFigure figure = new PathFigure {
                StartPoint = start,
                Segments = new PathSegmentCollection {
                    new PolyLineSegment(points, true)
                },
            };

            // Create PathGeometry with PathFigure
            PathGeometry geometry = new PathGeometry {
                Figures = new PathFigureCollection {
                    figure
                }
            };

            return geometry;
        }

        private void Invalidate() {
            var geometry = GetGeometry();
            Geometry = geometry;

            InvalidateVisual();
        }
    }
}