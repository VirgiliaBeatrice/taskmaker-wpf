using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels {
    public partial class RegionSliderViewModel : MotorCollectionViewModel {
        [ObservableProperty]
        private bool _isSerialConnected;
        [ObservableProperty]
        private string _title = "Motor Control";

        partial void OnIsSerialConnectedChanged(bool value) {
            if (value) {
                Title = "Motor Control*";
            }
            else {
                Title = "Motor Control";
            }
        }

        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly SerialService _serialService;

        public RegionSliderViewModel(MotorService motorService, SerialService serialService) : base(motorService) {
            _serialService = serialService;

            IsSerialConnected = _serialService.IsConnected;
        }

        [RelayCommand]
        public void FetchThis() {
            IsSerialConnected = _serialService.IsConnected;

            Fetch();
        }
    }
}