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
            // confirm initialization of motors
            if (_motorSrv.Motors.Count == 0) {
                var msg = new ShowMessageBoxMessage {
                    Message = "No motors found. Please connect motors and restart the application.",
                    Caption = "Error",
                    Button = MessageBoxButton.OK,
                    Icon = MessageBoxImage.Error
                };

                WeakReferenceMessenger.Default.Send(msg);
            } else {
                var motors = _motorSrv.Motors;

                // Add ui and map
                var ui = _uiSrv.AddUi();
                var map = _uiSrv.AddMap();

                // Bind motors to ui
                _uiSrv.BindMotorsToUi(
                    ref map, 
                    new[] { InPlug.Create(ui) }, 
                    motors.Select(OutPlug.Create).ToArray());

                var msg = new ShowMessageBoxMessage {
                    Message = "Evaluation session initialized.",
                    Caption = "Success",
                    Button = MessageBoxButton.OK,
                    Icon = MessageBoxImage.Information
                };

                WeakReferenceMessenger.Default.Send(msg);
            }
        }


    }

}
