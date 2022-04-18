using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Model.Data;

namespace taskmaker_wpf.Services {
    public class MotorService {
        public List<Motor> Motors { get; set; } = new List<Motor> { };

        public MotorService() {
            TestPurpose();
        }

        private void TestPurpose() {
            Enumerable.Range(0, 5)
                .ToList()
                .ForEach(
                e => {
                    var motor = new Motor();

                    motor.SetValue(new[] { (object)e });

                    Motors.Add(motor);
                });
        }
    }
}
