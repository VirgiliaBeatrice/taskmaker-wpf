﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Model.Data;
using PCController;
using mMotor = taskmaker_wpf.Model.Data.Motor;
using cMotor = PCController.Motor;
using taskmaker_wpf.Model.Core;

namespace taskmaker_wpf.Services {
    public class TargetService {
        public List<IValue> Targets { get; set; } = new List<IValue> { };

        public TargetService() {
            Enumerable.Range(0, 3)
                .ToList()
                .ForEach(
                    e => {
                        var motor = new mMotor() {
                            Max = 10000,
                            Min = -10000,
                            Value = 0,
                            Alias = "S0"
                        };

                        Targets.Add(motor);
                    });
        }
    }

    public class SystemService {
        public List<UI> UIs { get; set; } = new List<UI>();

        public SystemService() {
            UIs.Add(new UI { Name = "Example1" });
        }
    }

    public class MotorService {
        public List<mMotor> Motors { get; set; } = new List<mMotor> { };

        public MotorService() {
        }
    }

    public class SerialService {
        public List<string> Ports { get; set; } = new List<string> { };
        public Motors Motors { get; set; }
        public Boards Boards { get; set; }


        public bool IsConnected => _serial != null;
        private SerialPort _serial;


        private int _nBoard;
        private int _nMotor;

        public SerialService() {
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

            _nMotor = Boards.NMotor;
            _nBoard = Boards.Count;

            Motors.AddRange(Enumerable.Repeat(new cMotor(), _nMotor));

            Boards.Clear();
            Motors.Clear();
            Boards.EnumerateBoard();

            return 0;
        }

        public cMotor GetMotorInstance(int board, int motor) {
            return Motors[board * _nBoard + motor];
        }
    }
}
