using Numpy;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using taskmaker_wpf.Data;
using taskmaker_wpf.Qhull;
using taskmaker_wpf.ViewModels;
using taskmaker_wpf.Model.Data;
using System.Dynamic;
using Numpy.Models;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Windows.Shapes;
using System.Windows.Media;
using taskmaker_wpf.Views.Widget;
using System.Drawing.Drawing2D;

namespace taskmaker_wpf.Domain {
    public interface IEntity {
        int Id { get; set; }
        string Name { get; set; }
    }

    public struct Bary {
        public Point[] Basis { get; set; }


        // 2d simplex
        static public double[] GetLambdas(Point[] basis, Point b) {
            var affineMat = basis
                .Select(e => new double[] { e.X, e.Y, 1.0 })
                .SelectMany(e => e)
                .ToArray();
            var affineMatNd = np
                .array(affineMat)
                .reshape(3, 3)
                .T;
            var bVecNd = np
                .array(new double[] { b.X, b.Y, 1.0 });
            var x = np.linalg.solve(affineMatNd, bVecNd);

            return x.GetData<double>();
        } 
    }

    public class BaseEntity : IEntity {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString() {
            return GetType().Name + "_" + Name;
        }
    }

    public class NLinearMapEntity : BaseEntity {
        public NDarray Tensor { get; set; }
        public int[] Shape { get; set; }

        public bool IsDirty { get; set; } = true;

        [XmlIgnore]
        public bool IsFullySet {
            get {
                if (IsDirty)
                    return false;
                else {
                    return !np.isnan(Tensor).any();
                }
            }
        }


        // For NLinearMap, InSockets
        public InPlug[] InSockets { get; set; } = Array.Empty<InPlug>();

        // For NLinearMap, OutSockets
        public OutPlug[] OutSockets { get; set; } = Array.Empty<OutPlug>();
        

        protected double[] tensor => Tensor.isnan().any() ? null : Tensor.GetData<double>();

        public (int, int) GetCurrentStatus() {
            return (Tensor.size - np.count_nonzero(np.isnan(Tensor)), Tensor.size);
        }

        public void Initialize(int[] basisDims) {
            if (OutSockets.Length == 0 || InSockets.Length == 0) return;

            var targetDim = OutSockets.Select(e => e.Dimension).Sum();
            //var basisDims = InputPorts.Select(e => e.BasisCount).ToArray();

            Shape = new int[] { targetDim }.Concat(basisDims).ToArray();
            Tensor = np.empty(Shape);

            Tensor.fill(np.nan);

            IsDirty = false;
        }


        public void Initialize() {
            if (OutSockets.Length == 0 || InSockets.Length == 0) return;

            var targetDim = OutSockets.Select(e => e.Dimension).Sum();
            var basisDims = InSockets.Select(e => e.BasisCount).ToArray();

            Shape = new int[] { targetDim }.Concat(basisDims).ToArray();
            Tensor = np.empty(Shape);

            Tensor.fill(np.nan);

            IsDirty = false;
        }

        public void SetValue(int[] indices, double[] value) {
            // only 1 bary
            //Tensor[$":,{indices[0]}"] = np.atleast_2d(value);

            // more than 1
            var indexStr = $":,{string.Join(",", indices)}";
            Tensor[indexStr] = value;
        }

        public bool HasSet(int[] indices) {
            var indexStr = $":,{string.Join(",", indices)}";

            try {
                return !np.isnan(Tensor[indexStr]).any();
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return false;
            }
            //return Tensor[indexStr].GetData<double>().All(e => !double.IsNaN(e));
        }

        public NDarray MapTo(double[][] lambdas) {
            if (!IsFullySet)
                return null;

            try {
                NDarray kronProd = null;
                //lambdas = np.atleast_2d(lambdas);

                for (int i = 0; i < lambdas.Length; ++i) {
                    if (i == 0) {
                        kronProd = np.array(lambdas[i]).flatten();
                    }
                    else {
                        kronProd = np.kron(kronProd, np.array(lambdas[i]));
                    }
                }

                var w = np.dot(Tensor.reshape(Shape[0], -1), kronProd);

                return w;
            } catch(Exception e) {
                Console.WriteLine(e);
                return np.zeros(1);
            }
        }

    }

    public class NodeEntity : BaseEntity {
        public Point Value { get; set; }

    }

    [XmlInclude(typeof(SimplexRegionEntity))]
    [XmlInclude(typeof(VoronoiRegionEntity))]
    public abstract class BaseRegionEntity : BaseEntity {
        public abstract double[] GetLambdas(Point pt, NodeEntity[] collection);

