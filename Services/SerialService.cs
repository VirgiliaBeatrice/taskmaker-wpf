using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using PCController;
using taskmaker_wpf.Entity;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Services {
    public class CircularQueue<T> {
        private T[] queue;
        private int head = -1; // position of the first item
        private int tail = -1; // position of the last item
        private int count = 0;

        public int Capacity { get; init; } = 200;
        public bool IsEmpty => count == 0;
        public bool IsFull {
            get {
                if (count != 0)
                    return count == Capacity;
                else
                    return false;
            }}

        public CircularQueue(int capacity) {
            Capacity = capacity;
            queue = new T[capacity];
        }

        public void Enqueue(T item) {
            if (IsFull) {
                Dequeue();  // remove oldest to make space
            }

            if (IsEmpty) {
                head = 0;
            }

            tail = (tail + 1) % Capacity;
            queue[tail] = item;
            count++;
        }

        public T Dequeue() {
            if (IsEmpty) {
                throw new InvalidOperationException();
            }

            T item = queue[head];
            queue[head] = default;  // Clear the slot (optional)
            head = (head + 1) % Capacity;
            count--;

            if (IsEmpty) {
                head = -1;
                tail = -1;
            }

            return item;
        }

        public T Peek() {
            if (IsEmpty) {
                throw new InvalidOperationException("Queue is empty");
            }

            return queue[head];
        }
    }


    public class SerialService {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        public List<string> Ports { get; set; } = new List<string> { };
        public Motors Motors { get; set; }
        public Boards Boards { get; set; }

        public int Max { get; set; } = 8000;
        public int Min { get; set; } = -8000;

        public CircularQueue<short[]> MessageQueue { get; set; } = new CircularQueue<short[]>(2000);
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

                if (serial.IsConnected && !double.IsNaN(value)) {
                    serial.Motors[bId * 4 + mId].position.Value = (int)value;
                    serial._buffer[bId * 4 + mId] = (short)value;

                    _logger.Debug("Board: {0}, Motor: {1}, Value: {2}", bId, mId, value);
                    serial.MessageQueue.Enqueue(serial._buffer);
                }
                
                if (double.IsNaN(value)) {
                    _logger.Debug("Found NaN, skip.");
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
            if (!MessageQueue.IsEmpty) {
                var data = MessageQueue.Dequeue();

                if (data == null) {
                    // warn
                    _logger.Warn("Data is null");

                    return;
                }

                SendToNuibot(data);
            }
        }

        public void SendToNuibot(short[] data) {
            if (!IsConnected) return;

            Boards.SendPosDirect(data);
        }
    }
}
