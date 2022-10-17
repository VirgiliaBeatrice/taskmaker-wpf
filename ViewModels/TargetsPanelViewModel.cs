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
    public interface ISelectableState {
        bool IsSelected { get; set; }
        string Name { get; }
        int Id { get; }
        double[] Value { get; }
        event PropertyChangedEventHandler PropertyChanged;
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
        private int _id;
        public int Id {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name;
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string[] _targets;
        public string[] Targets {
            get => _targets;
            set => SetProperty(ref _targets, value);
        }

    }

    public class TargetsPanelViewModel : BindableBase, IPresenter {
        private readonly ListTargetInteractor _target;
        private readonly NLinearMapInteractorBus _mapBus;
        private readonly MotorInteractorBus _motorBus;
        //private readonly ListTargetUseCase _useCase;
        //private readonly NLinearMapUseCase _mapUseCase;

        private ISelectableState[] _validTargets;
        public ISelectableState[] ValidTargets {
            get => _validTargets.OrderBy(e => e.Name).ToArray();
            set => SetProperty(ref _validTargets, value);
        }

        public ISelectableState[] _targetsOfSelectedMap;
        public ISelectableState[] TargetsOfSelectedMap {
            get => _targetsOfSelectedMap;
            set => SetProperty(ref _targetsOfSelectedMap, value);
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

        private DelegateCommand _addCommand;
        public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(ExecuteAddCommand));
        private void ExecuteAddCommand() {
            _mapBus.Handle<AddNLinearMapRequest, bool>(new AddNLinearMapRequest(), (e) => { });
            _mapBus.Handle<ListNLinearMapRequest, NLinearMapEntity[]>(new ListNLinearMapRequest(),
                UpdateMaps);
        }

        private void UpdateMaps(NLinearMapEntity[] maps) {
            Maps.Clear();
            Maps.AddRange(maps.Select(e => _mapper.Map<NLinearMapState>(e)));
        }

        private DelegateCommand _updateCommand;
        public DelegateCommand UpdateCommand =>
            _updateCommand ?? (_updateCommand = new DelegateCommand(ExecuteUpdateCommand));
        void ExecuteUpdateCommand() {
            //SelectedMap.Targets = ValidTargets.Where(e => e.IsSelected).Select(e => e.GetType().Name + "/" + e.Id).ToArray();
            //TargetsOfSelectedMap = SelectedMap.Targets.Select(
            //    e => {
            //        var context = e.Split('/');
            //        var target = ValidTargets.Where(t => t.GetType().Name == context[0] & t.Id == int.Parse(context[1])).FirstOrDefault();

            //        return target;
            //    }).ToArray();

            //_mapUseCase.UpdateMap(_mapper.Map<NLinearMapEntity>(SelectedMap));
            //_mapUseCase.SetMapTargetDim(SelectedMap.Id, SelectedMap.Targets.Select(e => e.Length).Sum());
        }

        //private readonly MotorUseCase _motorUseCase;
        private readonly IMapper _mapper;
        public TargetsPanelViewModel(
            ListTargetInteractor target,
            MotorInteractorBus motorBus,
            NLinearMapInteractorBus mapBus,
            //IEnumerable<IUseCase> useCases,
            MapperConfiguration config) {
            //_useCase = useCases.OfType<ListTargetUseCase>().FirstOrDefault();
            //_mapUseCase = useCases.OfType<NLinearMapUseCase>().FirstOrDefault();
            //_motorUseCase = useCases.OfType<MotorUseCase>().FirstOrDefault();
            _target = target;
            _motorBus = motorBus;
            _mapBus = mapBus;

            _mapper = config.CreateMapper();

            _target.Handle<ListTargetRequest, ITargetable[]>(new ListTargetRequest(), targets => {
                targets.Select(e => {
                    if (e is MotorEntity) {
                        return _mapper.Map<MotorTargetState>(e);
                    }
                    else if (e is ControlUiEntity) {
                        return _mapper.Map<ControlUiTargetState>(e);
                    }
                    else
                        return default(ISelectableState);
                }).ToArray();
            });

            _mapBus.Handle<ListNLinearMapRequest, NLinearMapEntity[]>(new ListNLinearMapRequest(), UpdateMaps);

            //ValidTargets.OfType<MotorTargetState>().ToList().ForEach(e => e.PropertyChanged += (s, args) => {
            //    if (args.PropertyName == nameof(MotorTargetState.MotorValue))
            //        _motorUseCase.UpdateMotor(_mapper.Map<MotorEntity>(s));
            //});
        }
    }
}
