using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace taskmaker_wpf.Entity
{
    [Serializable]
    public class QuestionEntity : BaseEntity {
        private string _question;
        private string _description;
        private double _score;
        private string[] _options;

        public string Question { get => _question; set => _question = value; }
        public string Description { get => _description; set => _description = value; }
        public double Score { get => _score; set => _score = value; }
        public string[] Options { get => _options; set => _options = value; }
    }
}
