using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Numpy;
using taskmaker_wpf.Model.Data;

namespace taskmaker_wpf.Model.Core {
    public class BaseModel { }

    public partial class UI : IBindable {
        public int Dim => 2;

        private NDarray _inputValue;

        // Input
        public bool SetValue(object[] values) {
            _inputValue = values;

            return true;
        }

        // Output
        public NDarray ToNDarray() {
            // TODO: dont use!
            //var lambdas = Complex.GetLambdas()
            var result = Map.MapTo(_inputValue);
            return result;
        }
    }

    public partial class UI {
        public List<NodeM> Nodes { get; set; } = new List<NodeM>();
        public ComplexM Complex { get; set; }
        public NLinearMap Map { get; set; }


        private IBindable _target;
        public UI() {
        }

        public void Add(NodeM node) {
            Nodes.Add(node);
        }

        public void Add(NDarray<float> location) {
            var newNode = new NodeM(Nodes.Count + 1) {
                Location = location
            };

            Nodes.Add(newNode);
        }

        public void Remove(NodeM node) {
            Nodes.Remove(node);
        }

        public void RemoveAll() {
            Nodes.Clear();
        }

        public IRegionUnit FindRegionById(int id) {
            var v = Complex.Regions.Where(e => e.Hash == id);
            var s = Complex.Simplices.Where(e => e.Hash == id);
            
            return v.Concat<IRegionUnit>(s).FirstOrDefault();
        }

        public void BindTarget(IBindable target) {
            _target = target;
        }

        public void BindData(NodeM node) {
            node.TargetValue?.Dispose();

            node.TargetValue = (NDarray<float>)_target.ToNDarray();

            Map.SetValue(new int[] { node.Id - 1 }, node.TargetValue);
        }

        public (int, NDarray[])[] GetSimplexInfos() {
            return Complex.Simplices
                .Select(e => e.GetSimplexInfo()).ToArray();
        }

        public (int, NDarray[])[] GetVoronoiInfos() {
            return Complex.Regions
                .Select(e => e.GetVoronoiInfo()).ToArray();
        }

        public void CreateRegions() {
            var nodes = np.array(
                Nodes.Select(e => e.Location).ToArray());
            var simplices = Qhull.QhullCSharp.RunDelaunay(nodes)
                .Select(
                    e => new SimplexM(
                        Nodes[e[0]], Nodes[e[1]], Nodes[e[2]]))
                .ToArray();

            // Create simplicial complex
            Complex = new ComplexM();

            Complex.AddSimplices(simplices);

            // reverse for ccw
            var extremes = Qhull.QhullCSharp.RunConvex(nodes)
                .Select(
                    e => Nodes[e])
                .Reverse()
                .ToArray();

            // Create exterior
            Complex.Regions = new List<VoronoiRegionM>(ExteriorM.Create(extremes, simplices));
        }

        public void CreateMap() {
            // Create map
            Map = new NLinearMap(new ComplexBaryD[] { Complex.Bary }, _target.Dim);
        }
    }
}
