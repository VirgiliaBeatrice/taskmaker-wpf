using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PCController;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Services;
using taskmaker_wpf.Views.Widget;

namespace taskmaker_wpf.ViewModels {
    public struct NodeState {
        private int _id;
        private Point _value = new Point();

        public int Id { get => _id; set => _id = value; }
        public Point Value { get => _value; set => _value = value; }

        public NodeState(int id, Point value) {
            Id = id;
            Value = value;
        }

        public override bool Equals(object obj) {
            if (obj == null || this.GetType() != obj.GetType()) {
                return false;
            }

            var other = (NodeState)obj;
            return this.Id == other.Id;
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public static bool operator ==(NodeState left, NodeState right) {
            return left.Equals(right);
        }

        public static bool operator !=(NodeState left, NodeState right) {
            return !(left == right);
        }
    }


    public partial class ControlUiCollectionViewModel : ObservableObject {
        private readonly UIService _uiSrv;

        public ObservableCollection<ControlUiViewModel> VMs { get; private set; } = new();

        public ControlUiCollectionViewModel(UIService uiSrv) {
            _uiSrv = uiSrv;
        }

        [RelayCommand]
        public void Fetch() {
            VMs.Clear();
            foreach(var entity in _uiSrv.GetAll()) {
                var vm = new ControlUiViewModel(_uiSrv);

                vm.FromEntity(entity);
                VMs.Add(vm);
            }
        }


        [RelayCommand]
        public void Add() {
            var entity = new ControlUiEntity();
            var vm = new ControlUiViewModel(_uiSrv);

            _uiSrv.Create(entity);
            vm.FromEntity(entity);
            VMs.Add(vm);
        }
    }

    public class UiViewModelNodeAddedMessage {
        public ControlUiViewModel ViewModel { get; init; }
    }

    public class UiViewModelNodeDeletedMessage {
        public ControlUiViewModel ViewModel { get; init; }
        public int NodeIndex { get; init; }
    }

    public class UiViewModelNodeUpdatedMessage {
        public ControlUiViewModel ViewModel { get; init; }
    }


    public partial class ControlUiViewModel : ObservableObject {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private BaseRegionState[] _regionStates = Array.Empty<BaseRegionState>();
        [ObservableProperty]
        private NodeState[] _nodeStates = Array.Empty<NodeState>();
        [ObservableProperty]
        private Point _input = new Point();
        [ObservableProperty]
        private BaseRegionState _hitRegion;

        // TODO: unify to service
        private readonly UIService _uiSrv;
        private ControlUiEntity _entity;

        partial void OnIdChanged(int value) {
            if (value != -1) {
                _entity = _uiSrv.Read(value);
            }
        }

        public ControlUiViewModel(UIService uiSrv) {
            _uiSrv = uiSrv;
        }

        [RelayCommand]
        public void AddNode(Point position) {
            var nodeEntity = _entity.Create();
            nodeEntity.Value = position;

            NodeStates = _entity.Nodes.Select(node => new NodeState(node.Id, node.Value)).ToArray();

            if (NodeStates.Length >= 3) {
                _entity.Build();
                RegionStates = MapFromRegionEntities();
            }

            WeakReferenceMessenger.Default.Send(new UiViewModelNodeAddedMessage() { ViewModel = this });
        }

        [RelayCommand]
        public void UpdateNode(NodeState nodeState) {
            _entity.Update(new NodeEntity() { Id = nodeState.Id, Value = nodeState.Value });

            NodeStates = _entity.Nodes.Select(node => new NodeState(node.Id, node.Value)).ToArray();

            if (NodeStates.Length >= 3) {
                _entity.Build();
                RegionStates = MapFromRegionEntities();
            }
        }


        [RelayCommand]
        public void DeleteNode(NodeState nodeState) {
            var index = Array.FindIndex(NodeStates, node => node.Id == nodeState.Id);

            _entity.Delete(nodeState.Id);

            NodeStates = _entity.Nodes.Select(node => new NodeState(node.Id, node.Value)).ToArray();

            if (NodeStates.Length >= 3) {
                _entity.Build();
                RegionStates = MapFromRegionEntities();
            }

            WeakReferenceMessenger.Default.Send(new UiViewModelNodeDeletedMessage() { ViewModel = this, NodeIndex = index });
        }

        private BaseRegionState[] MapFromRegionEntities() {
            // create RegionStates from _entity.Regions
            var regions = new List<BaseRegionState>();

            foreach (var entity in _entity.Regions) {
                if (entity is SimplexRegionEntity) {
                    regions.Add(BaseRegionState.Create<SimplexRegionEntity, SimplexState>(entity as SimplexRegionEntity));
                }
                else if (entity is VoronoiRegionEntity) {
                    regions.Add(BaseRegionState.Create<VoronoiRegionEntity, VoronoiState>(entity as VoronoiRegionEntity));
                }
                else
                    throw new InvalidOperationException($"Unsupported entity type: {entity.GetType()}");
            }

            return regions.ToArray();
        }

        public void FromEntity(ControlUiEntity entity) {
            Id = entity.Id;
            Name = entity.Name;
            NodeStates = entity.Nodes.Select(node => new NodeState(node.Id, node.Value)).ToArray();
        }

        public override string ToString() {
            return Name;
        }
    }

}