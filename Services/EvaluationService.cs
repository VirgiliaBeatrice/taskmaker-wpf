using NLog;
using NLog.Fluent;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Services
{
    public class EvaluationService {
        public Logger logger = LogManager.GetCurrentClassLogger();
        public string EvaluationName { get; set; } = "Default";
        public string ParticipantName { get; set; } = "";
        public int Phase { get; set; } = 0;
        
        public StringBuilder logFile = new StringBuilder();


        private readonly MotorService _motorSrv;
        private readonly UIService _uiSrv;

        public EvaluationService(MotorService motorSrv, UIService uiSrv) {
            _motorSrv = motorSrv;
            _uiSrv = uiSrv;
        }

        public void Log() {
            // configure a new logger file handler for this class
            // log to file
            // log to console

            logger.Info("Event");
            logFile.AppendLine();
        }

        public void Initialize() {
            //// Add motors
            //_motorSrv.Initialize();
            var motors = _motorSrv.Motors;

            // Add a Ui
            var ui = _uiSrv.AddUi();
            var map = _uiSrv.AddMap();

            // Bind motors to ui
            _uiSrv.BindMotorsToUi(
                ref map, 
                new[] { InPlug.Create(ui) }, 
                motors.Select(OutPlug.Create).ToArray());
            
        }


    }

}
