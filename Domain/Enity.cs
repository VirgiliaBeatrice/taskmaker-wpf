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
        [System.Xml.Serialization.XmlIgnore]
        public NDarray Tensor { get; set; }
        public int[] Shape { get; set; }
        //public string[] Targets { get; set; }
        //public (string, int)[] Targets { get; set; }
        

        private bool _isSet => !Tensor.isnan().any();
        private double[] tensor => Tensor.isnan().any() ? null : Tensor.GetData<double>();

        public static NLinearMapEntity Create(ControlUiEntity ui) {
            var map = new NLinearMapEntity();

            map.Initialize(ui);

            //var targetDim = ui.Targets.Select(e => e.Dimension).Sum();
            //var basisDim = new int[] { ui.Nodes.Length };

            //map.Shape = new int[] { targetDim }.Concat(basisDim).ToArray();
            //map.Tensor = np.empty(map.Shape);

            //map.Tensor.fill(np.nan);

            //var basis = ui.Nodes;
            //for (int idx = 0; idx < basis.Length; idx++) {
            //    map.Tensor[$":,{string.Join(",", new int[] { idx })}"] = basis[idx].TargetValue;
            //}

            return map;
        }

        public void Initialize(ControlUiEntity ui) {
            var targetDim = ui.Targets.Select(e => e.Dimension).Sum();
            var basisDim = new int[] { ui.Nodes.Length };

            Shape = new int[] { targetDim }.Concat(basisDim).ToArray();
            Tensor = np.empty(Shape);

            Tensor.fill(np.nan);

            var basis = ui.Nodes;
            for (int idx = 0; idx < basis.Length; idx++) {
                Tensor[$":,{string.Join(",", new int[] { idx })}"] = basis[idx].TargetValue;
            }
        }

        public void SetValue(int[] indices, NDarray value) {
            // only 1 bary
            Tensor[$":,{indices[0]}"] = np.atleast_2d(value);
        }

        public NDarray MapTo(NDarray lambdas) {
            if (!_isSet)
                return null;

            NDarray kronProd = null;

            for (var i = 0; i < lambdas.shape[0]; i++) {
                if (i == 0)
                    kronProd = np.array(lambdas[$"{i},:"]);
                else
                    kronProd = np.kron(kronProd, lambdas[$"{i},:"]);
            }

            var w = np.dot(Tensor.reshape(Shape[0], -1), kronProd);

            return w;
        }

    }

    public class NodeEntity : BaseEntity {
        public Point Value { get; set; }
        public double[] TargetValue { get; set; }

    }

    public abstract class BaseRegionEntity : BaseEntity { }

    public class SimplexRegionEntity : BaseRegionEntity {
        public NodeEntity[] Nodes { get; set; }
        public Point[] Vertices => Nodes.Select(x => x.Value).ToArray();

        public SimplexRegionEntity() { }

        public SimplexRegionEntity(NodeEntity[] nodes) {
            Nodes = nodes;
        }

        public double[] GetLambdas(Point pt, NodeEntity[] collection) {
            var lambdas = Bary.GetLambdas(Vertices, pt);
            var indices = Nodes
                .Select(e => collection.ToList().IndexOf(e))
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

    public class VoronoiRegionEntity : BaseRegionEntity {
        public double Factor { get; set; } = 100.0;
        public Point[] Vertices { get; set; }
        public SimplexRegionEntity[] Governors { get; set; }

        public VoronoiRegionEntity() { }

        public virtual double[] GetLambdas(Point pt, NodeEntity[] collection) { throw new NotImplementedException(); }

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
            var factors = GetFactors(pt);
            var lambda0 = Governors[0].GetLambdas(pt, collection).Select(e => e * factors[0]).ToArray();
            var lambda1 = Governors[1].GetLambdas(pt, collection).Select(e => e * factors[1]).ToArray();

            return lambda0.Zip(lambda1, (f, s) => (f + s)).ToArray();
        }

        public SectoralVoronoiRegionEntity() { }

        public SectoralVoronoiRegionEntity(NodeEntity[] nodes, SimplexRegionEntity[] simplices) {
            Governors = simplices;
            Invalidate(nodes);
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

    public interface ITargetableEntity {
        string TargetType { get; }
        int Id { get; }
    }

    public class ControlUiEntity : BaseEntity, ITargetableEntity {
        public NodeEntity[] Nodes { get; set; }
        public BaseRegionEntity[] Regions { get; set; }
        public double[] Value { get; set; }

        public string TargetType => "ControlUi";
        public TargetEntity[] Targets { get; set; }

        public void Build() {
            var nodes = Nodes.OrderBy(e => e.Id).ToArray();
            var input = np.array(
                nodes.Select(e => np.array(new[] { e.Value.X, e.Value.Y }))
                    .ToArray());

            if (nodes.Length <= 2) return;

            var regions = new List<BaseRegionEntity>();

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

            Regions = regions.ToArray();
        }

        public NLinearMapEntity CreateMap() {
            if (Nodes.Any(e => e.TargetValue == null)) {
                return default;
            }
            else {
                return NLinearMapEntity.Create(this);
            }
        }
    }

    public class MotorEntity : BaseEntity, ITargetableEntity {
        public double[] Value { get; set; } = new double[1] { 0 };
        public int Min { get; set; }
        public int Max { get; set; }
        public int BoardId { get; set; }
        public int MotorId { get; set; }
        public string TargetType => "Motor";
    }

    public struct TargetState {
        public string Name { get; set; }
        public int Id { get; set; }
        public int Dimension { get; set; }
    }

    public struct TargetEntity {
        public string Name { get; set; }
        public int Id { get; set; }
        public int Dimension => Name.Contains("Motor") ? 1 : 2;
    }
}
