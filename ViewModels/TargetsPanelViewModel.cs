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
using Prism.Events;

namespace taskmaker_wpf.ViewModels {
    public interface ISelectableStateNew {
        int id { get; set; }
        string Name { get; set; }
        bool IsSelected { get; set; }

    }

    public class SelectableSourceState : BindableBase {
        private bool isSelected;
        private string name;
        private int id;

        public int Id { get => id; set => SetProperty(ref id, value); }
        public string Name { get => name; set => SetProperty(ref name, value); }
        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected,value); }

        public override string ToString() {
            return Name;
        }
    }

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

        private int[] _shape;
        public int[] Shape {
            get => _shape;
            set => SetProperty(ref _shape, value);
        }

        private int[] _currentPosition;
        public int[] CurrentPosition {
            get => _currentPosition;
            set => SetProperty(ref _currentPosition, value);
        }

        private string[] _targets;
        public string[] Targets {
            get => _targets;
            set => SetProperty(ref _targets, value);
        }

        public override string ToString() {
            return $"Map[{Name}]";
        }
    }

    public class TargetsPanelViewModel : BindableBase {
        private readonly ListTargetUseCase _useCase;
        private readonly NLinearMapUseCase _mapUseCase;

        private ObservableCollection<int[]> _indices;
        public ObservableCollection<int[]> Indices {
            get => _indices;
            set => SetProperty(ref _indices, value);
        }

        private int[] _basisDim;
        public int[] BasisDim {
            get => _basisDim;
            set => SetProperty(ref _basisDim, value);
        }

        private SelectableSourceState[] _sourceState;
        public SelectableSourceState[] SourceStates {
            get => _sourceState;
            set => SetProperty(ref _sourceState, value);
        }

        private SelectableSourceState _selectedSourceState;
        public SelectableSourceState SelectedSourceState {
            get => _selectedSourceState;
            set => SetProperty(ref _selectedSourceState, value);
        }

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
            set {
                SetProperty(ref _selectedMap, value);
                
                if (SelectedMap != null) {
                    BasisDim = Maps.Where(e => e.Id == SelectedMap.Id).FirstOrDefault().Shape.Skip(1).ToArray();
                    //TargetsOfSelectedMap = SelectedMap.Targets.Select(
                    //    e => {
                    //        var context = e.Split('/');
                    //        var target = ValidTargets.Where(t => t.GetType().Name == context[0] & t.Id == int.Parse(context[1])).FirstOrDefault();

                    //        return target;
                    //    }).ToArray();
                }
            }
        }

        private DelegateCommand _addCommand;
        public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(ExecuteAddCommand));
        private void ExecuteAddCommand() {
            var map = _mapUseCase.AddMap();

            Maps.Clear();
            Maps.AddRange(
                _mapUseCase.GetMaps()
                    .Select(e => _mapper.Map<NLinearMapState>(e))
                    );
        }

        //private DelegateCommand _updateCommand;
        //public DelegateCommand UpdateCommand =>
        //    _updateCommand ?? (_updateCommand = new DelegateCommand(ExecuteUpdateCommand));
        //void ExecuteUpdateCommand() {
        //    var map = _mapper.Map<NLinearMapEntity>(SelectedMap);

        //    map.SetTargets(ValidTargets.Where(e => e.IsSelected).Select(e => _mapper.Map<ITargetable>(e)));
        //    _mapUseCase.UpdateMap(map);

            //var 

            //SelectedMap.Targets = ValidTargets.Where(e => e.IsSelected).Select(e => e.GetType().Name + "/" + e.Id).ToArray();
            //TargetsOfSelectedMap = SelectedMap.Targets.Select(
            //    e => {
            //        var context = e.Split('/');
            //        var target = ValidTargets.Where(t => t.GetType().Name == context[0] & t.Id == int.Parse(context[1])).FirstOrDefault();

            //        return target;
            //    }).ToArray();

            //_mapUseCase.UpdateMap(_mapper.Map<NLinearMapEntity>(SelectedMap));
            //_mapUseCase.SetMapTargetDim(SelectedMap.Id, SelectedMap.Targets.Select(e => e.Length).Sum());
        //}

        private DelegateCommand _initializeCommand;
        public DelegateCommand InitilizeCommand =>
            _initializeCommand ?? (_initializeCommand = new DelegateCommand(ExecuteInitilizeCommand));

        void ExecuteInitilizeCommand() {
            var sourceIds = SourceStates.Where(e => e.IsSelected).Select(e => e.Id).ToArray();
            var targetIds = ValidTargets.Where(e => e.IsSelected).Select(e => e.Id).ToArray();

            _mapUseCase.InitializeMap(SelectedMap.Id, sourceIds, targetIds);

            Invalidate();

            Indices = new ObservableCollection<int[]>();
            Indices.AddRange(_mapUseCase.GetTensorIndices(SelectedMap.Id));
            //Indices.RemoveAt(0);
            //_mapUseCase.GetMaps()
            //    .ToList()
            //    .ForEach(e => {
            //        var id = e.Id;
            //        var destMapState = Maps.Where(m => m.Id == id).FirstOrDefault();

            //        _mapper.Map<NLinearMapEntity, NLinearMapState>(e, destMapState);
            //    });

            //BasisDim = SelectedMap.Shape.Skip(1).ToArray();
            //Maps.Clear();
            //Maps.AddRange(
            //    _mapUseCase.GetMaps()
            //        .Select(e => _mapper.Map<NLinearMapState>(e)));
        }

        private void Invalidate() {
            _mapUseCase.GetMaps()
                .ToList()
                .ForEach(e => {
                    var id = e.Id;
                    var destMapState = Maps.Where(m => m.Id == id).FirstOrDefault();

                    _mapper.Map(e, destMapState);
                });
            
            BasisDim = SelectedMap.Shape.Skip(1).ToArray();
        }

        private DelegateCommand _setValueCommand;
        public DelegateCommand SetValueCommand =>
            _setValueCommand ?? (_setValueCommand = new DelegateCommand(ExecuteSetValueCommand));

        void ExecuteSetValueCommand() {
            var mapId = SelectedMap.Id;
            var values = ValidTargets.Where(e => e.IsSelected)
                .OfType<MotorTargetState>()
                .SelectMany(e => _motorUseCase.GetMotor(e.Id).Value)
                .ToArray();
            var indices = Indices[0];

            _mapUseCase.UpdateMapValue(mapId, indices, values);

            Indices.RemoveAt(0);

            Invalidate();
        }


        private readonly MotorUseCase _motorUseCase;
        private readonly ControlUiUseCase _uiUseCase;
        private readonly IMapper _mapper;
        private readonly IEventAggregator _ea;  

        public TargetsPanelViewModel(
            IEnumerable<IUseCase> useCases,
            MapperConfiguration config
            ) {
            _useCase = useCases.OfType<ListTargetUseCase>().FirstOrDefault();
            _mapUseCase = useCases.OfType<NLinearMapUseCase>().FirstOrDefault();
            _motorUseCase = useCases.OfType<MotorUseCase>().FirstOrDefault();
            _uiUseCase = useCases.OfType<ControlUiUseCase>().FirstOrDefault();


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

            ValidTargets.OfType<MotorTargetState>().ToList().ForEach(e => e.PropertyChanged += (s, args) => {
                if (args.PropertyName == nameof(MotorTargetState.MotorValue))
                    _motorUseCase.UpdateMotor(_mapper.Map<MotorEntity>(s));
            });

            Maps.AddRange(
                _mapUseCase.GetMaps()
                    .Select(e => _mapper.Map<NLinearMapState>(e)));

            SourceStates = _uiUseCase.GetControlUis()
                .Select(e => _mapper.Map<SelectableSourceState>(e))
                .ToArray();
        }
    }
}
