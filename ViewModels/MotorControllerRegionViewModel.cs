using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace taskmaker_wpf.ViewModels {
    public class MotorControllerRegionViewModel : BindableBase {

        private ObservableCollection<object> _motors;
        public ObservableCollection<object> Motors { get; private set; }
    }
}
