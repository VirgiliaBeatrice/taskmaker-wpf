using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Numpy;
using SkiaSharp;

namespace taskmaker_wpf.Model.Data {
    public class NodeM : IDisposable {
        private bool disposedValue;

        public int Id { get; set; }
        public NDarray<float> Location { get; set; }
        public NDarray<float> TargetValue { get; set; }

        public NodeM(int id) {
            Id = id;
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                    (Location as IDisposable)?.Dispose();
                    (TargetValue as IDisposable)?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        //~NodeM() {
        //    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //    Dispose(disposing: false);
        //}

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public interface IRegionUnit {
        IBary Bary { get; set; }
    }


    public class SimplexM : IDisposable, IRegionUnit {
        private bool disposedValue;

        public int Hash { get; set; }
        public List<NodeM> Nodes { get; set; } = new List<NodeM>();
        public IBary Bary { get; set; } = null;

        public SimplexM(NodeM n0, NodeM n1, NodeM n2) : this(new NodeM[] { n0, n1, n2 }) { }

        public SimplexM() {
            Hash = GetHashCode();
        }

        public SimplexM(IEnumerable<NodeM> nodes) : this() {
            Nodes.AddRange(nodes);
        }

        public bool IsVertex(NodeM node) {
            return Nodes.Contains(node);
        }

        public (int, NDarray[]) GetSimplexInfo() {
            return (Hash, Nodes.Select(e => e.Location).ToArray());
        }

        public void SetBary() {
            var indices = np.asarray(Nodes.Select(e => e.Id).ToArray());
            var basis = np.array(Nodes.Select(e => e.Location).ToArray());

            Bary = new SimplexBaryD(basis);
            Bary.Indices = indices;
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                    foreach (var c in Nodes) {
                        c.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SimplexM()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


    public class ComplexM : IDisposable {
        private bool disposedValue;
        public int Hash { get; set; }
        public List<SimplexM> Simplices { get; set; } = new List<SimplexM>();
        public List<VoronoiRegionM> Regions { get; set; } = new List<VoronoiRegionM>();
        //public ExteriorM Exterior { get; set; }

        public ComplexBaryD Bary { get; set; } = null;

        public ComplexM() {
            Hash = GetHashCode();
        }

        public void AddSimplex(params NodeM[] nodes) {
            if (nodes.Length == 3) {
                var simplex = new SimplexM(nodes);

                Simplices.Add(simplex);
            } else {
                throw new NotImplementedException();

            }
        }

        public void AddSimplices(IEnumerable<SimplexM> nodes) {
            Simplices.AddRange(nodes);
        }

        public void SetBary() {
            Simplices.ForEach(e => e.SetBary());
            Regions.ForEach(e => e.SetBary());

            var r1 = Simplices
                .SelectMany(x => x.Nodes).ToArray();
            var r2 = r1.Distinct().ToArray();
            var r3 = r2.Select(e => e.Location).ToArray();

            var basis = np.array(Simplices
                .SelectMany(x => x.Nodes)
                .Distinct()
                .Select(e => e.Location));
            var simplexBarys = Simplices
                .Select(x => x.Bary).ToArray<IBary>();
            var voronoiBarys = Regions
                .Select(x => x.Bary).ToArray<IBary>();
            var allBarys = simplexBarys
                .Concat(voronoiBarys).ToArray();

            Bary = new ComplexBaryD(
                basis,
                allBarys);
        }
        public NDarray GetLambdas(IRegionUnit target, NDarray p) {
            return Bary.GetLambdas(target.Bary, p);
        }

        public NDarray GetLambdas(SimplexM target, NDarray p) {
            return Bary.GetLambdas(target.Bary, p);
        }

        public NDarray GetLambdas(VoronoiRegionM target, NDarray p) {
            return Bary.GetLambdas(target.Bary, p);
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                    foreach(var s in Simplices) {
                        s.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SimplicialComplexM()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


    public class ExteriorM : IDisposable {
        private bool disposedValue;

        public List<VoronoiRegionM> Regions { get; set; } = new List<VoronoiRegionM>();

        public ExteriorM(IEnumerable<VoronoiRegionM> regions) {
            Regions.AddRange(regions);
        }

        public static VoronoiRegionM[] Create(NodeM[] extremes, SimplexM[] simplices) {
            if (extremes.Length <= 3) {
                throw new Exception("Invalid extremes");
            }

            NodeM prev, it, next;

            var newRegions = new List<VoronoiRegionM>();

            for (var i = 0; i < extremes.Length; i++) {
                it = extremes[i];

                if (i <= 0) {
                    prev = extremes.Last();
                    next = extremes[i + 1];
                }
                else if (i > 0 & i < extremes.Length - 1) {
                    prev = extremes[i - 1];
                    next = extremes[i + 1];
                }
                //else if (i >= extremes.Length - 1) {
                else {
                prev = extremes[i - 1];
                    next = extremes.First();
                }

                // Find governor simplices for sectoral voronoi region
                var prevGov = Array.Find(
                    simplices, 
                    e => e.IsVertex(prev) & e.IsVertex(it));
                var nextGov = Array.Find(
                    simplices, 
                    e => e.IsVertex(it) & e.IsVertex(next));
                var sectorVoro = new SectoralVoronoiRegion(
                    new NodeM[] { prev, it, next},
                    new SimplexM[] { prevGov, nextGov });

                newRegions.Add(sectorVoro);

                // Find governor simplices for rectangular voronoi region
                var gov = Array.Find(simplices, 
                    e => e.IsVertex(it) & e.IsVertex(next));
                var rectVoro = new RectVoronoiRegion(
                    new NodeM[] { it, next },
                    gov);

                newRegions.Add(rectVoro);
            }

            return newRegions.ToArray();
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                    foreach(var c in Regions) {
                        c.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ExteriorM()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public abstract class VoronoiRegionM : IDisposable, IRegionUnit {
        //public abstract SimplexBary GetBary();
        public int Hash { get; set; }
        public abstract IBary Bary { get; set; }
        public abstract (int, NDarray[]) GetVoronoiInfo();
        public abstract void SetBary();

        public VoronoiRegionM() {
            Hash = GetHashCode();
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }

    public struct RectVoronoiRegionRecord {
        public float radius;
        public float[] a;
        public float[] b;
        public float[] bP;
        public float[] aP;
    }

    public class RectVoronoiRegion : VoronoiRegionM {
        private bool disposedValue;

        public float Factor { get; set; } = 100.0f;
        public SimplexM Governor { get; set; }
        public NodeM Node0 { get; set; }
        public NodeM Node1 { get; set; }
        public override IBary Bary { get; set; }

        public NDarray[] RectVertices { get; set; }

        private NDarray[] CreateRectangleVertices(NDarray a, NDarray b) {
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

                theta.Dispose();

                return np.array(new double[] { e00, e01, e10, e11 }).reshape(2, 2);
            }

            dir.Dispose();
            dirHat.Dispose();
            perp.Dispose();

            return new NDarray[] { a, b, bP, aP };
        }


        public override string ToString() {
            return $"RectVor. - {Governor}";
        }

        public override void SetBary() {
            Bary = new VoronoiBaryD(new SimplexBaryD[] { Governor.Bary as SimplexBaryD });
        }

        public RectVoronoiRegionRecord ToRecord() {
            var vertices = RectVertices
                .Select(e => 
                    e.GetData<double>()
                        .Select(i => Convert.ToSingle(i)).ToArray())
                .ToArray();

            return new RectVoronoiRegionRecord {
                radius = Factor,
                a = vertices[0],
                b = vertices[1],
                bP = vertices[2],
                aP = vertices[3]
            };
        }

        public RectVoronoiRegion(NodeM[] nodes, SimplexM governor) {
            Governor = governor;
            Node0 = nodes[0];
            Node1 = nodes[1];

            RectVertices = CreateRectangleVertices(Node0.Location, Node1.Location);
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                foreach(var v in RectVertices) {
                    v.Dispose();
                }

                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~RectVoronoiRegion() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }


        public override (int, NDarray[]) GetVoronoiInfo() {
            return (Hash, RectVertices);
        }
    }

    public struct SectoralVoronoiRegionRecord {
        public float radius;
        public float[] o;
        public float[] a;
        public float[] b;
    }

    public class SectoralVoronoiRegion: VoronoiRegionM {
        public float Factor { get; set; } = 100.0f;
        public SimplexM[] Governors { get; set; }
        public NodeM Node => Nodes[1];
        public NodeM[] Nodes { get; set; }
        public NDarray[] Vertices { get; set; }
        public override IBary Bary { get; set; }


        private NDarray[] _rays;
        private bool disposedValue;

        public SectoralVoronoiRegion(NodeM[] nodes, SimplexM[] simplices) {
            Governors = simplices;
            Nodes = nodes;

            CreateSector();
        }

        private void CreateSector() {
            var prev = Nodes[0];
            var it = Nodes[1];
            var next = Nodes[2];

            var dir0 = prev.Location - it.Location;
            var stdDir0 = dir0 / np.linalg.norm(dir0);
            var dir1 = next.Location - it.Location;
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

            var ray0 = it.Location + np.dot(theta0, stdDir0) * Factor;
            var ray1 = it.Location + np.dot(theta1, stdDir1) * Factor;

            _rays = new NDarray[] {
                ray0, ray1
            };
            Vertices = new NDarray[] {
                ray0, it.Location, ray1
            };
        }

        private NDarray GetFactors(NDarray p) {
            var a = Nodes[0].Location;
            var o = Nodes[1].Location;
            var b = Nodes[2].Location;

            var ao = a - o;
            var bo = b - o;
            var po = p - o;

            var theta0 = np.arcsin(
                np.cross(ao, po) / (np.linalg.norm(ao) * np.linalg.norm(po)));
            var theta1 = np.arcsin(
                np.cross(bo, po) / (np.linalg.norm(bo) * np.linalg.norm(po)));

            return np.array(new NDarray[] { theta0, theta1 }).squeeze();

        }

        public override (int, NDarray[]) GetVoronoiInfo() {
            return (Hash, Vertices);
        }

        public override string ToString() {
            return $"SectoralVor. - Type {Governors.Distinct().Count()}";
        }

        public override void SetBary() {
            Bary = new VoronoiBaryD(
                Governors.Select(e => e.Bary as SimplexBaryD).ToArray());
            (Bary as VoronoiBaryD).GetFactors += GetFactors;
            
        }

        public SectoralVoronoiRegionRecord ToRecord() {
            return new SectoralVoronoiRegionRecord {
                radius = Factor,
                o = Node.Location.GetData<float>(),
                a = _rays[0].GetData<float>(),
                b = _rays[1].GetData<float>(),
            };
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                foreach (var r in _rays) {
                    r.Dispose();
                }

                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~SectoralVoronoiRegion() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }
    }
}
