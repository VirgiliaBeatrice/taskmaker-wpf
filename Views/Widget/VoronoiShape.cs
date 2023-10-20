using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using taskmaker_wpf.ViewModels;
using Path = System.Windows.Shapes.Path;
using Point = System.Windows.Point;

namespace taskmaker_wpf.Views.Widget {
    public class VoronoiShape : UserControl {
        // Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(IEnumerable<Point>), typeof(VoronoiShape), new FrameworkPropertyMetadata(null, OnPointsPropertyChanged));

        private Matrix _transform = Matrix.Identity;
        public VoronoiShape(int uiId, BaseRegionState state) {
            UiId = uiId;

            MouseEnter += (s, e) => {
                var v = s as VoronoiShape;
                var path = v.Content as Path;

                path.Opacity = 0.1;
            };

            MouseLeave += (s, e) => {
                var v = s as VoronoiShape;
                var path = v.Content as Path;

                path.Opacity = 1;
            };
        }

        public IEnumerable<Point> Points {
            get { return (IEnumerable<Point>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        public Matrix Transform {
            get => _transform;
            set {
                var prevT = _transform;
                _transform = value;

                if (prevT != _transform) {
                    Invalidate();
                }
            }
        }

        public int UiId { get; set; }
        public void Invalidate() {
            var points = Points.Select(e => Transform.Transform(e)).ToArray();

            if (points.Length == 3) {
                var radius = (points[1] - points[0]).Length;
                var o = points[1];
                var p0 = points[0];
                var p1 = points[2];

                var p0o = (p0 - o);
                var p1o = (p1 - o);
                var dotProd = (p0o.X * p1o.X) + (p0o.Y * p1o.Y);
                var alpha = Math.Abs(Math.Acos(dotProd / (p0o.Length * p1o.Length)));

                var midLen = (float)Math.Tan(alpha / 2.0f) * Math.Abs(p0o.Length);

                var op0 = o - p0;

                op0.Normalize();
                //var op0 = Point.Normalize(o - p0);
                var transform = Matrix.Identity;
                transform.Rotate(Math.PI * 90.0 / 180.0);
                var midP0 = transform.Transform(op0);
                //var midP0 = SKMatrix.CreateRotation((float)(Math.PI * 90.0 / 180.0)).MapVector(op0);
                midP0 *= midLen;

                var mid = p0 + midP0;

                var pathGeo = new PathGeometry();
                var pathFig = new PathFigure {
                    StartPoint = o,
                };

                pathGeo.Figures.Add(pathFig);

                pathFig.Segments.Add(new LineSegment { Point = p1 });
                pathFig.Segments.Add(new ArcSegment { Point = p0, Size = new Size(radius, radius), SweepDirection = SweepDirection.Counterclockwise });
                pathFig.Segments.Add(new LineSegment { Point = o });

                var fill = ColorManager.GetTintedColor(ColorManager.Palette[UiId], 2);
                var radial = new RadialGradientBrush();
                var radialRadius = (p0 - o).Length;

                radial.MappingMode = BrushMappingMode.Absolute;
                radial.GradientOrigin = o;
                radial.Center = o;
                radial.RadiusX = radialRadius;
                radial.RadiusY = radialRadius;
                radial.GradientStops.Add(new GradientStop(fill, 0.0));
                radial.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
                radial.Freeze();

                var path = new Path {
                    Fill = radial,
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    Stretch = Stretch.None,
                    StrokeThickness = 1.0,
                    Data = pathGeo
                };

                Content = path;
            }
            else {
                var pathGeo = new PathGeometry();
                var pathFig = new PathFigure {
                    StartPoint = points[0],
                };

                pathGeo.Figures.Add(pathFig);

                pathFig.Segments.Add(new LineSegment { Point = points[1] });
                pathFig.Segments.Add(new LineSegment { Point = points[2] });
                pathFig.Segments.Add(new LineSegment { Point = points[3] });
                pathFig.Segments.Add(new LineSegment { Point = points[0] });

                var fill = ColorManager.GetTintedColor(ColorManager.Palette[UiId], 2);
                var linear = new LinearGradientBrush();

                linear.MappingMode = BrushMappingMode.Absolute;
                linear.StartPoint = points[1];
                linear.EndPoint = points[2];
                linear.GradientStops.Add(new GradientStop(fill, 0.0));
                linear.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
                linear.Freeze();

                var path = new Path {
                    Fill = linear,
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    Stretch = Stretch.None,
                    StrokeThickness = 1.0,
                    Data = pathGeo
                };

                Content = path;
            }
        }

        private static void OnPointsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var shape = (VoronoiShape)d;

            shape.Invalidate();
        }
    }
}