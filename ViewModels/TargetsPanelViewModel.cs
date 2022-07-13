using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels {
    public class TargetsPanelViewModel : BindableBase {
        private readonly SystemService _systemSvr;

        public ISelectableTarget[] ValidTargets => _systemSvr.Targets;

        private ISelectableTarget[] _selectedTargets;
        public ISelectableTarget[] SelectedTargets {
            get => _selectedTargets;
            set => SetProperty(ref _selectedTargets, value);
        }

        public TargetsPanelViewModel(SystemService systemSvr) {
            _systemSvr = systemSvr;

            PropertyChanged += TargetPanelViewModel_PropertyChanged;
        }

        private void TargetPanelViewModel_PropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName == nameof(SelectedTargets)) {
                SelectedTargets = ValidTargets.Where(e => e.IsSelected).ToArray();
            }
        }
    }
}
