using System.Windows.Controls;
using System.Windows.Media;
using taskmaker_wpf.ViewModels;
using Path = System.Windows.Shapes.Path;
using Point = System.Windows.Point;

namespace taskmaker_wpf.Views.Widget {
    public class SimplexShape : UserControl {
        private int id;
        private Point[] points;

        public int Id { get => id; init => id = value; }
        public Point[] Points { get => points; init => points = value; }
        public BaseRegionState State => DataContext as BaseRegionState;

        public UiController Ui { get; set; }

        public SimplexShape(int id, Point[] points) {
            Id = id;
            Points = points;

            MouseEnter += (s, e) => {
                var v = s as SimplexShape;
                var path = v.Content as Path;

                path.Opacity = 0.1;
            };

            MouseLeave += (s, e) => {
                var v = s as SimplexShape;
                var path = v.Content as Path;

                path.Opacity = 0.3;
            };

            Loaded += (_, _) => {
                Ui = VisualTreeHelperExtensions.FindParentOfType<UiController>(this);
            };

            Initialize();

            //RenderTransform = new ScaleTransform(1, -1);
        }

        public void Initialize() {
            var points = Points;

            // 3-simplex
            if (points.Length == 3) {
                var pathGeo = new PathGeometry();
                var pathFig = new PathFigure {
                    StartPoint = points[0],
                };

                pathGeo.Figures.Add(pathFig);
                pathFig.Segments.Add(new LineSegment { Point = points[1] });
                pathFig.Segments.Add(new LineSegment { Point = points[2] });
                pathFig.Segments.Add(new LineSegment { Point = points[0] });

                var fill = ColorManager.GetTintedColor(ColorManager.Palette[0], 2);

                var path = new Path {
                    Fill = new SolidColorBrush(fill),
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    Stretch = Stretch.None,
                    StrokeThickness = 1.0,
                    Data = pathGeo
                };

                path.Opacity = 0.3;

                Content = path;
            }
            // 2-simplex
            else if (points.Length == 2) {
                var pathGeo = new PathGeometry();
                var pathFig = new PathFigure {
                    StartPoint = points[0],
                };

                pathGeo.Figures.Add(pathFig);
                pathFig.Segments.Add(new LineSegment { Point = points[1] });
                pathFig.Segments.Add(new LineSegment { Point = points[0] });

                var path = new Path {
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    Stretch = Stretch.None,
                    StrokeThickness = 2.0,
                    Data = pathGeo
                };

                path.Opacity = 0.3;

                Content = path;
            }
        }
    }
}