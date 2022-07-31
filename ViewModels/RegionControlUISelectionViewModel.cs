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
using AutoMapper;

namespace taskmaker_wpf.ViewModels {
    public class RegionControlUISelectionViewModel : BindableBase, INavigationAware {
        private readonly IRegionManager _regionManager;
        private readonly ControlUiUseCase _useCase;
        private readonly IMapper _mapper;

        private ControlUiState _selectedUi;
        public ControlUiState SelectedUi {
            get { return _selectedUi; }
            set { SetProperty(ref _selectedUi, value); }
        }

        public ObservableCollection<ControlUiState> UIs { get; set; } = new ObservableCollection<ControlUiState>();

        public RegionControlUISelectionViewModel(
            IRegionManager regionManager,
            MapperConfiguration config,
            IEnumerable<IUseCase> useCases) {
            _regionManager = regionManager;
            _useCase = useCases.OfType<ControlUiUseCase>().First();
            _mapper = config.CreateMapper();

            UIs.AddRange(_useCase.GetControlUis().Select(e => _mapper.Map<ControlUiState>(e)));
        }

        private DelegateCommand _updateCommand;
        public DelegateCommand UpdateCommand =>
            _updateCommand ?? (_updateCommand = new DelegateCommand(ExecuteUpdateCommand));

        void ExecuteUpdateCommand() {
            _useCase.Update(_mapper.Map<ControlUiEntity>(SelectedUi));
            var ui = _useCase.GetControlUi(SelectedUi.Id);

            UIs.Clear();
            UIs.AddRange(_useCase.GetControlUis().Select(e => _mapper.Map<ControlUiState>(e)));
        }

        private DelegateCommand _addCmd;
        public DelegateCommand AddCmd => _addCmd ?? (_addCmd = new DelegateCommand(ExecuteAddCmd));


        private DelegateCommand _navigateToNextCommand;
        public DelegateCommand NavigateToNextCommand =>
            _navigateToNextCommand ?? (_navigateToNextCommand = new DelegateCommand(ExecuteNavigateToNextCommand));

        private void ExecuteNavigateToNextCommand() {
            if (SelectedUi != null) {
                var args = new NavigationParameters {
                    { "ui", SelectedUi }
                };

                _regionManager.RequestNavigate("ContentRegion", "RegionControlUI", args);
            }
        }

        private void ExecuteAddCmd() {
            //_systemSvr.UIs.Add(new ControlUi());
            _useCase.AddUi();

            UIs.Clear();
            UIs.AddRange(_useCase.GetControlUis().Select(e => _mapper.Map<ControlUiState>(e)));
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
