using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Models;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels {
    public class StatefulTarget : BindableBase {
        private ITarget _target;

        private bool _isSelected = false;
        public bool IsSelected {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private string _name;
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public StatefulTarget(ITarget target) {
            _target = target;

            Name = target.Name;
        }

        public ITarget GetTarget() => _target;

    }

    public class TargetsPanelViewModel : BindableBase {
        private readonly SystemService _systemSvr;

        private StatefulTarget[] _validTargets;
        public StatefulTarget[] ValidTargets {
            get => _validTargets;
            set => SetProperty(ref _validTargets, value);
        }

        private ITarget[] _uiTargets;
        public ITarget[] UiTargets {
            get => _uiTargets;
            private set {
                SetProperty(ref _uiTargets, value);

                UI.SetTargets(_uiTargets);
            }
        }

        private ControlUI _ui;
        public ControlUI UI {
            get => _ui;
            set {
                SetProperty(ref _ui, value);

                UiTargets = UI.Complex.Targets.ToArray();
            }
        }

        private DelegateCommand _updateCommand;
        public DelegateCommand UpdateCommand =>
            _updateCommand ?? (_updateCommand = new DelegateCommand(ExecuteUpdateCommand));

        void ExecuteUpdateCommand() {
            UiTargets = _validTargets
                .Where(e => e.IsSelected)
                .Select(e => e.GetTarget())
                .ToArray();
        }

        public TargetsPanelViewModel(SystemService systemSvr) {
            _systemSvr = systemSvr;

            _validTargets = _systemSvr.Targets
                .Select(e => new StatefulTarget(e))
                .ToArray();
        }
    }
}
