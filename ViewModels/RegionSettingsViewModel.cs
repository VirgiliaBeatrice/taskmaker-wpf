using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels {
    public class RegionSettingsViewModel : BindableBase {
        private string _selectedPort = "";
        public ObservableCollection<string> Coms { get; private set; }
        
        private SerialService _serialSvr;
        private IDialogService _dialogService;

        private ICommand connectCmd;
        private ICommand listCmd;
        private ICommand selectCmd;

        public RegionSettingsViewModel(
            IDialogService dialogService,
            SerialService service) { 
            _dialogService = dialogService;
            _serialSvr = service;

            Coms = new ObservableCollection<string>();
        }

        public ICommand ConnectCmd => connectCmd?? (connectCmd = new DelegateCommand(ConnectCmdExecute));
        public ICommand ListCmd => listCmd ?? (listCmd = new DelegateCommand(ListCmdExecute));
        public ICommand SelectCmd => selectCmd ?? (selectCmd= new DelegateCommand<string>(SelectCmdExecute));

        public string SelectedPort { get => _selectedPort; set => _selectedPort = value; }

        private void SelectCmdExecute(string port) {
            SelectedPort = port;
        }

        private void ListCmdExecute() {
            var coms = _serialSvr.ListAllPorts();

            Coms.Clear();
            Coms.AddRange(coms);
        }

        private void ConnectCmdExecute() {
            if (SelectedPort != "")
                _serialSvr.Connect(SelectedPort);
            else {
                var parameters = new DialogParameters();

                parameters.Add("title", "Error");
                parameters.Add("message", "Could not find any selected port!");

                _dialogService.ShowDialog(
                    "standard",
                    parameters, 
                    (result) => { Console.WriteLine(result); });
            }
        }
    }
}
