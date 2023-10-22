using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        private readonly ControlUiEntity _entity;

        public ControlUiViewModel(ControlUiEntity entity) {
            _entity = entity;
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
            _entity.Delete(nodeState.Id);

            NodeStates = _entity.Nodes.Select(node => new NodeState(node.Id, node.Value)).ToArray();

            if (NodeStates.Length >= 3) {
                _entity.Build();
                RegionStates = MapFromRegionEntities();
            }
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

        public override string ToString() {
            return Name;
        }
    }

}