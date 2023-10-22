using Numpy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cMotor = PCController.Motor;
using PCController;
using System.IO.Ports;
using taskmaker_wpf.Model.Core;
using taskmaker_wpf.Views.Widget;
using Prism.Mvvm;
using taskmaker_wpf.Views;
using taskmaker_wpf.Services;
using Prism.Modularity;
using Prism.Ioc;

namespace taskmaker_wpf.Model.Data {
    public class MotorAgent {
        private SerialService _serialSvr;

        private List<Motor> _repository;
        public List<Motor> Repository => _repository;


        public MotorAgent(SerialService serialService) { 
            _serialSvr = serialService;

            _repository = new List<Motor>();
        }

        public Motor AddMotor() {
            var motor = new Motor();
            _repository.Add(motor);

            return motor;
        }
    }


    public class Motor : ITarget {
        private double _value;
        public double Value {
            get => _value;
            set {
                _value = value;

                if (_instance != null) {
                    _instance.Value = (int)_value;
                }
            }
        }

        private int _max;
        private int _min;
        private string _name;
        private string _label;
        private string _id;

        private cMotor _instance;
        private int _boardId;
        private int _motorId;
        


        public int Max { get => _max; set => _max = value; }
        public int Min { get => _min; set => _min = value; }
        public string Name { get => _name; set => _name = value; }
        public string Label { get => _label; set => _label = value; }
        public string Id { get => _id; set => _id = value; }
        public int BoardId { get => _boardId; set => _boardId = value; }
        public int MotorId { get => _motorId; set => _motorId = value; }

        public Motor() {
            _value = 0;
            _max = 100;
            _min = -100;
        }

        //public bool SetValue(object[] value) {
        //    _value = (int)value[0];

        //    return true;
        //}

        public void Link(cMotor instance, int boardId, int motorId) {
            _instance = instance;
            BoardId = boardId;
            MotorId = motorId;
        }

        //public NDarray ToNDarray() {
        //    return np.array<float>(_value);
        //}

        public override string ToString() {
            return $"Motor[{Name}]";
        }
    }

    public interface ITarget {
        string Name { get; set; }
        double Value { get; set; }
    }

    public class BindableTargetCollection : List<ITarget>, IBindableTarget {
        public int Dim => Count;

        public TValue GetValue<TValue>() {
            throw new NotImplementedException();
        }

        public bool SetValue(object[] values) {
            for (var i = 0; i < values.Length; i++) {
                this[i].Value = (int)values[i];
            }

            return true;
        }

        public void SetValue<TValue>(TValue values) {
            if (values is double[] inputs) {
                for (var i = 0; i < inputs.Length; i++) {
                    this[i].Value = inputs[i];
                }
            }
            else {
                throw new NotSupportedException();
            }
        }

        public NDarray ToNDarray() {
            return np.array(this.Select(m => (float)m.Value).ToArray());
        }

        public override string ToString() {
            return string.Join(", ", this.Select(e => (int)e.Value));
        }
    }

    //public class Nuibot {
    //    public SerialPort Port { get; set; }
    //    public Boards Boards { get; set; } = new Boards();
    //    public Motors Motors { get; set; } = new Motors();

    //    private SerialService _serialSvr;

    //    public Nuibot(SerialService serialService) {
    //        _serialSvr = serialService;
    //    }

    //    public void Init(string[] args) {
    //        string[] ports = SerialPort.GetPortNames();

    //        Console.WriteLine(string.Join(", ", ports));

    //        ConectToBoards();
    //    }

    //    private void ConectToBoards() {
    //        if (Port.IsOpen)
    //            Port.Close();

    //        Port.PortName = "COM3";
    //        Port.BaudRate = 2000000;

    //        try {
    //            Port.Open();
    //        }
    //        catch {
    //            return;
    //        }

    //        if (Port.IsOpen) {
    //            Boards.Clear();
    //            Boards.EnumerateBoard();

    //            ResetMotor();

    //            if (Boards.NMotor != 0) {
    //                Console.WriteLine("Nuibot is ready.");
    //            }
    //        }
    //    }

    //    private void ResetMotor() {
    //        Motors.Clear();

    //        for (int i = 0; i < Boards.NMotor; ++i) {
    //            PCController.Motor m = new PCController.Motor();

    //            Motors.Add(m);
    //        }

    //        short[] k = new short[Boards.NMotor];
    //        short[] b = new short[Boards.NMotor];
    //        short[] a = new short[Boards.NMotor];
    //        short[] limit = new short[Boards.NMotor];
    //        short[] release = new short[Boards.NMotor];
    //        short[] torqueMin = new short[Boards.NMotor];
    //        short[] torqueMax = new short[Boards.NMotor];

    //        Boards.RecvParamPd(ref k, ref b);
    //        Boards.RecvParamCurrent(ref a);
    //        Boards.RecvParamTorque(ref torqueMin, ref torqueMax);
    //        Boards.RecvParamHeat(ref limit, ref release);

    //        for (int i = 0; i < Boards.NMotor; ++i) {
    //            Motors[i].pd.K = k[i];
    //            Motors[i].pd.B = b[i];
    //            Motors[i].pd.A = a[i];
    //            if (limit[i] > 32000) limit[i] = 32000;
    //            if (limit[i] < 0) limit[i] = 0;
    //            Motors[i].heat.HeatLimit = limit[i] * release[i];
    //            Motors[i].heat.HeatRelease = release[i];
    //            Motors[i].torque.Minimum = torqueMin[i];
    //            Motors[i].torque.Maximum = torqueMax[i];
    //        }
    //    }
    //}
}
