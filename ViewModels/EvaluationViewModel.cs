using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using NLog;
using System;
using System.Collections.ObjectModel;
using taskmaker_wpf.Entity;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Services {

    public partial class SurveyPageViewModel : ObservableObject {
        [ObservableProperty]
        private QuestionViewModel[] _questions;

        public SurveyPageViewModel() {
            Questions = new QuestionViewModel[] {
                new QuestionViewModel {
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
                new QuestionViewModel {
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
                new QuestionViewModel {
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
                new QuestionViewModel {
                    Question = "How did you plan to design the controller?",
                    Description = "Open-ended",
                },
                new QuestionViewModel {
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
                new QuestionViewModel {
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
        }
    }
    public class EvaluationSelectedSessionChangedMessage : ValueChangedMessage<SessionViewModel> {
        public EvaluationSelectedSessionChangedMessage(SessionViewModel value) : base(value) { }
    }
    /// <summary>
    /// Evaluation => Sessions
    /// </summary>
    public partial class EvaluationViewModel : ObservableObject {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly EvaluationEntity _entity;

        public ObservableCollection<SessionViewModel> Sessions { get; private set; } = new ObservableCollection<SessionViewModel>();

        [ObservableProperty]
        private SessionViewModel _selectedSession;

        public EvaluationViewModel(EvaluationEntity entity) {
            _entity = entity;

            Fetch();
        }

        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _participant;
        [ObservableProperty]
        private TrialType _trialType;
        [ObservableProperty]
        private AgeGeneration _age;
        [ObservableProperty]
        private Gender _gender;
        [ObservableProperty]
        private LevelOfKnowledge _knowledge;

        [ObservableProperty]
        private DateTime _date;

        [RelayCommand]
        public void Update() {
            // Update all properties to entity
            _entity.Id = Id;
            _entity.Name = Name;
            _entity.Participant = Participant;
            _entity.TrialType = TrialType;
            _entity.Age = Age;
            _entity.Gender = Gender;
            _entity.Knowledge = Knowledge;
        }

        [RelayCommand]
        public void Fetch() {
            // Update all properties from entity
            // Update basic properties
            Id = _entity.Id;
            Name = _entity.Name;
            Participant = _entity.Participant;
            TrialType = _entity.TrialType;
            Age = _entity.Age;
            Gender = _entity.Gender;
            Knowledge = _entity.Knowledge;
            Date = _entity.Date;

            // Update Sessions
            Sessions.Clear();
            foreach (var entity in _entity.Sessions) {
                Sessions.Add(new SessionViewModel(entity));
            }
        }

        public override string ToString() {
            return Name;
        }

    }
}
