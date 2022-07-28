﻿using Numpy;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

    public class NLinearMapEntity : BaseEntity {
        public NDarray Tensor { get; set; }
        public int[] Shape { get; set; }
        public object[] Targets { get; set; }
    }

    public class NodeEntity : BaseEntity {
        public Point Value { get; set; }

    }

    public class SimplexRegionEntity : BaseEntity, IRegionEntity {
        public NodeEntity[] Nodes { get; set; }
        public Point[] Vertices => Nodes.Select(x => x.Value).ToArray();

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
    }

    public class VoronoiRegionEntity : BaseEntity, IRegionEntity {
        public double Factor { get; set; } = 100.0;
        public Point[] Vertices { get; set; }
        public SimplexRegionEntity[] Governors { get; set; }

        public virtual double[] GetLambdas(Point pt, NodeEntity[] collection) { throw new NotImplementedException(); }

    }

    public class SectoralVoronoiRegionEntity : VoronoiRegionEntity {
        public override double[] GetLambdas(Point pt, NodeEntity[] collection) {
            var factors = GetFactors(pt);
            var lambda0 = Governors[0].GetLambdas(pt, collection).Select(e => e * factors[0]).ToArray();
            var lambda1 = Governors[1].GetLambdas(pt, collection).Select(e => e * factors[1]).ToArray();

            return lambda0.Zip(lambda1, (f, s) => (f + s)).ToArray();
        }

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
        public RectVoronoiRegionEntity(NodeEntity[] nodes, SimplexRegionEntity governor) {
            Governors = new[] { governor };
            Invalidate(nodes);
        }

        private void Invalidate(NodeEntity[] nodes) {
            var a = np.array(nodes[0].Value.X, nodes[0].Value.X);
            var b = np.array(nodes[1].Value.X, nodes[1].Value.X);

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

    public interface IRegionEntity {
        //Point[] Vertices { get; }

        //double[] GetLambdas(Point pt, NodeEntity[] collection);
    }

    public class ControlUiEnity : BaseEntity {
        public ICollection<NodeEntity> Nodes { get; set; }
        public ICollection<IRegionEntity> Regions { get; set; }
        public double[] Value { get; set; }
    }

    public class MotorEntity : BaseEntity {
        public double Value { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int BoardId { get; set; }
        public int MotorId { get; set; }
    }
}
