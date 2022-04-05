using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.View.Elements;
using taskmaker_wpf.Model.Core;

namespace taskmaker_wpf.Model {
    public class Unit : BaseModel {
        public Unit Parent { get; set; }
        public List<Unit> Children { get; set; }

        public Func<object, object> UnitFunction { get; set; }

        // Cache endpoint data structure
        private object _cache;
        private UnitElement _element;

        public void Validate() {
            // Do unit function
            _cache = UnitFunction.Invoke(null);
        }
    }

    public class GroupUnit : Unit {
        private List<Unit> _units;

        public GroupUnit() { }


    }

    public class MotorUnit: Unit {
        public MotorUnit() { } 
    }
}
