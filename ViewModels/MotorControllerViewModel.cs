using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace taskmaker_wpf.ViewModels {
    public class MotorControllerViewModel : BindableBase {
        private int _minimum;
        public int Minimum {
            get { return _minimum; }
            set { 
                SetProperty(ref _minimum, value);
            }
        }

        private int _maximum;
        public int Maximum {
            get { return _maximum; }
            set { SetProperty(ref _maximum, value); }
        }

        private int _value;
        public int Value { 
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
    }
}
