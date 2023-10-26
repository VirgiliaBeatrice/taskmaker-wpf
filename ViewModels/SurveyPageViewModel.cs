using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels {
    public partial class SurveyPageViewModel : ObservableObject {
        [ObservableProperty]
        private QuestionViewModel[] _questions;

        private readonly SurveyService _surveyService;
        public SurveyPageViewModel(SurveyService surveyService) {
            _surveyService = surveyService;
            Fetch();
        }

        [RelayCommand]
        public void Fetch() {
            Questions = _surveyService.GetAll()
                .Select(e => new QuestionViewModel(e))
                .ToArray();
        }

        [RelayCommand]
        public void Commit() {
            foreach(var q in Questions) {
                q.Commit();
            }
        }
    }
}
