using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.Collections.ObjectModel;
using taskmaker_wpf.Entity;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Services
{
    /// <summary>
    /// Evaluation => Sessions
    /// </summary>
    public partial class EvaluationViewModel : ObservableObject {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly EvaluationEntity _entity;

        public ObservableCollection<SessionViewModel> Sessions { get; private set; } = new ObservableCollection<SessionViewModel>();

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
        private readonly DateTime _date;

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
