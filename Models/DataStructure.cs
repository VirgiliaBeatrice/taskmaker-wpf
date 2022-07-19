using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Numpy;
using SkiaSharp;
using taskmaker_wpf.ViewModels;
using taskmaker_wpf.Qhull;
using Prism.Mvvm;

namespace taskmaker_wpf.Model.Data {
    public class NodeM : BindableBase, IDisposable {
        private bool disposedValue;

        public Guid Uid { get; set; }
        public int Id { get; set; }

        private NDarray<float> _location;
        public NDarray<float> Location {
            get => _location;
            set => SetProperty(ref _location, value);
        }
        public NDarray<float> TargetValue { get; set; }
        public bool IsSet { get; set; } = false;

        public NodeM(int id) {
            Id = id;
        }

        public NodeM() {
            Uid = Guid.NewGuid();
        }

        //public NodeData ToData() {
        //    return new NodeData {
        //        Uid = Uid,
        //        Location = Location.ToPoint(),
        //        IsSet = IsSet
        //    };
        //}

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

    public interface IRegion {
        IBary Bary { get; set; }
    }


    public class SimplexM : IDisposable, IRegion {
        private bool disposedValue;
        public Guid Uid { get; set; }
        public List<NodeM> Nodes { get; set; } = new List<NodeM>();
        public IBary Bary { get; set; } = null;

        public SimplexM(NodeM n0, NodeM n1, NodeM n2) : this(new NodeM[] { n0, n1, n2 }) { }

        public SimplexM() {
            Uid = Guid.NewGuid();
        }

        public SimplexM(IEnumerable<NodeM> nodes) : this() {
            Nodes.AddRange(nodes);
        }

        public bool IsVertex(NodeM node) {
            return Nodes.Contains(node);
        }

        //public SimplexData ToData() {
        //    var data = new SimplexData {
        //        Uid = Uid,
        //        Points = Nodes.Select(e => e.Location.ToPoint())
        //                      .ToArray()
        //    };

        //    return data;
        //}

        public void SetBary() {
            Bary = new SimplexBaryD(Nodes);
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

    public class ComplexM : ITarget, IDisposable {
        private bool disposedValue;

        // temporary solution, delete soon
        public string Name { get; set; }
        public Guid Uid { get; set; }

        public List<NodeM> Nodes { get; set; } = new List<NodeM>();
        public List<SimplexM> Simplices { get; set; } = new List<SimplexM>();
        public List<VoronoiRegionM> Regions { get; set; } = new List<VoronoiRegionM>();

        public BindableTargetCollection Targets { get; set; } = new BindableTargetCollection();

        public ComplexBaryD Bary { get; set; } = null;
        public double Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ComplexM() {
            Uid = new Guid();
        }

        public NodeM Add(NDarray<float> pt) {
            var node = new NodeM() {
                Location = pt
            };

            Nodes.Add(node);

            return node;
        }

        public void RemoveAt(Guid uid) {
            Nodes.RemoveAll(e => e.Uid == uid);
        }

        public void CreateComplex() {
            var nodes = np.array(
                Nodes.Select(e => e.Location)
                .ToArray());

            var simplices = QhullCSharp.RunDelaunay(nodes)
                .Select(
                    e => new SimplexM(
                        Nodes.ElementAt(e[0]),
                        Nodes.ElementAt(e[1]),
                        Nodes.ElementAt(e[2])))
                .ToArray();

            AddSimplices(simplices);

            // reverse for ccw
            var extremes = QhullCSharp.RunConvex(nodes)
                .Select(
                    e => Nodes.ElementAt(e))
                .Reverse()
                .ToArray();

            // Create exterior
            Regions = new List<VoronoiRegionM>(ExteriorM.Create(extremes, simplices));

            // Initial barys
            InitializeBarys();
        }

        //public SimplexData[] GetSimplexData() {
        //    return Simplices.Select(e => e.ToData()).ToArray();
        //}

        //public VoronoiData[] GetVoronoiData() {
        //    return Regions.Select(e => e.ToData()).ToArray();
        //}

        internal void AddSimplices(IEnumerable<SimplexM> nodes) {
            Simplices.AddRange(nodes);
        }

        internal void InitializeBarys() {
            Simplices.ForEach(e => e.SetBary());
            Regions.ForEach(e => e.SetBary());

            Bary = new ComplexBaryD(Nodes);
        }

        public IRegion FindRegionById(Guid id) {
            var a = Regions.Find(e => e.Uid == id);
            var b = Simplices.Find(e => e.Uid == id);

            if (a != null)
                return a;

            if (b != null)
                return b;

            return null;
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

        public static VoronoiRegionM[] Create(NodeM[] extremes, SimplexM[] simplices) {
            if (extremes.Length <= 2) {
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
                else if (i < extremes.Length - 1) {
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
                    new NodeM[] { prev, it, next },
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

    public abstract class VoronoiRegionM : IDisposable, IRegion {
        //public abstract SimplexBary GetBary();
        public Guid Uid { get; set; }
        public abstract IBary Bary { get; set; }
        //public abstract VoronoiData ToData();
        public abstract void SetBary();

        protected VoronoiRegionM() {
            Uid = Guid.NewGuid();
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
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

        //public override VoronoiData ToData() {
        //    return new VoronoiData {
        //        Uid = Uid,
        //        Points = RectVertices.Select(e => e.ToPoint()).ToArray()
        //    };
        //}
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
            //var a = Nodes[0].Location;
            var a = _rays[0];
            var o = Nodes[1].Location;
            var b = _rays[1];
            //var b = Nodes[2].Location;

            var ao = a - o;
            var bo = b - o;
            var po = p - o;

            var theta0 = np.abs(np.arccos(
                np.dot(ao, po) / (np.linalg.norm(ao) * np.linalg.norm(po))));
            var theta1 = np.abs(np.arccos(
                np.dot(bo, po) / (np.linalg.norm(bo) * np.linalg.norm(po))));
            var theta = theta0 + theta1;

            return np.array(new NDarray[] { theta1 / theta, theta0 / theta });

        }

        public override string ToString() {
            return $"SectoralVor. - Type {Governors.Distinct().Count()}";
        }

        public override void SetBary() {
            Bary = new VoronoiBaryD(
                Governors.Select(e => e.Bary as SimplexBaryD).ToArray());
            (Bary as VoronoiBaryD).GetFactors += GetFactors;
            
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

        //public override VoronoiData ToData() {
        //    return new VoronoiData {
        //        Uid = Uid,
        //        Points = Vertices.Select(e => e.ToPoint()).ToArray()
        //    };
        //}

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~SectoralVoronoiRegion() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }
    }
}
