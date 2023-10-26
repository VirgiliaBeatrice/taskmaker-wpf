using taskmaker_wpf.Entity;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Services
{
    public class SurveyService : BaseEntityManager<QuestionEntity> {
        public SurveyService() {
            var tempEntities = new QuestionEntity[] {

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

            foreach (var item in tempEntities)
            {
                //entities.Add(item.Id, item);
            }
        }
    }
}
