using CommunityToolkit.Mvvm.ComponentModel;

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
    }
}