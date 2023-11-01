using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels
{
    public class MotorDialogViewModel : MotorCollectionViewModel {
        public MotorDialogViewModel(MotorService motorService) : base(motorService) {
        
        }
    }
}
