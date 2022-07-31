using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCController;

namespace taskmaker_wpf.Services {
    public class SerialService {
        public List<string> Ports { get; set; } = new List<string> { };
        public Motors Motors { get; set; }
        public Boards Boards { get; set; }


        public bool IsConnected => _serial != null;
        private SerialPort _serial;


        private int _nBoard;
        private int _nMotor;

        public SerialService() { }

        public string[] ListAllPorts() {
            Ports = new List<string>(SerialPort.GetPortNames());

            return Ports.ToArray();
        }

        public int Connect(string name) {
            if (_serial != null) {
                _serial.Close();
            }

            _serial = new SerialPort(name);

            try {
                _serial.Open();
            }
            catch {
                return -1;
            }

            Boards = new Boards();
            Boards.Serial = _serial;
            Boards.Serial.BaudRate = 2000000;

            Motors = new Motors();

            Boards.Clear();
            Motors.Clear();

            Boards.EnumerateBoard();
            _nBoard = Boards.Count;

            _nMotor = Boards.NMotor;
            Motors.AddRange(Enumerable.Range(0, _nMotor).Select(e => new Motor()));

            return 0;
        }

        public Motor GetMotorInstance(int board, int motor) {
            return Motors[board * _nBoard + motor];
        }

        public void Update(int boardId, int motorId, double value) {
            if (IsConnected)
                Motors[boardId * 4 + motorId].position.Value = (int)value;
        }

        public void SendToNuibot() {
            if (!IsConnected) return;

            short[] targets = new short[Boards.NMotor];

            for (int i = 0; i < Motors.Count; ++i) {
                targets[i] = (short)Motors[i].position.Value;
            }

            Boards.SendPosDirect(targets);
        }
    }
}
