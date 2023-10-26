using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic;
using NLog;
using NLog.Fluent;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using taskmaker_wpf.Entity;
using taskmaker_wpf.ViewModels;
using taskmaker_wpf.Views;
using WPFLocalizeExtension.Engine;

namespace taskmaker_wpf.Services {


    public class EvaluationService : BaseEntityManager<EvaluationEntity> {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        public StringBuilder logFile = new StringBuilder();

        private DispatcherTimer timer;
        private readonly MotorService _motorSrv;
        private readonly UIService _uiSrv;
        private readonly MapService _mapSrv;
        private readonly SurveyService _surveyService;
        private readonly SessionService _sessionSrv;

        private TimeSpan _time;
        private DateTime _startTime;

        public EvaluationService(MotorService motorSrv, UIService uiSrv, MapService mapSrv, SessionService sessionSrv, SurveyService surveyService) {
            _motorSrv = motorSrv;
            _uiSrv = uiSrv;
            _mapSrv = mapSrv;
            _sessionSrv = sessionSrv;

            timer = new DispatcherTimer();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (_, _) => {
                _time = DateTime.Now - _startTime;
            };

            WeakReferenceMessenger.Default.Register<SaveMessage>(this, (r, m) => {
                var filename = m.Path;

                var idx = 0;

                foreach (var item in entities) {
                    BaseEntity.SaveData(item.Value, filename);
                }
            });

            WeakReferenceMessenger.Default.Register<LoadMessage>(this, (r, m) => {
                var filename = m.Path;

            });

            WeakReferenceMessenger.Default.Register<MapOutputMessage>(this, (r, m) => {
                var values = m.Output;
                var motors = _motorSrv.GetAll();
                for (int i = 0; i < values.Length; i++) {

                    motors[i].Value = values[i];

                    WeakReferenceMessenger.Default.Send(new MotorValueUpdatedMessage {
                        NuiBoardId = motors[i].NuiBoardId,
                        NuiMotorId = motors[i].NuiMotorId,
                        Value = motors[i].Value
                    });
                }
            });
            _surveyService = surveyService;
        }

        public override EvaluationEntity Create(EvaluationEntity entity) {
            var ui = new ControlUiEntity();
            _uiSrv.Create(ui);

            var map = new NLinearMapEntity(2, 6);
            _mapSrv.Create(map);

            var session = new SessionEntity() {
                Map = map,
                Uis = new List<ControlUiEntity>() { ui },
            };
            _sessionSrv.Create(session);

            entity.Sessions.Add(session);
            
            return base.Create(entity);
        }

        public void Start() {
            //if (!timer.IsEnabled) {
            //    _startTime = DateTime.Now;
            //    timer.Start();
            //}
        }

        public void Pause() {
            timer.Stop();
        }

        public void Stop() {
            _startTime = DateTime.MinValue;
            timer.Stop();
        }

        public void InitializeMotors() {
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
                motor.Min = 0;

                _motorSrv.Update(motor);
            }

        }

        public void InitializeSurvey() {
            var questions = new QuestionEntity[] {

            new QuestionEntity {
                Question = "How confident are you in designing the controller for the given task?",
                Description = "",
                Score = 5.0d,
                Options = new[] {
                        "Not confident at all",
                        "Not confident",
                        "Neutral",
                        "Confident",
                        "Very confident"
                    }
            },
                new QuestionEntity {
                    Question = "How confident are you in using the UI to control the robot to complete the task?",
                    Description = "",
                    Score = 5.0d,
                    Options = new[] {
                        "Not confident at all",
                        "Not confident",
                        "Neutral",
                        "Confident",
                        "Very confident"
                    }
                },
                new QuestionEntity {
                    Question = "Did the robot's behavior match your expectations when you used the controller?",
                    Description = "",
                    Score = 5.0d,
                    Options = new[] {
                        "Did not match at all",
                        "Did not match",
                        "Neutral",
                        "Matched",
                        "Matched perfectly"
                    }
                },
                new QuestionEntity {
                    Question = "How did you plan to design the controller?",
                    Description = "Open-ended",
                },
                new QuestionEntity {
                    Question = "How would you rate your initial experience learning to use the system?",
                    Description = "",
                    Score = 5.0d,
                    Options = new[] {
                        "Very difficult",
                        "Difficult",
                        "Neutral",
                        "Easy",
                        "Very easy",
                    }
                },
                new QuestionEntity {
                    Question = "How would you rate the balance between performance and effort?",
                    Description = "Considering the entire evaluation, including both the learning process and the effort to design the controllers for the tasks.",
                    Score = 5.0d,
                    Options = new[] {
                        "Very difficult",
                        "Difficult",
                        "Neutral",
                        "Easy",
                        "Very easy",
                    }
                },
            };

            foreach(var item in questions) {
                _surveyService.Create(item);
            }
        }
    }
}
