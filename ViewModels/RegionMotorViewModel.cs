using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels {
    public class RegionMotorViewModel : BindableBase, INavigationAware {

        public ObservableCollection<Motor> Motors { get; private set; }

        private IRegionManager _regionManager;
        private MotorService _motorService;
        public RegionMotorViewModel(
            IRegionManager regionManager,
            MotorService motorService) {
            _regionManager = regionManager;
            _motorService = motorService;

            Motors = new ObservableCollection<Motor>();
            Motors.AddRange(_motorService.Motors);
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            //throw new NotImplementedException();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            //throw new NotImplementedException();
        }
    }
}
