using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace taskmaker_wpf.Services {
    public static class EventDispatcher {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static List<IEvent> Events { get; set; } = new List<IEvent>();

        public static void Record(IEvent @event) {
            Events.Add(@event);
            _logger.Debug(_logger.GetType().Name + " " + @event.Tags);
        }

        public static void Save(string path) {
            // Save to csv file
            var sb = new StringBuilder();
            sb.AppendLine("EventId,Timestamp,Tags");
            foreach (var item in Events) {
                sb.AppendLine($"{item.EventId},{item.Timestamp},{item.Tags}");
            }
            sb.AppendLine(
                $"Total,{Events.Count},");
            System.IO.File.WriteAllText(path, sb.ToString());

            Reset();
        }

        public static void Reset() {
            Events.Clear();
        }
    }


    public interface IEvent {
        Guid EventId { get; }
        DateTime Timestamp { get; }
        string Tags { get; }
    }

    public record EvaluationStartedEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Tags => "Evaluation.Started";
    }

    public record EvaluationStoppedEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Tags => "Evaluation.Stopped";
    }

    public record PracticeStartedEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Tags => "Evaluation.Practice.Started";
    }

    public record PracticeStoppedEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Tags => "Evaluation.Practice.Stopped";
    }

    public record PerformStoppedEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Tags => "Evaluation.Perform.Stopped";
    }

    public record PerformStartedEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Tags => "Evaluation.Perform.Started";
    }

    public record CreationStartedEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Tags => "Evaluation.Creation.Started";
    }
    public record CreationStoppedEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Tags => "Evaluation.Creation.Stopped";
    }

    public record CreationAddEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Tags => "Evaluation.Creation.Operations.Add";
    }

    public record CreationDeleteEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Tags => "Evaluation.Creation.Operations.Delete";
    }

    public record CreationAssignEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Tags => "Evaluation.Creation.Operations.Assign";
    }

    public record CreationMoveEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Tags => "Evaluation.Creation.Operations.Move";
    }

    public record CreationTryControlEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Tags => "Evaluation.Creation.Operations.TryControl";
    }


    public record UiModeChangedEvent : IEvent {
        public Guid EventId => Guid.NewGuid();
        public DateTime Timestamp => DateTime.UtcNow;
        public string Details { get; init; } = "";
        public string Tags => "Ui.ModeChanged." + Details;
    }
}
