using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace taskmaker_wpf.ViewModels {
    public class MessageBoxViewModel : BindableBase, IDialogAware {
        private ICommand _closeDialogCmd;
        public ICommand CloseDialogCmd => _closeDialogCmd ?? (_closeDialogCmd = new DelegateCommand(CloseDialogExecute));
        public string Title { get; set; } = "MessageBox";

        public string Message { get => _message; set => _message = value; }

        private string _message;

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() {
        }

        public void CloseDialogExecute() {
            var result = ButtonResult.OK;

            RequestClose?.Invoke(new DialogResult(result));
        }

        public void OnDialogOpened(IDialogParameters parameters) {
            Title = parameters.GetValue<string>("title");
            Message = parameters.GetValue<string>("message");
        }
    }
}
