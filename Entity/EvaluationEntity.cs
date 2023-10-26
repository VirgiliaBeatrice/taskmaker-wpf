using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml.Serialization;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.Entity
{

    public class EnumToDescriptionConverter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Enum) {
                FieldInfo fi = value.GetType().GetField(value.ToString());

                DescriptionAttribute[] attributes =
                    (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes != null && attributes.Length > 0)
                    return attributes[0].Description;
                else
                    return value.ToString();
            }
            else
                return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public enum AgeGeneration {
        [Description("0-9 years")]
        Child = 0,

        [Description("10-19 years")]
        Teenager = 1,

        [Description("20-29 years")]
        Twenties = 2,

        [Description("30-39 years")]
        Thirties = 3,

        [Description("40-49 years")]
        Forties = 4,

        [Description("50+ years")]
        Fifties = 5,
        None
    }

    public enum Gender {
        Male,
        Female,
        NonBinary,
        PreferNotToSay,
        None
    }

    public enum TrialType {
        Tutorial,
        Task,
        None
    }

    public enum LevelOfKnowledge {
        Beginner,
        Intermediate,
        Advanced,
        None
    }
    
    [Serializable]
    public class EvaluationEntity : BaseEntity
    {
        private string _participant = "Participant";
        private TrialType _trialType = TrialType.None;
        private AgeGeneration _age = AgeGeneration.None;
        private Gender _gender = Gender.None;
        private LevelOfKnowledge _knowledge = LevelOfKnowledge.None;
        private DateTime _date;
        private List<SessionEntity> _sessions = new List<SessionEntity>();
        private List<IEvent> _events = new List<IEvent>();

        public string Participant { get => _participant; set => _participant = value; }
        public TrialType TrialType { get => _trialType; set => _trialType = value; }
        public AgeGeneration Age { get => _age; set => _age = value; }
        public Gender Gender { get => _gender; set => _gender = value; }
        public LevelOfKnowledge Knowledge { get => _knowledge; set => _knowledge = value; }
        public DateTime Date { get => _date; init => _date = value; }
        public List<SessionEntity> Sessions { get => _sessions; set => _sessions = value; }
        public List<IEvent> Events { get => _events; set => _events = value; }

        public EvaluationEntity() {
            Date = DateTime.UtcNow;
        }

        public void Tag(IEvent @event) {
            Events.Add(@event);
        }
    }
}
