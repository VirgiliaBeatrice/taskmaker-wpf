using Prism.Mvvm;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using taskmaker_wpf.Services;
using taskmaker_wpf.Models;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace taskmaker_wpf.ViewModels {
    public class TestWindowViewModel : BindableBase {
        private ICommand testCommand;
        public ICommand TestCommand => testCommand ?? (testCommand = new DelegateCommand(TestCommandExecute));

        private ICommand navigateCommand;
        public ICommand NavigateCommand => navigateCommand ?? (navigateCommand = new DelegateCommand<string>(NavigateCommandExecute));

        private ICommand updateCommand;
        public ICommand UpdateCommand => updateCommand ?? (updateCommand = new DelegateCommand(UpdateCommandExecute));


        private void UpdateCommandExecute() {
            //Motor.
        }

        private readonly IRegionManager _regionManager;

        public TestWindowViewModel(IRegionManager regionManager) {
            _regionManager = regionManager;
            _regionManager.RegisterViewWithRegion("NavigationRegion",
                typeof(Views.NavigationView));
            _regionManager.RegisterViewWithRegion("ContentRegion", typeof(Views.RegionHome));
        }

        public void TestCommandExecute() {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void NavigateCommandExecute(string navigatePath) {
            if (navigatePath != null) {
                if (navigatePath == "RegionControlUI") {
                    _regionManager.RequestNavigate("ContentRegion", nameof(Views.RegionControlUISelection));
                }
                else {
                    _regionManager.RequestNavigate("ContentRegion", navigatePath);
                }
            }
        }
    }
}
