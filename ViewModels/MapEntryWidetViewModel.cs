using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.VisualBasic;
using NLog;
using System.Threading.Tasks;
using taskmaker_wpf.Views;
using taskmaker_wpf.Views.Widget;

namespace taskmaker_wpf.ViewModels {
    public partial class MapEntryWidetViewModel : ObservableObject {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly SessionViewModel _session;

        [ObservableProperty]
        private string _formattedIds;
        [ObservableProperty]
        private double[] _value;
        [ObservableProperty]
        private double[] _outputValue;

        public MapEntryWidetViewModel(SessionViewModel session) {
            _session = session;

            Fetch();
        }

        public void Fetch() {
            if (_session.SelectedNodeStates != null) {
                var ids = _session.SelectedNodeIds;
                var indices = _session.SelectedNodeIndices;
                var value = _session.Map.GetValue(indices);
                var output = _session.Map.Output;

                FormattedIds = string.Join(",", ids);
                Value = value; 
                OutputValue = output;

                // motor output
                WeakReferenceMessenger.Default.Send(new SendToMotorMessage {
                    Sender = this,
                    Value = value
                });
            }
        }

        [RelayCommand]
        public async Task GetValueAsync() {
            var result = await WeakReferenceMessenger.Default.Send(new DialogRequestMessage());

            if (result.Result == System.Windows.MessageBoxResult.OK) {
                Value = result.Value as double[];

                _session.Map.SetValue(_session.SelectedNodeIndices, Value);
            }
        }

        [RelayCommand]
        public void Clear() {
            var indices = _session.SelectedNodeIndices;

            _session.Map.ClearValue(indices);

            Fetch();
        }

        [RelayCommand]
        public void Close() {
            _session.ShowWidget = false;
        }
    }

    public class SendToMotorMessage {
        public object Sender { get; init; }
        public double[] Value { get; init; }
    }
}