        public abstract BaseRegionEntity HitTest(Point pt);

        public static bool HitTest(Point[] polygon, Point pt) {
            var result = false;

            int j = polygon.Length - 1;

            for (int i = 0; i < polygon.Count(); i++) {
                if (polygon[i].Y < pt.Y && polygon[j].Y >= pt.Y || polygon[j].Y < pt.Y && polygon[i].Y >= pt.Y) {
                    if (polygon[i].X + (pt.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < pt.X) {
                        result = !result;
                    }
                }

                j = i;
            }

            return result;
        }
    }

    public class SimplexRegionEntity : BaseRegionEntity {
        public NodeEntity[] Nodes { get; set; }
        public Point[] Vertices => Nodes.Select(x => x.Value).ToArray();

        public SimplexRegionEntity() { }

        public SimplexRegionEntity(NodeEntity[] nodes) {
            Nodes = nodes;
        }

        public override BaseRegionEntity HitTest(Point pt) {
            return HitTest(Vertices, pt) ? this : null;
        }

        public override double[] GetLambdas(Point pt, NodeEntity[] collection) {
            var lambdas = Bary.GetLambdas(Vertices, pt);
            var indices = Nodes
                .Select(e => {
                    var target = collection.Where(e1 => e1.Id == e.Id).FirstOrDefault();

                    return collection.ToList().IndexOf(target);
                })
                .ToArray();
            var result = Enumerable.Repeat(0.0, collection.Length).ToArray();

            for(var idx = 0; idx < lambdas.Length; idx ++) {
                result[indices[idx]] = lambdas[idx];
            }

            return result;
        }

        public bool IsVertex(NodeEntity node) {
            return Nodes.Any(e => node == e);
        }
    }

    [XmlInclude(typeof(RectVoronoiRegionEntity))]
    [XmlInclude(typeof(SectoralVoronoiRegionEntity))]
    public class VoronoiRegionEntity : BaseRegionEntity {
        public double Factor { get; set; } = 200.0;
        public Point[] Vertices { get; set; }
        public SimplexRegionEntity[] Governors { get; set; }

        public VoronoiRegionEntity() { }

        public override BaseRegionEntity HitTest(Point pt) {
            return HitTest(Vertices, pt)? this : null;
        }

        public override double[] GetLambdas(Point pt, NodeEntity[] collection) { throw new NotImplementedException(); }

        public static VoronoiRegionEntity[] Build(NodeEntity[] extremes, SimplexRegionEntity[] simplices) {
            NodeEntity prev, it, next;

            var voronois = new List<VoronoiRegionEntity>();

            for (var i = 0; i < extremes.Length; i++) {
                it = extremes[i];

                if (i <= 0) {
                    prev = extremes.Last();
                    next = extremes[i + 1];
                }
                else if (i < extremes.Length - 1) {
                    prev = extremes[i - 1];
                    next = extremes[i + 1];
                }
                else {
                    prev = extremes[i - 1];
                    next = extremes.First();
                }

                // Sectoral
                var prevGov = Array.Find(simplices, e => e.IsVertex(prev) & e.IsVertex(it));
                var nextGov = Array.Find(simplices, e => e.IsVertex(it) & e.IsVertex(next));

                var sectorVoronoi = new SectoralVoronoiRegionEntity(
                    new[] { prev, it, next },
                    new[] { prevGov, nextGov });

                voronois.Add(sectorVoronoi);

                // Rect
                var gov = Array.Find(simplices, e => e.IsVertex(it) & e.IsVertex(next));
                var rectVoronoi = new RectVoronoiRegionEntity(new[] { it, next }, gov);

                voronois.Add(rectVoronoi);
            }

            return voronois.ToArray();
        }
    }

    public class SectoralVoronoiRegionEntity : VoronoiRegionEntity {
        public override double[] GetLambdas(Point pt, NodeEntity[] collection) {
            if (Governors[0].Id == Governors[1].Id) {
                return Governors[0].GetLambdas(pt, collection);
            }
            else {
                var factors = GetFactors(pt);
                var lambda0 = Governors[0].GetLambdas(pt, collection).Select(e => e * factors[0]).ToArray();
                var lambda1 = Governors[1].GetLambdas(pt, collection).Select(e => e * factors[1]).ToArray();

                return lambda0.Zip(lambda1, (f, s) => (f + s)).ToArray();
            }
        }
        private PathGeometry Geometry { get; set; }

        public SectoralVoronoiRegionEntity() { }

