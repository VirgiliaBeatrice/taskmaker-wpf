using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using PCController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Services;
using taskmaker_wpf.Views.Widget;

namespace taskmaker_wpf.ViewModels {
    public class NodeUpdatedMessage {
        public NodeViewModel Sender { get; init; }
    }

    // TODO: use node as data binding
    public partial class NodeViewModel : ObservableObject {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private Point _value;

        partial void OnValueChanged(Point value) {
            _entity.Value = value;

            EventDispatcher.Record(new CreationMoveEvent());
            WeakReferenceMessenger.Default.Send(new NodeUpdatedMessage() { Sender = this });
        }

        private readonly NodeEntity _entity;

        public NodeViewModel(NodeEntity entity) {
            _entity = entity;

            //FetchThis();

            _id = _entity.Id;
            _value = _entity.Value;
        }

        [RelayCommand]
        public void FetchThis() {
            Id = _entity.Id;
            Value = _entity.Value;
        }

        public override bool Equals(object obj) {
            if (obj == null || this.GetType() != obj.GetType()) {
                return false;
            }

            var other = (NodeViewModel)obj;
            return this.Id == other.Id;
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public static bool operator ==(NodeViewModel left, NodeViewModel right) {
            return left.Equals(right);
        }

        public static bool operator !=(NodeViewModel left, NodeViewModel right) {
            return !(left == right);
        }
    }


    public partial class ControlUiCollectionViewModel : ObservableObject {
        private readonly UIService _uiSrv;
        private readonly ControlUiEntity _entity;

        public ObservableCollection<ControlUiViewModel> Uis { get; private set; } = new();

        public ControlUiCollectionViewModel(UIService uiSrv) {
            _uiSrv = uiSrv;
        }

        [RelayCommand]
        public void Fetch() {
            Uis.Clear();
            foreach(var entity in _uiSrv.GetAll()) {
                var vm = new ControlUiViewModel(entity);

                Uis.Add(vm);
            }
        }


        [RelayCommand]
        public void Add() {
            var entity = new ControlUiEntity();
            var vm = new ControlUiViewModel(entity);

            _uiSrv.Create(entity);
            Uis.Add(vm);
        }
    }

    public class UiViewModelNodeAddedMessage {
        public ControlUiViewModel Ui { get; init; }
    }

    public class UiViewModelNodeDeletedMessage {
        public ControlUiViewModel Ui { get; init; }
        public int NodeIndex { get; init; }
    }

    public class UiViewModelNodeUpdatedMessage {
        public ControlUiViewModel Ui { get; init; }
    }


    public partial class ControlUiViewModel : ObservableObject {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private BaseRegionState[] _regionStates = Array.Empty<BaseRegionState>();
        [ObservableProperty]
        private Point _input = new();
        [ObservableProperty]
        private BaseRegionState _hitRegion;
        [ObservableProperty]
        private UiMode _mode = UiMode.Default;

        partial void OnModeChanged(UiMode oldValue, UiMode newValue) {
            if (oldValue != UiMode.Default && newValue == UiMode.Default) {
                _logger.Debug("Back to default mode.");
                _logger.Debug("Try to rebuild.");

                Fetch();
            }
        }

        public ObservableCollection<NodeViewModel> NodeStates { get; set; } = new ObservableCollection<NodeViewModel>();
        private readonly ControlUiEntity _entity;

        public ControlUiViewModel(ControlUiEntity entity) {
            _entity = entity;

            PropertyChanged += ControlUiViewModel_PropertyChanged;

            WeakReferenceMessenger.Default.Register<NodeUpdatedMessage>(this, (r, m) => {
                FetchRegions();
            });

            Fetch();
        }

        private void ControlUiViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "HitRegion") {
                //_logger.Debug($"Hit changed {HitRegion}: {sender.GetHashCode()}");
            }
            else if (e.PropertyName == "Input") {
                //_logger.Debug($"Input: {Input}: {sender.GetHashCode()}");

            }
        }

        [RelayCommand]
        public void Fetch() {
            // update all properties from entity to this
            Id = _entity.Id;
            Name = _entity.Name;

            NodeStates.Clear();
            foreach(var item in _entity.Nodes.Select(e => new NodeViewModel(e))) {
                NodeStates.Add(item);
            }

            FetchRegions();
        }


        [RelayCommand]
        public void AddNode(Point position) {
            var nodeEntity = _entity.Create();
            nodeEntity.Value = position;

            NodeStates.Add(new NodeViewModel(nodeEntity));

            FetchRegions();

            WeakReferenceMessenger.Default.Send(new UiViewModelNodeAddedMessage() { Ui = this });
            EventDispatcher.Record(new CreationAddEvent());
        }

        [RelayCommand]
        public void Rebuild() {
            Fetch();
        }

        public void FetchRegions() {
            if (NodeStates.Count >= 3) {
                _entity.Build();
                RegionStates = BuildRegions();
            }
            else {
                RegionStates = Array.Empty<BaseRegionState>();
            }
        }

        [RelayCommand]
        public void DeleteNode(NodeViewModel nodeState) {
            var index = Array.FindIndex(NodeStates.ToArray(), node => node.Id == nodeState.Id);

            _entity.Delete(nodeState.Id);

            NodeStates.Remove(nodeState);

            FetchRegions();

            WeakReferenceMessenger.Default.Send(new UiViewModelNodeDeletedMessage() { Ui = this, NodeIndex = index });
            EventDispatcher.Record(new CreationDeleteEvent());
        }

        public BaseRegionState[] BuildRegions() {
            var regionStates = new List<BaseRegionState>();

            foreach(var region in _entity.Regions) {
                if (region is SimplexRegionEntity s) {
                    regionStates.Add(new SimplexState {
                        Id = s.Id,
                        Name = s.Name,
                        Vertices = s.Vertices.Select(v => NodeStates.First(n => n.Id == v.Id)).ToArray(),
                    });
                }
                else if (region is VoronoiRegionEntity vo) {
                    regionStates.Add(new VoronoiState {
                        Id = vo.Id,
                        Name = vo.Name,
                        Vertices = vo.Vertices.Select(vec => NodeStates.First(n => n.Id == vec.Id)).ToArray(),
                    });
                }
                else
                    throw new InvalidOperationException($"Unsupported entity type: {region.GetType()}");
            }

            return regionStates.ToArray();
        }

        public override string ToString() {
            return Name;
        }
    }

}