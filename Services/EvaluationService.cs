using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic;
using NLog;
using NLog.Fluent;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using taskmaker_wpf.Entity;
using taskmaker_wpf.ViewModels;
using taskmaker_wpf.Views;

namespace taskmaker_wpf.Services
{
    public partial class EvaluationService : ObservableObject {
        public Logger logger = LogManager.GetCurrentClassLogger();
        public string EvaluationName { get; set; } = "Default";
        public string ParticipantName { get; set; } = "";
        public int Phase { get; set; } = 0;
        
        public StringBuilder logFile = new StringBuilder();

        private DispatcherTimer timer;
        private readonly MotorService _motorSrv;
        private readonly UIService _uiSrv;

        [ObservableProperty]
        private TimeSpan _time;
        private DateTime _startTime;

        public EvaluationService(MotorService motorSrv, UIService uiSrv) {
            _motorSrv = motorSrv;
            _uiSrv = uiSrv;

            timer = new DispatcherTimer();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (_, _) => {
                Time = DateTime.Now - _startTime;
            };

        }

        public void Start() {
            if (!timer.IsEnabled) {
                _startTime = DateTime.Now;
                timer.Start();
            }
        }

        public void Pause() {
            timer.Stop();
        }

        public void Stop() {
            _startTime = DateTime.MinValue;
            timer.Stop();
        }

        public void Log() {
            // configure a new logger file handler for this class
            // log to file
            // log to console

            logger.Info("Event");
            logFile.AppendLine();
        }

        public void Initialize() {
            // Create List<MotorEntity> with 6 motors
            var motors = Enumerable.Range(0, 6)
                                   .Select(_ => _motorSrv.Create(new MotorEntity()))
                                   .ToArray();

            motors[0].NuiBoardId = 0;
            motors[0].NuiMotorId = 0;
            motors[1].NuiBoardId = 0;
            motors[1].NuiMotorId = 1;
            motors[2].NuiBoardId = 0;
            motors[2].NuiMotorId = 2;
            motors[3].NuiBoardId = 1;
            motors[3].NuiMotorId = 0;
            motors[4].NuiBoardId = 1;
            motors[4].NuiMotorId = 1;
            motors[5].NuiBoardId = 1;
            motors[5].NuiMotorId = 2;

            for (int i = 0; i < motors.Count(); i++) {
                var motor = motors[i];
                motor.Max = 10000;
                motor.Min = -10000;

                _motorSrv.Update(motor);
            }
        }
    }
}