        public override BaseRegionEntity HitTest(Point pt) {
            return base.HitTest(pt);

            //if (Geometry == null)
            //    InitializeGeometry();

            //return Geometry.FillContains(pt)? this : null;
        }

        public SectoralVoronoiRegionEntity(NodeEntity[] nodes, SimplexRegionEntity[] simplices) {
            Governors = simplices;
            Invalidate(nodes);

            InitializeGeometry();
        }

        private void Invalidate(NodeEntity[] nodes) {
            var prev = np.array(new double[] { nodes[0].Value.X, nodes[0].Value.Y });
            var it = np.array(new double[] { nodes[1].Value.X, nodes[1].Value.Y });
            var next = np.array(new double[] { nodes[2].Value.X, nodes[2].Value.Y });

            var dir0 = prev - it;
            var stdDir0 = dir0 / np.linalg.norm(dir0);
            var dir1 = next - it;
            var stdDir1 = dir1 / np.linalg.norm(dir1);

            var theta0 = CreateRotationMatrix(-90);
            var theta1 = CreateRotationMatrix(90);

            NDarray CreateRotationMatrix(double degree) {
                var theta = np.deg2rad(np.array(degree));
                var c = np.cos(theta);
                var s = np.sin(theta);

                theta.Dispose();

                return np.array(new NDarray[] { c, -s, s, c }).squeeze().reshape(2, 2);
            }

            var ray0 = it + np.dot(theta0, stdDir0) * Factor;
            var ray1 = it + np.dot(theta1, stdDir1) * Factor;

            Vertices = (new NDarray[] { ray0, it, ray1 })
                .Select(e => e.astype(np.int32).GetData<int>())
                .Select(e => new Point(e[0], e[1]))
                .ToArray();
        }

        private void InitializeGeometry() {
            var path = new GraphicsPath();

            var radius = (Vertices[1] - Vertices[0]).Length;
            var o = Vertices[1];
            var p0 = Vertices[0];
            var p1 = Vertices[2];

            var p0o = (p0 - o);
            var p1o = (p1 - o);
            var dotProd = (p0o.X * p1o.X) + (p0o.Y * p1o.Y);
            var alpha = Math.Abs(Math.Acos(dotProd / (p0o.Length * p1o.Length)));

            var midLen = (float)Math.Tan(alpha / 2.0f) * Math.Abs(p0o.Length);

            var op0 = o - p0;

            op0.Normalize();
            //var op0 = Point.Normalize(o - p0);
            var transform = System.Windows.Media.Matrix.Identity;
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

            Geometry = pathGeo;
        }

        private double[] GetFactors(Point pt) {
            var a = np.array(Vertices[0].X, Vertices[0].Y);
            var o = np.array(Vertices[1].X, Vertices[1].Y);
            var b = np.array(Vertices[2].X, Vertices[2].Y);
            var p = np.array(pt.X, pt.Y);

            var ao = a - o;
            var bo = b - o;
            var po = p - o;

            var theta0 = np.abs(np.arccos(
                np.dot(ao, po) / (np.linalg.norm(ao) * np.linalg.norm(po))));
            var theta1 = np.abs(np.arccos(
                np.dot(bo, po) / (np.linalg.norm(bo) * np.linalg.norm(po))));
            var theta = theta0 + theta1;

            return new double[] {
                (theta1 / theta).GetData<double>()[0],
                (theta0 / theta).GetData<double>()[0]
            }; 
        }
    }


    public class RectVoronoiRegionEntity : VoronoiRegionEntity {
        public RectVoronoiRegionEntity() { }

        public RectVoronoiRegionEntity(NodeEntity[] nodes, SimplexRegionEntity governor) {
            Governors = new[] { governor };
            Invalidate(nodes);
        }

        private void Invalidate(NodeEntity[] nodes) {
            var a = np.array(nodes[0].Value.X, nodes[0].Value.Y);
            var b = np.array(nodes[1].Value.X, nodes[1].Value.Y);

            var dir = b - a;
            var dirHat = dir / np.linalg.norm(dir);

            // ccw
            var perp = np.dot(CreateRotationMatrix(90), dirHat);
            var aP = a + perp * Factor;
            var bP = b + perp * Factor;

            // [ cost  -sint ] [ e00 e01 ]
            // [ sint   cost ] [ e10 e11 ]

            NDarray CreateRotationMatrix(double degree) {
                var theta = np.deg2rad(np.array(degree));
                var e00 = np.cos(theta).GetData<double>()[0];
                var e01 = -np.sin(theta).GetData<double>()[0];
                var e10 = np.sin(theta).GetData<double>()[0];
                var e11 = np.cos(theta).GetData<double>()[0];

                return np.array(new double[] { e00, e01, e10, e11 }).reshape(2, 2);
            }

            Vertices = new Point[] {
                new Point(a.astype(np.int32).GetData<int>()[0], a.astype(np.int32).GetData<int>()[1]),
                new Point(b.astype(np.int32).GetData<int>()[0], b.astype(np.int32).GetData<int>()[1]),
                new Point(bP.astype(np.int32).GetData<int>()[0], bP.astype(np.int32).GetData<int>()[1]),
                new Point(aP.astype(np.int32).GetData<int>()[0], aP.astype(np.int32).GetData<int>()[1]),
            };

            //return new NDarray[] { a, b, bP, aP };
        }

