using System;
using System.Collections.Generic;
using System.IO.Ports;
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
                    motor.Label = $"Motor{e}";
                    motor.Alias = $"Motor{e}";

                    Motors.Add(motor);
                });
        }
    }

    public class SerialService {
        public List<string> Ports { get; set; } = new List<string> { };

        private SerialPort _serial;

        public SerialService() {
        }

        public string[] ListAllPorts() {
            Ports = new List<string>(SerialPort.GetPortNames());

            return Ports.ToArray();
        }

        public void Connect(string name) {
            _serial = new SerialPort(name);
        }
    }
}
