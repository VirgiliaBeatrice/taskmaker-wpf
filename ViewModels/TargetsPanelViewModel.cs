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
using AutoMapper;

namespace taskmaker_wpf.ViewModels {
    public class TargetState : BindableBase {
        protected object _target;

        public object Target {
            get => _target;
            set => SetProperty(ref _target, value);
        }

        protected bool _isSelected;

        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }


        public string Name => ((BaseEntity)_target).Name;
        public override string ToString() {
            return ((BaseEntity)_target).ToString();
        }
    }

    public interface ISelectableState {
        bool IsSelected { get; set; }
    }

    public class MotorTargetState : MotorState, ISelectableState {
        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public override string ToString() {
            return Name;
        }
    }

    public class ControlUiTargetState : ControlUiState, ISelectableState {
        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public override string ToString() {
            return Name;
        }
    }

    public class NLinearMapState : BindableBase {
        private string _name;
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private object[] _targets;
        public object[] Targets {
            get => _targets;
            set => SetProperty(ref _targets, value);
        }

    }

    public class TargetsPanelViewModel : BindableBase {
        private readonly ListTargetUseCase _useCase;
        private readonly NLinearMapUseCase _mapUseCase;

        private ISelectableState[] _validTargets;
        public ISelectableState[] ValidTargets {
            get => _validTargets;
            set => SetProperty(ref _validTargets, value);
        }

        private ObservableCollection<NLinearMapState> _maps = new ObservableCollection<NLinearMapState>();
        public ObservableCollection<NLinearMapState> Maps {
            get => _maps;
            set => SetProperty(ref _maps, value);
        }

        private NLinearMapState _selectedMap;
        public NLinearMapState SelectedMap {
            get => _selectedMap;
            set => SetProperty(ref _selectedMap, value);
        }

        private DelegateCommand _updateCommand;
        public DelegateCommand UpdateCommand =>
            _updateCommand ?? (_updateCommand = new DelegateCommand(ExecuteUpdateCommand));

        private DelegateCommand _addCommand;
        public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(ExecuteAddCommand));

        private readonly MotorUseCase _motorUseCase;
        private readonly IMapper _mapper;
        private void ExecuteAddCommand() {
            var map = _mapUseCase.AddMap();

            Maps.Clear();
            Maps.AddRange(
                _mapUseCase.GetMaps()
                    .Select(e => _mapper.Map<NLinearMapState>(e))
                    );
        }

        void ExecuteUpdateCommand() {
            SelectedMap.Targets = ValidTargets.Where(e => e.IsSelected).ToArray();

            _mapUseCase.UpdateMap(_mapper.Map<NLinearMapEntity>(SelectedMap));
        }

        public TargetsPanelViewModel(IEnumerable<IUseCase> useCases, MapperConfiguration config) {
            _useCase = useCases.OfType<ListTargetUseCase>().FirstOrDefault();
            _mapUseCase = useCases.OfType<NLinearMapUseCase>().FirstOrDefault();
            _motorUseCase = useCases.OfType<MotorUseCase>().FirstOrDefault();

            _mapper = config.CreateMapper();

            ValidTargets = _useCase.GetTargets()
                .Select(e => {
                    if (e is MotorEntity) {
                        return _mapper.Map<MotorTargetState>(e);
                    }
                    else if (e is ControlUiEntity) {
                        return _mapper.Map<ControlUiTargetState>(e);
                    }
                    else
                        return default(ISelectableState);
                }).ToArray();

            Maps.AddRange(
                _mapUseCase.GetMaps()
                    .Select(e => _mapper.Map<NLinearMapState>(e))
                    );
        }
    }
}
