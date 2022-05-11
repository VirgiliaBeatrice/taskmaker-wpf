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
        private ICommand testCmd;
        public ICommand TestCmd => testCmd ?? (testCmd = new DelegateCommand<MouseEventArgs>(TestCmdExecute));

        public Subject<int> Count { get; set; }
        private void TestCmdExecute(MouseEventArgs obj) {
            Count.OnNext(1);
            //throw new NotImplementedException();
        }


        private void UpdateCommandExecute() {
            //Motor.
        }

        private readonly IRegionManager _regionManager;

        public TestWindowViewModel(IRegionManager regionManager) {
            _regionManager = regionManager;
            _regionManager.RegisterViewWithRegion("ContentRegion", typeof(Views.RegionHome));

            Count = new Subject<int>();
            Count.Subscribe(
                (e) => { Console.WriteLine(e); },
                (e) => { Console.WriteLine(e); },
                () => { Console.WriteLine("end"); });
            
        }

        public void TestCommandExecute() {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void NavigateCommandExecute(string navigatePath) {
            if (navigatePath != null)
                _regionManager.RequestNavigate("ContentRegion", navigatePath);
        }
    }
}
