using Numpy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Model.SimplicialMapping;

namespace taskmaker_wpf.Models {
    public enum ControlUIState {
        Init = 0,
        Created,
        Mapped
    }

    public class ControlUI : ISelectableTarget {
        public NLinearMap Map { get; private set; } = new NLinearMap();
        public ComplexM Complex { get; private set; } = new ComplexM();

        public string Name { get; set; } = "ControlUI";
        public double Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsSelected { get; set; } = false;

        public ControlUI() { }

        public override string ToString() {
            return Name;
        }
    }
}
