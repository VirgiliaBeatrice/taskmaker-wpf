using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Domain;
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

    public class TargetState : BindableBase {
        private object _target;

        public object Target {
            get => _target;
            set => SetProperty(ref _target, value);
        }

        private bool _isSelected;

        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    public class TargetsPanelViewModel : BindableBase {
        private readonly ListTargetUseCase _useCase;
        private readonly NLinearMapUseCase _mapUseCase;

        private TargetState[] _validTargets;
        public TargetState[] ValidTargets {
            get => _validTargets;
            set => SetProperty(ref _validTargets, value);
        }

        private ITarget[] _uiTargets;
        public ITarget[] UiTargets {
            get => _uiTargets;
            private set {
                SetProperty(ref _uiTargets, value);

                //UI.SetTargets(_uiTargets);
            }
        }

        private ObservableCollection<NLinearMapEntity> _maps = new ObservableCollection<NLinearMapEntity>();
        public ObservableCollection<NLinearMapEntity> Maps {
            get => _maps;
            set => SetProperty(ref _maps, value);
        }

        private NLinearMapEntity _selectedMap;
        public NLinearMapEntity SelectedMap {
            get => _selectedMap;
            set => SetProperty(ref _selectedMap, value);
        }

        private DelegateCommand _updateCommand;
        public DelegateCommand UpdateCommand =>
            _updateCommand ?? (_updateCommand = new DelegateCommand(ExecuteUpdateCommand));

        private DelegateCommand _addCommand;
        public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(ExecuteAddCommand));

        private void ExecuteAddCommand() {
            var map = _mapUseCase.AddMap();

            Maps.Add(map);
        }

        void ExecuteUpdateCommand() {
            SelectedMap.Targets = ValidTargets.Where(e => e.IsSelected).ToArray();
        }

        public TargetsPanelViewModel(IEnumerable<IUseCase> useCases) {
            _useCase = useCases.OfType<ListTargetUseCase>().First();
            _mapUseCase = useCases.OfType<NLinearMapUseCase>().First();

            ValidTargets = _useCase.GetTargets().Select(e => new TargetState() { Target = e }).ToArray();
            Maps.AddRange(_mapUseCase.GetMaps());
        }
    }
}
