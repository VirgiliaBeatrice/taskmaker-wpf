using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Timers;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using PCController;
using taskmaker_wpf.Entity;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Services {
    public class SerialService {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        public List<string> Ports { get; set; } = new List<string> { };
        public Motors Motors { get; set; }
        public Boards Boards { get; set; }

        public int Max { get; set; } = 8000;
        public int Min { get; set; } = -8000;

        public Queue<short[]> MessageQueue { get; set; } = new Queue<short[]>();
        public bool IsConnected => _serial != null;
        private SerialPort _serial;


        private int _nBoard;
        private int _nMotor;
        private short[] _buffer;
        private Timer _timer;

        public SerialService() {

            WeakReferenceMessenger.Default.Register<MotorValueUpdatedMessage>(this, (r, m) => {
                var bId = m.NuiBoardId;
                var mId = m.NuiMotorId;
                var value = m.Value;
                var serial = r as SerialService;

                if (serial.IsConnected) {
                    serial.Motors[bId * 4 + mId].position.Value = (int)value;
                    serial._buffer[bId * 4 + mId] = (short)value;

                    serial.MessageQueue.Enqueue(serial._buffer);
                }
                
            });
        }

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
        
        public void Update(MotorState state) {
            if (IsConnected) {

                var value = state.Value;

                if (value > Max) {
                    value = Max;

                    // Invoke clamp event
                    _logger.Info("Clamped caused by safety limit (max)");

                }
                else if (value < Min) {
                    value = Min;

                    // Invoke clamp event
                    _logger.Info("Clamped caused by safety limit (min)");
                }

                Motors[state.NuiBoardId * 4 + state.NuiMotorId].position.Value = (int)value;
                _buffer[state.NuiBoardId * 4 + state.NuiMotorId] = (short)value;

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