        public override double[] GetLambdas(Point pt, NodeEntity[] collection) {
            return Governors[0].GetLambdas(pt, collection);
        }

    }


    public class ControlUiEntity : BaseEntity, IInputPort, IOutputPort {
        private double[] inputValue = new double[2];
        private NodeEntity[] nodes;

        public NodeEntity[] Nodes { get => nodes;
            set {
                nodes = value;

                InvalidateInPlug();
            }
        }
        public BaseRegionEntity[] Regions { get; set; }
        public NLinearMapEntity Map { get; set; }

        public double[] Value { get; set; } = new double[2];

        public double[] OutputValue { get; set; } = new double[2];
        public double[] InputValue {
            get => inputValue;
            set {
                inputValue = value;

                OutputValue = inputValue;
            }
        }

        public void InvalidateInPlug() {
            if (Map != null) {
                var plug = InPlug.Create(this);
                var idx = Map.InSockets.ToList().IndexOf(plug);

                Map.InSockets[idx] = plug;
            }
        }

        public void InvalidateValue() {
            Value = new double[] {
                Nodes.Select(e => e.Value.X).Average(),
                Nodes.Select(e => e.Value.Y).Average(),
            };
        }

        public void Build() {
            var nodes = Nodes.OrderBy(e => e.Id).ToArray();
            var input = np.array(
                nodes.Select(e => np.array(new[] { e.Value.X, e.Value.Y }))
                    .ToArray());


            var regions = new List<BaseRegionEntity>();

            if (nodes.Length < 2) return;
            else if (nodes.Length == 2) {
                var simplex = new SimplexRegionEntity(nodes);

                regions.Add(simplex);


                int idx = 0;
                foreach (var item in regions) {
                    item.Id = ++idx;
                    item.Name = $"{item.GetType()}-{item.Id}";
                }

                Regions = regions.ToArray();
            }
            else if (nodes.Length == 3) {
                var simplex = new SimplexRegionEntity(nodes);

                regions.Add(simplex);

                var extremes = QhullCSharp.RunConvex(input)
                    .Select(idx => nodes.ElementAt(idx))
                    .Reverse()
                    .ToArray();
                var voronois = VoronoiRegionEntity.Build(extremes, new SimplexRegionEntity[] { simplex });

                foreach (var item in voronois) {
                    regions.Add(item);
                }


                int idx0 = 0;
                foreach (var item in regions) {
                    item.Id = ++idx0;
                    item.Name = $"{item.GetType()}-{item.Id}";
                }

                Regions = regions.ToArray();
            }
            else {
                var simplices = QhullCSharp.RunDelaunay(input)
                    .Select(indices => new SimplexRegionEntity(indices.Select(idx => nodes.ElementAt(idx)).ToArray()))
                    .ToArray();

                foreach (var item in simplices) {
                    regions.Add(item);
                }

                var extremes = QhullCSharp.RunConvex(input)
                    .Select(idx => nodes.ElementAt(idx))
                    .Reverse()
                    .ToArray();
                var voronois = VoronoiRegionEntity.Build(extremes, simplices);

                foreach (var item in voronois) {
                    regions.Add(item);
                }

                int idx0 = 0;
                foreach (var item in regions) {
                    item.Id = ++idx0;
                    item.Name = $"{item.GetType()}-{item.Id}";
                }

                Regions = regions.ToArray();
            }
        }
    }

    public class MotorEntity : BaseEntity, IOutputPort {
        public double[] Value { get; set; } = new double[1];
        public int Min { get; set; } = -10000;
        public int Max { get; set; } = 10000;
        public int NuibotBoardId { get; set; } = -1;
        public int NuibotMotorId { get; set; } = -1;

        public int ThresholdMin { get; set; } = -10000;
        public int ThresholdMax { get; set; } = 10000;
        public bool IsClamped { get; set; } = false;

        public SolidColorBrush Color { get; set; } = Brushes.Black;
    }
}
