using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels {
    public class RegionMotorViewModel : BindableBase, INavigationAware {
        private ICommand removeMotorCmd;
        public ICommand RemoveMotorCmd => removeMotorCmd ?? (removeMotorCmd = new DelegateCommand<Motor>(RemoveMotorCmdExecute));

        private void RemoveMotorCmdExecute(Motor obj) {
            _motorService.Motors.Remove(obj);
            Motors.Remove(obj);
        }

        private ICommand addMotorCmd;
        public ICommand AddMotorCmd => addMotorCmd ?? (addMotorCmd = new DelegateCommand(AddMotorCmdExecute));

        private void AddMotorCmdExecute() {
            var newMotor = new Motor();
            _motorService.Motors.Add(newMotor);
            Motors.Add(newMotor);
        }

        private ICommand setCmd;
        public ICommand SetCmd => setCmd ?? (setCmd = new DelegateCommand<string>(SetCmdExecute));

        private void SetCmdExecute(string text) {
            Console.WriteLine(text);
        }

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
