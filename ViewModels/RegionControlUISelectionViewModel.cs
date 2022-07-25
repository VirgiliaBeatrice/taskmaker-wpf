using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using taskmaker_wpf.Services;
using taskmaker_wpf.Models;

namespace taskmaker_wpf.ViewModels {
    public class RegionControlUISelectionViewModel : BindableBase, INavigationAware {
        private readonly SystemService _systemSvr;
        private readonly IRegionManager _regionManager;

        public ObservableCollection<ControlUi> UIs => _systemSvr.UIs;

        public RegionControlUISelectionViewModel(
            IRegionManager regionManager,
            SystemService systemSvr) {
            _systemSvr = systemSvr;
            _regionManager = regionManager;
        }

        private DelegateCommand _addCmd;
        public DelegateCommand AddCmd => _addCmd ?? (_addCmd = new DelegateCommand(ExecuteAddCmd));


        private DelegateCommand<ControlUi> _navigateToNextCommand;
        public DelegateCommand<ControlUi> NavigateToNextCommand =>
            _navigateToNextCommand ?? (_navigateToNextCommand = new DelegateCommand<ControlUi>(ExecuteNavigateToNextCommand));

        private void ExecuteNavigateToNextCommand(ControlUi parameter) {
            if (parameter != null) {
                var args = new NavigationParameters {
                    { "ui", parameter }
                };

                _regionManager.RequestNavigate("ContentRegion", "RegionControlUI", args);
            }
        }

        private void ExecuteAddCmd() {
            _systemSvr.UIs.Add(new ControlUi());
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
