using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Views.Widget;

namespace taskmaker_wpf.ViewModels
{
    public partial class SessionViewModel : ObservableObject {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private NodeState[] _selectedNodeStates;
        [ObservableProperty]
        private int[] _selectedNodeIndices;
        [ObservableProperty]
        private int[] _selectedNodeIds;
        [ObservableProperty]
        private Point[] _inputCollection;
        [ObservableProperty]
        private NLinearMapViewModel _map;
        public ObservableCollection<ControlUiViewModel> Uis { get; init; } = new();
        public NodeState[][] NodeCollections => Uis.Select(e => e.NodeStates).ToArray();


        private readonly SessionEntity _entity;

        public SessionViewModel(SessionEntity entity) {
            _entity = entity;

            WeakReferenceMessenger.Default.Register<UiViewModelNodeAddedMessage>(this, (r, m) => {
                var uiVM = m.Ui;

                var axis = Uis.IndexOf(uiVM);
                var op = new TensorOperationRecord {
                    Axis = axis + 1,
                    Index = uiVM.NodeStates.Length - 1
                };

                Map.ExpandAt(op);
            });

            WeakReferenceMessenger.Default.Register<UiViewModelNodeDeletedMessage>(this, (r, m) => {
                var uiVM = m.Ui;

                var axis = Uis.IndexOf(uiVM);
                var op = new TensorOperationRecord {
                    Axis = axis + 1,
                    Index = m.NodeIndex
                };

                Map.RemoveAt(op);
            });

            WeakReferenceMessenger.Default.Register<UiViewModelNodeUpdatedMessage>(this, (r, m) => {
                var uiVM = m.Ui;


            });

            Fetch();
        }

        partial void OnSelectedNodeStatesChanged(NodeState[] value) {
            SelectedNodeIds = SelectedNodeStates.Select(e => e.Id).ToArray();
            SelectedNodeIndices = SelectedNodeStates.Select(e => Array.FindIndex(NodeCollections[Array.IndexOf(SelectedNodeStates, e)], n => n.Id == e.Id)).ToArray();
        }


        [RelayCommand]
        public void SetValue(double[] values) {
            var mapEntry = new MapEntry {
                IDs = SelectedNodeIds,
                Indices = SelectedNodeIndices,
                Value = values
            };

            Map.SetValue(mapEntry);
            Fetch();
        }

        [RelayCommand]
        public void GetValue() {
            var mapEntry = new MapEntry {
                IDs = SelectedNodeIds,
                Indices = SelectedNodeIndices,
            };

            Map.GetValue(ref mapEntry);
            Fetch();
        }

        [RelayCommand]
        public void Fetch() {
            // Update all properties from entity
            // Update basic properties
            Id = _entity.Id;
            Name = _entity.Name;

            // Update Uis
            Uis.Clear();
            foreach (var entity in _entity.Uis) {
                var uiVM = new ControlUiViewModel(entity) {
                    NodeStates = entity.Nodes.Select(e => new NodeState(e.Id, e.Value)).ToArray()
                };

                Uis.Add(uiVM);
            }

            // Update Map
            Map = new NLinearMapViewModel(_entity.Map);
        }

        [RelayCommand]
        public void Interpolate() {
            var lambdasCollection = new List<double[]>();
            for (int i = 0; i < Uis.Count; i++) {
                var nodes = Uis[i].NodeStates;
                var input = Uis[i].Input;
                var region = Uis[i].HitRegion;

                if (region == null) return;

                var indices = region.Vertices.Select(e => Array.FindIndex(nodes, n => n.Id == e.Id)).ToArray();
                var lambdas = BaseRegionState.GetLambdas(region.Vertices.Select(e => e.Value).ToArray(), input, indices, nodes.Length);

                lambdasCollection.Add(lambdas);
            }

            Map.Interpolate(lambdasCollection.ToArray());
        }

        public override string ToString() {
            return Name;
        }
    }
}