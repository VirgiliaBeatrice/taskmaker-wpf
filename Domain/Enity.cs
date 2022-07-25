using Numpy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace taskmaker_wpf.Domain {
    public interface IEntity {
        string Name { get; set; }
        Guid Id { get; set; }
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
        public string Name { get; set; }
        public Guid Id { get; set; }
    }

    public class NLinearMapEntity : BaseEntity {
    }

    public class NodeEntity : BaseEntity {
        public Point Value { get; set; }

    }

    public class SimplexRegionEntity : BaseEntity, IRegionEntity {
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
    }

    public class VoronoiRegionEntity : BaseEntity, IRegionEntity {
        public double Factor { get; set; } = 100.0;
        public Point[] Vertices { get; set; }

        public virtual double[] GetLambdas(Point pt, NodeEntity[] collection) { throw new NotImplementedException(); }

    }

    public class SectoralVoronoiRegionEntity : VoronoiRegionEntity {
        public override double[] GetLambdas(Point pt, NodeEntity[] collection) {
            return base.GetLambdas(pt, collection);
        }

        public SectoralVoronoiRegionEntity(NodeEntity[] nodes, SimplexRegionEntity[] simplices) {
            
        }

        private void Initialize(NodeEntity[] nodes) {
            var prev = nodes[0];
            var it = nodes[1];
            var next = nodes[2];

            var dir0 = prev.Value - it.Value;
            var stdDir0 = dir0 / np.linalg.norm(dir0);
            var dir1 = next.Value - it.Value;
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

            var ray0 = it.Value + np.dot(theta0, stdDir0) * Factor;
            var ray1 = it.Value + np.dot(theta1, stdDir1) * Factor;

            _rays = new NDarray[] {
                ray0, ray1
            };
            Vertices = new NDarray[] {
                ray0, it.Value, ray1
            };
        }
    }

    public class RectVoronoiRegionEntity : VoronoiRegionEntity { }

    public interface IRegionEntity {
        //Point[] Vertices { get; }

        double[] GetLambdas(Point pt, NodeEntity[] collection);
    }

    public class ComplexEntity : BaseEntity {
        public NodeEntity[] Nodes { get; set; }
        public IRegionEntity[] Regions { get; set; }
    }


    public class ControlUiEnity : BaseEntity {
        public ComplexEntity Complex { get; set; }
        public double[] Value { get; set; }

        
    }
}
