﻿using Numpy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCController;
using System.IO.Ports;

namespace taskmaker_wpf.Model.Data {
    public interface IBindable {
        int Dim { get; }
        bool SetValue(object[] values);
        NDarray ToNDarray();
    }

    public class Motor : IBindable {
        private int _value;
        public int Value {
            get => _value;
        }
        public int Dim => 1;
        public PCController.Motor Entity { get; set; }

        public Motor() {
            _value = 0;
        }

        public bool SetValue(object[] value) {
            _value = (int)value[0];

            return true;
        }

        public void BindEntity(PCController.Motor entity) {
            Entity = entity;
        }

        public NDarray ToNDarray() {
            return np.array<float>(_value);
        }
    }

    public class MotorCollection : IBindable {
        public int Dim => Motors.Count;
        public List<Motor> Motors { get; set; } = new List<Motor>();

        public bool SetValue(object[] values) {
            for (var i = 0; i < values.Length; i++) {
                Motors[i].SetValue(new object[] { values[i] });
            }

            return true;
        }

        public NDarray ToNDarray() {
            return np.array(Motors.Select(m => (float)m.Value).ToArray());
        }
    }

    static public class Nuibot {
        static public SerialPort Port { get; set; }
        static public Boards Boards { get; set; } = new Boards();
        static public Motors Motors { get; set; } = new Motors();


        public static void Init(string[] args) {
            string[] ports = SerialPort.GetPortNames();

            Console.WriteLine(string.Join(", ", ports));

            ConectToBoards();
        }

        static private void ConectToBoards() {
            if (Port.IsOpen)
                Port.Close();

            Port.PortName = "COM3";
            Port.BaudRate = 2000000;

            try {
                Port.Open();
            }
            catch {
                return;
            }

            if (Port.IsOpen) {
                Boards.Clear();
                Boards.EnumerateBoard();

                ResetMotor();

                if (Boards.NMotor != 0) {
                    Console.WriteLine("Nuibot is ready.");
                }
            }
        }

        static private void ResetMotor() {
            Motors.Clear();

            for (int i = 0; i < Boards.NMotor; ++i) {
                PCController.Motor m = new PCController.Motor();

                Motors.Add(m);
            }

            short[] k = new short[Boards.NMotor];
            short[] b = new short[Boards.NMotor];
            short[] a = new short[Boards.NMotor];
            short[] limit = new short[Boards.NMotor];
            short[] release = new short[Boards.NMotor];
            short[] torqueMin = new short[Boards.NMotor];
            short[] torqueMax = new short[Boards.NMotor];

            Boards.RecvParamPd(ref k, ref b);
            Boards.RecvParamCurrent(ref a);
            Boards.RecvParamTorque(ref torqueMin, ref torqueMax);
            Boards.RecvParamHeat(ref limit, ref release);

            for (int i = 0; i < Boards.NMotor; ++i) {
                Motors[i].pd.K = k[i];
                Motors[i].pd.B = b[i];
                Motors[i].pd.A = a[i];
                if (limit[i] > 32000) limit[i] = 32000;
                if (limit[i] < 0) limit[i] = 0;
                Motors[i].heat.HeatLimit = limit[i] * release[i];
                Motors[i].heat.HeatRelease = release[i];
                Motors[i].torque.Minimum = torqueMin[i];
                Motors[i].torque.Maximum = torqueMax[i];
            }
        }
    }
}
