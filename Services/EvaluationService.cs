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

        public EvaluationService() {
            //motorVM;
        }

        public void Log() {
            // configure a new logger file handler for this class
            // log to file
            // log to console

            logger.Info("Event");
            logFile.AppendLine();
        }


    }

}
