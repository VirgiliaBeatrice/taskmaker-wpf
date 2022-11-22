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
using Prism.Events;
using taskmaker_wpf.Views;

namespace taskmaker_wpf.ViewModels {
    public class RegionControlUISelectionViewModel : BindableBase, INavigationAware {
        private readonly IRegionManager _regionManager;
        //private readonly ControlUiUseCase _useCase;
        private readonly ControlUiInteractorBus _uiBus;
        private readonly IMapper _mapper;
        private readonly IEventAggregator _ea;

        private ControlUiState_v1 _selectedUi;
        public ControlUiState_v1 SelectedUi {
            get { return _selectedUi; }
            set { SetProperty(ref _selectedUi, value); }
        }

        public ObservableCollection<ControlUiState_v1> UIs { get; set; } = new ObservableCollection<ControlUiState_v1>();

        public RegionControlUISelectionViewModel(
            IRegionManager regionManager,
            MapperConfiguration config,
            ControlUiInteractorBus uiBus,
            IEventAggregator ea) {
            _regionManager = regionManager;
            _uiBus = uiBus;
            _ea = ea;
            _mapper = config.CreateMapper();

            _ea.GetEvent<SystemLoadedEvent>().Subscribe(() => InvalidateUiList());

            InvalidateUiList();
        }

        private void InvalidateUiList() {
            _uiBus.Handle(new ListControlUiRequest(), (ControlUiEntity[] uis) => {
                UIs.Clear();
                UIs.AddRange(uis.Select(e => _mapper.Map<ControlUiState_v1>(e)));
            });
        }

        private void AddControlUi() {
            _uiBus.Handle(new AddControlUiRequest(), (bool res) => { });
        }

        private DelegateCommand _updateCommand;
        public DelegateCommand UpdateCommand =>
            _updateCommand ?? (_updateCommand = new DelegateCommand(ExecuteUpdateCommand));

        void ExecuteUpdateCommand() {
            _uiBus.Handle(new UpdateControlUiRequest(), (bool res) => { });
            InvalidateUiList();
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
            AddControlUi();
            InvalidateUiList();
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
