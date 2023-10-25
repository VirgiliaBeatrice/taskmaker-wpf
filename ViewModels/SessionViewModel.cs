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

        public ObservableCollection<ControlUiViewModel> Uis { get; init; } = new ObservableCollection<ControlUiViewModel>();

        public NodeState[][] UiVMNodeStates => Uis.Select(e => e.NodeStates).ToArray();

        [ObservableProperty]
        private NLinearMapViewModel _map;
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
        private MapEntry[] _entries;
        [ObservableProperty]
        private Point[] _inputs;
        [ObservableProperty]
        private BaseRegionEntity[] _regions;

        private readonly SessionEntity _entity;

        public SessionViewModel(SessionEntity entity) {
            _entity = entity;

            WeakReferenceMessenger.Default.Register<UiViewModelNodeAddedMessage>(this, (r, m) => {
                var uiVM = m.ViewModel;

                var axis = Uis.IndexOf(uiVM);
                var op = new TensorOperationRecord {
                    Axis = axis + 1,
                    Index = uiVM.NodeStates.Length - 1
                };

                Map.ExpandAt(op);
            });

            WeakReferenceMessenger.Default.Register<UiViewModelNodeDeletedMessage>(this, (r, m) => {
                var uiVM = m.ViewModel;

                var axis = Uis.IndexOf(uiVM);
                var op = new TensorOperationRecord {
                    Axis = axis + 1,
                    Index = m.NodeIndex
                };

                Map.RemoveAt(op);
            });

            WeakReferenceMessenger.Default.Register<UiViewModelNodeUpdatedMessage>(this, (r, m) => {
                var uiVM = m.ViewModel;


            });

            Fetch();
        }

        partial void OnSelectedNodeStatesChanged(NodeState[] value) {
            SelectedNodeIds = SelectedNodeStates.Select(e => e.Id).ToArray();
            SelectedNodeIndices = SelectedNodeStates.Select(e => Array.FindIndex(UiVMNodeStates[Array.IndexOf(SelectedNodeStates, e)], n => n.Id == e.Id)).ToArray();
        }


        [RelayCommand]
        public void SetValue(double[] values) {
            //for (int i = 0; i < UiViewModels.Count; i++) {
            //    var selectedNode = SelectedNodeStates[i];
            //    var selectedNodeIndex = Array.FindIndex(UiVMNodeStates[i], e => e.Id == selectedNode.Id);

            //    SelectedNodeIndices[i] = selectedNodeIndex;
            //    SelectedNodeIds[i] = selectedNode.Id;
            //}

            var mapEntry = new MapEntry {
                IDs = SelectedNodeIds,
                Indices = SelectedNodeIndices,
                Value = values
            };

            Map.SetValue(mapEntry);
            Fetch();
            //SetEntry(mapEntry);
        }

        [RelayCommand]
        public void GetValue() {
            var mapEntry = new MapEntry {
                IDs = SelectedNodeIds,
                Indices = SelectedNodeIndices,
            };

            Map.GetValue(ref mapEntry);
            Fetch();
            //SetEntry(mapEntry);
        }

        [RelayCommand]
        public void Fetch() {
            // Update all properties from entity
            Id = _entity.Id;
            Name = _entity.Name;

            Uis.Clear();
            foreach (var entity in _entity.Uis) {
                var uiVM = new ControlUiViewModel(entity) {
                    NodeStates = entity.Nodes.Select(e => new NodeState(e.Id, e.Value)).ToArray()
                };

                Uis.Add(uiVM);
            }

            Map = new NLinearMapViewModel(_entity.Map);
            Entries = Map.MapEntries.ToArray();




            //var dim = MapViewModel.Dimension;
            //if (dim == 0) return;

            //var shape = MapViewModel.Shape;
            //var entity = MapViewModel.Entity;

            //var indices = Enumerable.Range(1, dim - 1)
            //    .Select(i => Enumerable.Range(0, shape[i]).ToArray())
            //    .CartesianProduct()
            //    .Select(e => e.ToArray())
            //    .ToArray();

            //var values = indices.Select(e => entity.GetValue(new[] { -1 }.Concat(e).ToArray())).ToArray();

            //var entries = new List<MapEntry>();
            //for (int i = 0; i < indices.Length; i++) {
            //    entries.Add(new MapEntry {
            //        IDs = SelectedNodeIds,
            //        Indices = indices[i],
            //        Value = values[i]
            //    });
            //}

            //Entries = entries;
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