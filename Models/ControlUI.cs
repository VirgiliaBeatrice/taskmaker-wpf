using Numpy;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Model.SimplicialMapping;

namespace taskmaker_wpf.Models {
    public class ControlUi : BindableBase, ITarget {
        //public NLinearMap Map { get; private set; } = new NLinearMap();
        public ComplexM Complex { get; private set; } = new ComplexM();

        public string Name { get; set; } = "ControlUI";
        public double Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsSelected { get; set; } = false;

        public ControlUi() { }

        public NodeM AddNode(NDarray<float> pt) {
            var node = new NodeM {
                Location = pt
            };

            Complex.Nodes.Add(node);

            return node;
        }

        public void SetTargets(ITarget[] targets) {
            Complex.Targets.Clear();
            Complex.Targets.AddRange(targets);
        }

        public void SetMap() {
            //Map.Invalidate(Complex.Targets.Dim);
            //Map.Invalidate(new ComplexBaryD[] { Complex.Bary });
        }


        public override string ToString() {
            return $"ControlUI[{Name}]";
        }
    }
}
