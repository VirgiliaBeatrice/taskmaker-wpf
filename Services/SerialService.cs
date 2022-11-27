using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Timers;
using PCController;
using taskmaker_wpf.Domain;

namespace taskmaker_wpf.Services {
    public class SerialService {
        public List<string> Ports { get; set; } = new List<string> { };
        public Motors Motors { get; set; }
        public Boards Boards { get; set; }

        public Queue<short[]> MessageQueue { get; set; } = new Queue<short[]>();
        public bool IsConnected => _serial != null;
        private SerialPort _serial;


        private int _nBoard;
        private int _nMotor;
        private short[] _buffer;
        private Timer _timer;

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

            _buffer = new short[Boards.NMotor];
            _timer = new Timer();

            _timer.Interval = 20;
            _timer.Elapsed += _timer_Elapsed;

            _timer.Start();
            return 0;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e) {
            if (MessageQueue.Count != 0) {
                var data = MessageQueue.Dequeue();

                SendToNuibot(data);
            }
        }

        public Motor GetMotorInstance(int board, int motor) {
            return Motors[board * _nBoard + motor];
        }

        public void Update(int boardId, int motorId, double value) {
            if (IsConnected)
                Motors[boardId * 4 + motorId].position.Value = (int)value;
        }
        
        public void Update(MotorEntity motor) {
            if (IsConnected) {
                Motors[motor.NuibotBoardId * 4 + motor.NuibotMotorId].position.Value = (int)motor.Value[0];
                _buffer[motor.NuibotBoardId * 4 + motor.NuibotMotorId] = (short)motor.Value[0];

                MessageQueue.Enqueue(_buffer);
            }
        }

        public void SendToNuibot(short[] data) {
            if (!IsConnected) return;

            //short[] targets = new short[Boards.NMotor];

            //for (int i = 0; i < Motors.Count; ++i) {
            //    targets[i] = (short)Motors[i].position.Value;
            //}

            //Boards.SendPosDirect(targets);
            Boards.SendPosDirect(data);
        }
    }
}
