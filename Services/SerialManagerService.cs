using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace taskmaker_wpf.Services {
    public class MotorService {
        public List<Model.Data.Motor> Motors { get; set; } = new List<Model.Data.Motor> { };

        public MotorService() {
            TestPurpose();
        }

        private void TestPurpose() {
            var motor = new Model.Data.Motor();

            motor.SetValue(new[] { (object)5 });

            Motors.Add(motor);
        }
    }
}
