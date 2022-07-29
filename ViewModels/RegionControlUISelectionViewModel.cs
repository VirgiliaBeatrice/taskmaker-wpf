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
using taskmaker_wpf.Domain;

namespace taskmaker_wpf.ViewModels {
    public class RegionControlUISelectionViewModel : BindableBase, INavigationAware {
        private readonly SystemService _systemSvr;
        private readonly IRegionManager _regionManager;
        private readonly ControlUiUseCase _useCase;
        public ObservableCollection<ControlUiEntity> UIs { get; set; } = new ObservableCollection<ControlUiEntity>();

        public RegionControlUISelectionViewModel(
            IRegionManager regionManager,
            IEnumerable<IUseCase> useCases,
            SystemService systemSvr) {
            _systemSvr = systemSvr;
            _regionManager = regionManager;
            _useCase = useCases.OfType<ControlUiUseCase>().First();

            UIs.AddRange(_useCase.GetControlUis());
        }

        private DelegateCommand _addCmd;
        public DelegateCommand AddCmd => _addCmd ?? (_addCmd = new DelegateCommand(ExecuteAddCmd));


        private DelegateCommand<ControlUiEntity> _navigateToNextCommand;
        public DelegateCommand<ControlUiEntity> NavigateToNextCommand =>
            _navigateToNextCommand ?? (_navigateToNextCommand = new DelegateCommand<ControlUiEntity>(ExecuteNavigateToNextCommand));

        private void ExecuteNavigateToNextCommand(ControlUiEntity ui) {
            if (ui != null) {
                var args = new NavigationParameters {
                    { "ui", ui }
                };

                _regionManager.RequestNavigate("ContentRegion", "RegionControlUI", args);
            }
        }

        private void ExecuteAddCmd() {
            //_systemSvr.UIs.Add(new ControlUi());
            _useCase.AddUi();

            UIs.Clear();
            UIs.AddRange(_useCase.GetControlUis());
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
