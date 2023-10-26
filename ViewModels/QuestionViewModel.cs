using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using taskmaker_wpf.Entity;

namespace taskmaker_wpf.ViewModels {
    public partial class QuestionViewModel : ObservableObject {
        [ObservableProperty]
        private string _question;
        [ObservableProperty]
        private string _description;
        [ObservableProperty]
        private double _score;
        [ObservableProperty]
        private string[] _options;

        public bool IsOpenEnded => Options == null;

        private readonly QuestionEntity _entity;
        public QuestionViewModel(QuestionEntity entity) {
            _entity = entity;

            Fetch();
        }

        [RelayCommand]
        public void Fetch() {
            // update all properties from entity
            Question = _entity.Question;
            Description = _entity.Description;
            Score = _entity.Score;
            Options = _entity.Options;
        }

        [RelayCommand]
        public void Commit() {
            // update score to entity
            _entity.Score = Score;
        }
    }

    
}