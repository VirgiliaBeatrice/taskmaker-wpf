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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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

        private bool _isInitialized = false;

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
                var full = m.Path;
                var dir = Path.GetDirectoryName(full);
                var filename = Path.GetFileNameWithoutExtension(full);

                string file;
                foreach (var item in entities) {
                    file = Path.Join(dir, filename + "." + item.Value.Name + ".xml");

                    BaseEntity.SaveData(item.Value, file);
                }

                file = Path.Join(dir, filename + ".csv");

                EventDispatcher.Save(file);
            });

            WeakReferenceMessenger.Default.Register<LoadMessage>(this, (r, m) => {
                var filename = m.Path;

            });

            WeakReferenceMessenger.Default.Register<MapInterpolatedMessage>(this, (r, m) => {
                var values = m.Value;
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

            WeakReferenceMessenger.Default.Register<SendToMotorMessage>(this, (r, m) => {
                var values = m.Value;
                var motors = _motorSrv.GetAll();
                for (int i = 0; i < values.Length; i++) {

                    motors[i].Value = values[i];

                    WeakReferenceMessenger.Default.Send(new MotorValueUpdatedMessage {
                        NuiBoardId = motors[i].NuiBoardId,
                        NuiMotorId = motors[i].NuiMotorId,
                        Value = motors[i].Value
                    });

                    logger.Info($"Motor {motors[i].NuiBoardId}-{motors[i].NuiMotorId} set to {motors[i].Value}");
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

        static public SolidColorBrush Pink => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f06292"));
        static public SolidColorBrush Yellow => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffee58"));
        static public SolidColorBrush Blue => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#42a5f5"));

        public void InitializeMotors() {
            if (_isInitialized) {
                WeakReferenceMessenger.Default.Send(new ShowMessageBoxMessage {
                    Message = "Motors already initialized",
                    Caption = MessageBoxImage.Information.ToString(),
                    Button = MessageBoxButton.OK,
                    Icon = MessageBoxImage.Information
                });

                return;
            }


            // Create List<MotorEntity> with 6 motors
            var motors = Enumerable.Range(0, 6)
                                   .Select(_ => _motorSrv.Create(new MotorEntity()))
                                   .ToArray();

            motors[0].NuiBoardId = 0;
            motors[0].NuiMotorId = 0;
            motors[0].Color = Yellow;
           
            motors[1].NuiBoardId = 0;
            motors[1].NuiMotorId = 1;
            motors[1].Color = Blue;
            
            motors[2].NuiBoardId = 0;
            motors[2].NuiMotorId = 2;
            motors[2].Color = Pink;
            
            motors[3].NuiBoardId = 1;
            motors[3].NuiMotorId = 0;
            motors[3].Color = Blue;

            motors[4].NuiBoardId = 1;
            motors[4].NuiMotorId = 1;
            motors[4].Color = Pink;

            motors[5].NuiBoardId = 1;
            motors[5].NuiMotorId = 2;
            motors[5].Color = Yellow;

            for (int i = 0; i < motors.Count(); i++) {
                var motor = motors[i];
                motor.Max = 10000;
                motor.Min = 0;

                _motorSrv.Update(motor);
            }

            _isInitialized = true;
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

            var survey = _surveyService.GetAll();
            var evaluation = GetAll()[0];

            foreach(var q in survey) {
                evaluation.Survey.Add(q);
            }
        }
    }
}
