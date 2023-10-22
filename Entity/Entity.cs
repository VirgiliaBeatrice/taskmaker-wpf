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
using System.Windows.Media;
using taskmaker_wpf.Views.Widget;
using System.Security.Policy;
using NLog;

namespace taskmaker_wpf.Entity {
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
        private int id = -1;

        public int Id {
            get => id;
            set {
                id = value;
                Name = $"{GetType().Name}-{id}";
            }
        }
        public string Name { get; set; }

        public BaseEntity() { }
        public override string ToString() {
            return Name;
        }
    }

    public class NLinearMapEntity : BaseEntity {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public int[] Keys { get; set; } = Array.Empty<int>();

        public NDarray Tensor { get; set; }
        public int[] Shape => Tensor?.shape.Dimensions;
        public int? Dimension => Tensor?.ndim;


        // Output: Motor/1 or UI/2
        // Input: UI/2
        public NLinearMapEntity(int[] shape) {
            Tensor = np.empty(shape);

            Tensor.fill(np.nan);
        }

        static NDarray ExpandAt(NDarray a, int axis, int idx, float fillValue) {
            var end = a.shape[axis];

            // Slice array into two parts: before and after idx
            var slice1 = new Slice(0, idx);
            var slice2 = new Slice(idx, end - 1);

            var slicesBefore = new Slice[a.ndim];
            var slicesAfter = new Slice[a.ndim];

            for (int i = 0; i < a.ndim; i++) {
                if (i == axis) {
                    slicesBefore[i] = slice1;
                    slicesAfter[i] = slice2;
                }
                else {
                    slicesBefore[i] = Slice.All();
                    slicesAfter[i] = Slice.All();
                }
            }

            var part1 = a[slicesBefore];
            var part2 = a[slicesAfter];

            // Create new array of the desired shape to insert
            var newShape = new int[a.ndim];
            for (int i = 0; i < a.ndim; i++) {
                newShape[i] = a.shape[i];
            }
            newShape[axis] = 1;

            var insertArray = np.full(new Shape(newShape), fillValue);

            // Concatenate the three arrays together along the specified axis
            return np.concatenate(new NDarray[] { part1, insertArray, part2 }, axis: axis);
        }

        public void ExpandAt(int axis, int idx) {
            Tensor = ExpandAt(Tensor, axis, idx, np.nan);
        }

        static NDarray RemoveAt(NDarray a, int axis, int idx) {
            // Slice array into two parts: before and after idx
            var slice1 = new Slice(0, idx);
            var slice2 = new Slice(idx + 1, a.shape[axis] - 1);

            var slicesBefore = new Slice[a.ndim];
            var slicesAfter = new Slice[a.ndim];

            for (int i = 0; i < a.ndim; i++) {
                if (i == axis) {
                    slicesBefore[i] = slice1;
                    slicesAfter[i] = slice2;
                }
                else {
                    slicesBefore[i] = Slice.All();
                    slicesAfter[i] = Slice.All();
                }
            }

            var part1 = a[slicesBefore];
            var part2 = a[slicesAfter];

            // Concatenate the two arrays together along the specified axis
            return np.concatenate(new NDarray[] { part1, part2 }, axis: axis);
        }

        public void RemoveAt(int axis, int idx) {
            Tensor = RemoveAt(Tensor, axis, idx);
        }

        public void SetValue(int[] indices, double[] value) {
            // make slice
            var slices = new Slice[Tensor.ndim];

            for (int i = 0; i < indices.Length; i++) {
                if (indices[i] == -1)
                    slices[i] = Slice.All();
                else
                    slices[i] = Slice.Index(indices[i]);
            }

            Tensor[slices] = np.array(value);
        }

        public double[] GetValue(int[] indices) {
            var slices = new Slice[Tensor.ndim];

            for (int i = 0; i < indices.Length; i++) {
                if (indices[i] == -1)
                    slices[i] = Slice.All();
                else
                    slices[i] = Slice.Index(indices[i]);
            }

            return Tensor[slices].GetData<double>();
        }


        public NDarray MapTo(double[][] lambdas) {
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
            }
            catch (Exception e) {
                _logger.Error(e);
                return np.zeros(1);
            }
        }

        public static IEnumerable<NDarray> GetValuesFromCombinations(NDarray array) {
            foreach (var combo in GetCombinations(array)) {
                yield return array[combo];
            }
        }

        // Reusing the previously defined GetCombinations method
        public static IEnumerable<int[]> GetCombinations(NDarray array) {
            int nDims = array.ndim;
            int[] shape = array.shape.Dimensions;
            return GenerateCombinations(shape, 1);
        }

        private static IEnumerable<int[]> GenerateCombinations(int[] shape, int currentDim) {
            if (currentDim >= shape.Length) {
                yield return new int[shape.Length];
                yield break;
            }

            for (int i = 0; i < shape[currentDim]; i++) {
                foreach (var next in GenerateCombinations(shape, currentDim + 1)) {
                    next[currentDim] = i;
                    yield return next;
                }
            }
        }

    }

    public class NodeEntity : BaseEntity {
        public Point Value { get; set; }

    }

    [XmlInclude(typeof(SimplexRegionEntity))]
    [XmlInclude(typeof(VoronoiRegionEntity))]
    public abstract class BaseRegionEntity : BaseEntity {
        public abstract Point[] Vertices { get; }
        public abstract double[] GetLambdas(Point pt, NodeEntity[] collection);

    }

    public class SimplexRegionEntity : BaseRegionEntity {
        public NodeEntity[] Nodes { get; set; }
        public override Point[] Vertices => Nodes.Select(x => x.Value).ToArray();

        public SimplexRegionEntity() { }

        public SimplexRegionEntity(NodeEntity[] nodes) {
            Nodes = nodes;
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

            for (var idx = 0; idx < lambdas.Length; idx++) {
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
        public override Point[] Vertices => _vertices;
        protected Point[] _vertices;
        public SimplexRegionEntity[] Governors { get; set; }

        public VoronoiRegionEntity() { }

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

        public SectoralVoronoiRegionEntity(NodeEntity[] nodes, SimplexRegionEntity[] simplices) {
            Governors = simplices;
            Invalidate(nodes);

            //InitializeGeometry();
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

            _vertices = (new NDarray[] { ray0, it, ray1 })
                .Select(e => e.astype(np.int32).GetData<int>())
                .Select(e => new Point(e[0], e[1]))
                .ToArray();
        }

        //// Should be in View's logic
        //private void InitializeGeometry() {
        //    var path = new GraphicsPath();

        //    var radius = (Vertices[1] - Vertices[0]).Length;
        //    var o = Vertices[1];
        //    var p0 = Vertices[0];
        //    var p1 = Vertices[2];

        //    var p0o = (p0 - o);
        //    var p1o = (p1 - o);
        //    var dotProd = (p0o.X * p1o.X) + (p0o.Y * p1o.Y);
        //    var alpha = Math.Abs(Math.Acos(dotProd / (p0o.Length * p1o.Length)));

        //    var midLen = (float)Math.Tan(alpha / 2.0f) * Math.Abs(p0o.Length);

        //    var op0 = o - p0;

        //    op0.Normalize();
        //    //var op0 = Point.Normalize(o - p0);
        //    var transform = System.Windows.Media.Matrix.Identity;
        //    transform.Rotate(Math.PI * 90.0 / 180.0);
        //    var midP0 = transform.Transform(op0);
        //    //var midP0 = SKMatrix.CreateRotation((float)(Math.PI * 90.0 / 180.0)).MapVector(op0);
        //    midP0 *= midLen;

        //    var mid = p0 + midP0;

        //    var pathGeo = new PathGeometry();
        //    var pathFig = new PathFigure {
        //        StartPoint = o,
        //    };

        //    pathGeo.Figures.Add(pathFig);

        //    pathFig.Segments.Add(new LineSegment { Point = p1 });
        //    pathFig.Segments.Add(new ArcSegment { Point = p0, Size = new Size(radius, radius), SweepDirection = SweepDirection.Counterclockwise });
        //    pathFig.Segments.Add(new LineSegment { Point = o });

        //    Geometry = pathGeo;
        //}

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

            _vertices = new Point[] {
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


    public class ControlUiEntity : BaseEntity {
        private List<NodeEntity> nodes = new List<NodeEntity>();
        private List<BaseRegionEntity> regions = new List<BaseRegionEntity>();
        private int nextIdx = 1;

        public List<BaseRegionEntity> Regions { get => regions; set => regions = value; }
        public List<NodeEntity> Nodes {
            get => nodes; 
            set {
                nodes = value;
            }
        }

        public double[] Value { get; set; } = new double[2];


        public NodeEntity Create() {
            var node = new NodeEntity() { Id = nextIdx++, Value = new Point() };
            Nodes.Add(node);

            return node;
        }
        public NodeEntity Read(int idx) {
            return Nodes.Where(e => e.Id == idx).FirstOrDefault();
        }


        public void Update(NodeEntity entity) {
            // update node which its index property equals idx
            var idx = nodes.FindIndex(e => e.Id == entity.Id);

            if (idx != -1)
                Nodes[idx] = entity;
        }

        public void Delete(int idx) {
            // delete node which its index property equals idx
            var node = Nodes.Where(e => e.Id == idx).FirstOrDefault();

            Nodes.Remove(node);
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

                Regions = regions.ToList();
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

                Regions = regions.ToList();
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

                Regions = regions.ToList();
            }
        }
    }

    public class MotorEntity : BaseEntity {
        public double Value { get; set; }
        public double Min { get; set; } = -10000d;
        public double Max { get; set; } = 10000d;
        public int NuiBoardId { get; set; } = -1;
        public int NuiMotorId { get; set; } = -1;
    }
}
